using MegaAnalyzer.Models;

namespace MegaAnalyzer.Services.Analysis;

public class ResultadoCiclo
{
    public int Numero { get; set; }
    public double MediaCiclo { get; set; } // média de sorteios entre aparições
    public int MinCiclo { get; set; }
    public int MaxCiclo { get; set; }
    public List<int> Ciclos { get; set; } = new(); // histórico de intervalos
    public string Perfil => MediaCiclo switch
    {
        <= 7  => "Ciclo Curto",
        <= 12 => "Ciclo Normal",
        _     => "Ciclo Longo"
    };
}

public class AnaliseCiclo
{
    public List<ResultadoCiclo> Analisar(List<Sorteio> sorteios)
    {
        var ordenados = sorteios.OrderBy(s => s.Concurso).ToList();
        var resultado = new List<ResultadoCiclo>();

        for (int n = 1; n <= 60; n++)
        {
            var aparicoes = ordenados
                .Select((s, idx) => new { Sorteio = s, Indice = idx })
                .Where(x => x.Sorteio.Numeros.Any(num => num.Valor == n))
                .Select(x => x.Indice)
                .ToList();

            if (aparicoes.Count < 2)
            {
                resultado.Add(new ResultadoCiclo
                {
                    Numero = n,
                    MediaCiclo = 0,
                    MinCiclo = 0,
                    MaxCiclo = 0
                });
                continue;
            }

            var ciclos = new List<int>();
            for (int i = 1; i < aparicoes.Count; i++)
                ciclos.Add(aparicoes[i] - aparicoes[i - 1]);

            resultado.Add(new ResultadoCiclo
            {
                Numero = n,
                MediaCiclo = Math.Round(ciclos.Average(), 1),
                MinCiclo = ciclos.Min(),
                MaxCiclo = ciclos.Max(),
                Ciclos = ciclos
            });
        }

        return resultado.OrderBy(r => r.MediaCiclo).ToList();
    }

    public List<ResultadoCiclo> CicloCurto(List<ResultadoCiclo> ciclos)
        => ciclos.Where(c => c.MediaCiclo > 0 && c.MediaCiclo <= 7)
                 .OrderBy(c => c.MediaCiclo).ToList();

    public List<ResultadoCiclo> CicloLongo(List<ResultadoCiclo> ciclos)
        => ciclos.Where(c => c.MediaCiclo > 12)
                 .OrderByDescending(c => c.MediaCiclo).ToList();
}