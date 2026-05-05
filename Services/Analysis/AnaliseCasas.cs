using MegaAnalyzer.Models;

namespace MegaAnalyzer.Services.Analysis;

public class ResultadoCasa
{
    public int Casa { get; set; }
    public int Minimo { get; set; }
    public int Maximo { get; set; }
    public double Media { get; set; }
    public List<int> Valores { get; set; } = new();
}

public class AnaliseCasas
{
    public List<ResultadoCasa> Analisar(List<Sorteio> sorteios)
    {
        var resultado = new List<ResultadoCasa>();

        for (int casa = 1; casa <= 6; casa++)
        {
            var valores = sorteios
                .SelectMany(s => s.Numeros)
                .Where(n => n.Posicao == casa)
                .Select(n => n.Valor)
                .ToList();

            if (!valores.Any()) continue;

            resultado.Add(new ResultadoCasa
            {
                Casa = casa,
                Minimo = valores.Min(),
                Maximo = valores.Max(),
                Media = Math.Round(valores.Average(), 1),
                Valores = valores
            });
        }

        return resultado;
    }

    public List<int> GerarNumeroPorCasa(List<ResultadoCasa> faixas)
    {
        var random = new Random();
        var jogo = new List<int>();

        foreach (var faixa in faixas)
        {
            int numero;
            int tentativas = 0;
            do
            {
                numero = random.Next(faixa.Minimo, faixa.Maximo + 1);
                tentativas++;
            } while (jogo.Contains(numero) && tentativas < 100);

            jogo.Add(numero);
        }

        return jogo.OrderBy(n => n).ToList();
    }
}