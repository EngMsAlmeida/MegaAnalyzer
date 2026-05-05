using HtmlAgilityPack;
using MegaAnalyzer.Data;
using MegaAnalyzer.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace MegaAnalyzer.Services;

public class ScrapingService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly HttpClient _http;

    public ScrapingService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async Task<List<Sorteio>> BuscarEAtualizarSorteiosAsync(int quantidade = 20)
{
    var url = "https://www.somatematica.com.br/megasenaResultados.php";
    var html = await _http.GetStringAsync(url);

    var doc = new HtmlDocument();
    doc.LoadHtml(html);

    // Pega texto limpo
    var body = doc.DocumentNode.InnerText;

    // Limpa caracteres especiais
    body = System.Text.RegularExpressions.Regex.Replace(body, @"\s+", " ");

    var sorteios = new List<Sorteio>();
    using var db = _dbFactory.CreateDbContext();

    // Divide por "Concurso"
    var blocos = body.Split("Concurso ", StringSplitOptions.RemoveEmptyEntries);

    foreach (var bloco in blocos.Skip(1).Take(quantidade))
    {
        try
        {
            // Extrai número do concurso e data
            var matchCabecalho = System.Text.RegularExpressions.Regex.Match(
                bloco, @"^(\d+) \(de (\d{2}/\d{2}/\d{4})\)");

            if (!matchCabecalho.Success) continue;

            var concurso = int.Parse(matchCabecalho.Groups[1].Value);
            var dataStr = matchCabecalho.Groups[2].Value;

            // Extrai os 6 números colados (cada um tem 2 dígitos)
            var resto = bloco[matchCabecalho.Length..].Trim();

            // Pega a sequência de 12 dígitos colados (6 números x 2 dígitos)
            var matchNums = System.Text.RegularExpressions.Regex.Match(resto, @"(\d{12})");
            if (!matchNums.Success) continue;

            var sequencia = matchNums.Groups[1].Value;

            // Divide em grupos de 2
            var numeros = Enumerable.Range(0, 6)
                .Select(i => int.Parse(sequencia.Substring(i * 2, 2)))
                .Where(n => n >= 1 && n <= 60)
                .OrderBy(n => n)
                .ToList();

            if (numeros.Count != 6) continue;

            if (await db.Sorteios.AnyAsync(s => s.Concurso == concurso))
                continue;

            var data = DateTime.ParseExact(dataStr, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture);

            var sorteio = new Sorteio
            {
                Concurso = concurso,
                Data = data,
                Numeros = numeros.Select((v, i) => new NumeroSorteado
                {
                    Valor = v,
                    Posicao = i + 1
                }).ToList()
            };

            db.Sorteios.Add(sorteio);
            sorteios.Add(sorteio);
        }
        catch
        {
            continue;
        }
    }

    await db.SaveChangesAsync();

    return await db.Sorteios
        .Include(s => s.Numeros)
        .OrderByDescending(s => s.Concurso)
        .Take(quantidade)
        .ToListAsync();
}
}