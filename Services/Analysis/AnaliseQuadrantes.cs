using MegaAnalyzer.Models;

namespace MegaAnalyzer.Services.Analysis;

public class ResultadoQuadrantes
{
    public int QtdA { get; set; }
    public int QtdB { get; set; }
    public int QtdC { get; set; }
    public int QtdD { get; set; }
    public int QtdBorda { get; set; }
    public int QtdCentro { get; set; }
    public string Padrao => $"A{QtdA}B{QtdB}C{QtdC}D{QtdD}";
}

public class AnaliseQuadrantes
{
    // Quadrante A: col 1-5, lin 1-3 = 01-05,11-15,21-25
    // Quadrante B: col 6-10, lin 1-3 = 06-10,16-20,26-30
    // Quadrante C: col 1-5, lin 4-6 = 31-35,41-45,51-55
    // Quadrante D: col 6-10, lin 4-6 = 36-40,46-50,56-60

    private static readonly HashSet<int> QuadranteA = new()
    {
        1,2,3,4,5,11,12,13,14,15,21,22,23,24,25
    };
    private static readonly HashSet<int> QuadranteB = new()
    {
        6,7,8,9,10,16,17,18,19,20,26,27,28,29,30
    };
    private static readonly HashSet<int> QuadranteC = new()
    {
        31,32,33,34,35,41,42,43,44,45,51,52,53,54,55
    };
    private static readonly HashSet<int> QuadranteD = new()
    {
        36,37,38,39,40,46,47,48,49,50,56,57,58,59,60
    };

    // Bordas: primeira e última linha + primeira e última coluna
    private static readonly HashSet<int> Bordas = new()
    {
        1,2,3,4,5,6,7,8,9,10,       // linha 1
        51,52,53,54,55,56,57,58,59,60, // linha 6
        11,21,31,41,                  // col 1
        20,30,40,50                   // col 10
    };

    public string ClassificarQuadrante(int numero)
    {
        if (QuadranteA.Contains(numero)) return "A";
        if (QuadranteB.Contains(numero)) return "B";
        if (QuadranteC.Contains(numero)) return "C";
        return "D";
    }

    public bool EhBorda(int numero) => Bordas.Contains(numero);

    public ResultadoQuadrantes AnalisarSorteio(List<int> numeros)
    {
        return new ResultadoQuadrantes
        {
            QtdA = numeros.Count(n => QuadranteA.Contains(n)),
            QtdB = numeros.Count(n => QuadranteB.Contains(n)),
            QtdC = numeros.Count(n => QuadranteC.Contains(n)),
            QtdD = numeros.Count(n => QuadranteD.Contains(n)),
            QtdBorda = numeros.Count(n => Bordas.Contains(n)),
            QtdCentro = numeros.Count(n => !Bordas.Contains(n))
        };
    }

    public Dictionary<string, int> ContarPadroes(List<Sorteio> sorteios)
    {
        var padroes = new Dictionary<string, int>();

        foreach (var sorteio in sorteios)
        {
            var numeros = sorteio.Numeros.Select(n => n.Valor).ToList();
            var resultado = AnalisarSorteio(numeros);
            var padrao = resultado.Padrao;

            if (!padroes.ContainsKey(padrao))
                padroes[padrao] = 0;
            padroes[padrao]++;
        }

        return padroes.OrderByDescending(p => p.Value)
                      .ToDictionary(p => p.Key, p => p.Value);
    }

    public Dictionary<int, int> ContarBordasPorSorteio(List<Sorteio> sorteios)
    {
        var contagem = new Dictionary<int, int>();
        for (int i = 0; i <= 6; i++) contagem[i] = 0;

        foreach (var sorteio in sorteios)
        {
            var qtd = sorteio.Numeros.Count(n => Bordas.Contains(n.Valor));
            contagem[qtd]++;
        }

        return contagem;
    }
}