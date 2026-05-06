using System.Text.Json;
using MegaAnalyzer.Data;
using MegaAnalyzer.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaAnalyzer.Services;

public class ImportacaoService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly HttpClient _http;

    public ImportacaoService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _http.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<(int importados, int ignorados)> ImportarHistoricoAsync(
        int concursoInicio,
        int concursoFim,
        IProgress<int>? progresso = null)
    {
        var importados = 0;
        var ignorados = 0;

        using var db = _dbFactory.CreateDbContext();

        for (int concurso = concursoInicio; concurso <= concursoFim; concurso++)
        {
            try
            {
                // Verifica se já existe
                if (await db.Sorteios.AnyAsync(s => s.Concurso == concurso))
                {
                    ignorados++;
                    progresso?.Report(concurso);
                    continue;
                }

                var url = $"https://servicebus2.caixa.gov.br/portaldeloterias/api/megasena/{concurso}";
                var json = await _http.GetStringAsync(url);
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Extrai data
                var dataStr = root.GetProperty("dataApuracao").GetString() ?? "";
                if (!DateTime.TryParse(dataStr, out var data))
                    data = DateTime.MinValue;

                // Extrai dezenas
                var dezenas = root
                    .GetProperty("listaDezenas")
                    .EnumerateArray()
                    .Select(d => int.Parse(d.GetString() ?? "0"))
                    .OrderBy(n => n)
                    .ToList();

                if (dezenas.Count != 6)
                {
                    ignorados++;
                    continue;
                }

                var sorteio = new Sorteio
                {
                    Concurso = concurso,
                    Data = data,
                    Numeros = dezenas.Select((v, i) => new NumeroSorteado
                    {
                        Valor = v,
                        Posicao = i + 1
                    }).ToList()
                };

                db.Sorteios.Add(sorteio);
                await db.SaveChangesAsync();
                importados++;
                progresso?.Report(concurso);

                // Pequena pausa pra não sobrecarregar a API
                await Task.Delay(100);
            }
            catch
            {
                ignorados++;
                continue;
            }
        }

        return (importados, ignorados);
    }

    public async Task<int> ObterUltimoConcursoAsync()
    {
        try
        {
            var url = "https://servicebus2.caixa.gov.br/portaldeloterias/api/megasena/";
            var json = await _http.GetStringAsync(url);
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("numero").GetInt32();
        }
        catch
        {
            return 3003; // fallback
        }
    }
}