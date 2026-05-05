using MegaAnalyzer.Models;

namespace MegaAnalyzer.Services.Analysis;

public class ResultadoParImpar
{
    public int Concurso { get; set; }
    public int Pares { get; set; }
    public int Impares { get; set; }
    public string Padrao => $"{Impares}I{Pares}P";
}

public class AnaliseParImpar
{
    public ResultadoParImpar AnalisarSorteio(Sorteio sorteio)
    {
        var numeros = sorteio.Numeros.Select(n => n.Valor).ToList();
        return new ResultadoParImpar
        {
            Concurso = sorteio.Concurso,
            Pares = numeros.Count(n => n % 2 == 0),
            Impares = numeros.Count(n => n % 2 != 0)
        };
    }

    public List<ResultadoParImpar> AnalisarTodos(List<Sorteio> sorteios)
    {
        return sorteios
            .Select(AnalisarSorteio)
            .ToList();
    }

    public Dictionary<string, int> ContarPadroes(List<Sorteio> sorteios)
    {
        return sorteios
            .Select(AnalisarSorteio)
            .GroupBy(r => r.Padrao)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());
    }
}