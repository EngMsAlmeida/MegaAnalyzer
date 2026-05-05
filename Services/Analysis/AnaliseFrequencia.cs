using MegaAnalyzer.Models;

namespace MegaAnalyzer.Services.Analysis;

public class ResultadoFrequencia
{
    public int Numero { get; set; }
    public int TotalSorteios { get; set; }
    public int Frequencia { get; set; }
    public double Percentual => Math.Round((double)Frequencia / TotalSorteios * 100, 1);
    public double FrequenciaEsperada => Math.Round((double)TotalSorteios * 6 / 60, 1);
    public double Desvio => Math.Round(Frequencia - FrequenciaEsperada, 1);
    public string Status => Desvio > 2 ? "Quente" : Desvio < -2 ? "Frio" : "Normal";
}

public class AnaliseFrequencia
{
    public List<ResultadoFrequencia> Analisar(List<Sorteio> sorteios)
    {
        var total = sorteios.Count;
        var resultado = new List<ResultadoFrequencia>();

        for (int n = 1; n <= 60; n++)
        {
            var freq = sorteios
                .Count(s => s.Numeros.Any(num => num.Valor == n));

            resultado.Add(new ResultadoFrequencia
            {
                Numero = n,
                TotalSorteios = total,
                Frequencia = freq
            });
        }

        return resultado.OrderBy(r => r.Numero).ToList();
    }

    public List<ResultadoFrequencia> MaisQuentes(List<ResultadoFrequencia> frequencias, int top = 15)
        => frequencias.OrderByDescending(f => f.Frequencia).Take(top).ToList();

    public List<ResultadoFrequencia> MaisFrios(List<ResultadoFrequencia> frequencias, int top = 15)
        => frequencias.OrderBy(f => f.Frequencia).Take(top).ToList();
}