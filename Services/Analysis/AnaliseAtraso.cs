using MegaAnalyzer.Models;

namespace MegaAnalyzer.Services.Analysis;

public class ResultadoAtraso
{
    public int Numero { get; set; }
    public int Atraso { get; set; } // quantos sorteios sem aparecer
    public DateTime? UltimaVez { get; set; }
    public int UltimoConcurso { get; set; }
    public string Status => Atraso switch
    {
        0 => "Saiu agora",
        <= 5 => "Recente",
        <= 10 => "Normal",
        <= 20 => "Atrasado",
        _ => "Muito atrasado"
    };
}

public class AnaliseAtraso
{
    public List<ResultadoAtraso> Analisar(List<Sorteio> sorteios)
    {
        // Ordena do mais recente pro mais antigo
        var ordenados = sorteios.OrderByDescending(s => s.Concurso).ToList();
        var resultado = new List<ResultadoAtraso>();

        for (int n = 1; n <= 60; n++)
        {
            var atraso = 0;
            DateTime? ultimaVez = null;
            var ultimoConcurso = 0;

            foreach (var sorteio in ordenados)
            {
                if (sorteio.Numeros.Any(num => num.Valor == n))
                {
                    ultimaVez = sorteio.Data;
                    ultimoConcurso = sorteio.Concurso;
                    break;
                }
                atraso++;
            }

            // Se nunca apareceu no histórico disponível
            if (ultimaVez == null)
                atraso = ordenados.Count;

            resultado.Add(new ResultadoAtraso
            {
                Numero = n,
                Atraso = atraso,
                UltimaVez = ultimaVez,
                UltimoConcurso = ultimoConcurso
            });
        }

        return resultado.OrderByDescending(r => r.Atraso).ToList();
    }
}