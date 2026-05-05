using MegaAnalyzer.Models;

namespace MegaAnalyzer.Services.Analysis;

public class JogoComScore
{
    public List<int> Dezenas { get; set; } = new();
    public double Score { get; set; }
    public Dictionary<string, double> Detalhes { get; set; } = new();
    public string DezenasFormatadas => string.Join(" - ", Dezenas.Select(d => d.ToString("D2")));
    public bool Selecionado { get; set; } = false;
}

public class GeradorJogos
{
    private readonly AnaliseCasas _casas = new();
    private readonly AnaliseQuadrantes _quadrantes = new();
    private readonly AnaliseParImpar _parImpar = new();
    private readonly AnaliseFrequencia _frequencia = new();
    private readonly AnaliseAtraso _atraso = new();
    private readonly Random _random = new();

    public List<JogoComScore> Gerar(
        List<Sorteio> sorteios,
        int quantidade = 20,
        int tentativas = 500)
    {
        // Prepara as análises
        var faixasCasas = _casas.Analisar(sorteios.Take(7).ToList());
        var padroesPorImpar = _parImpar.ContarPadroes(sorteios);
        var padraoDominante = padroesPorImpar.First().Key; // ex: "2I4P"
        var imparDominante = int.Parse(padraoDominante[0].ToString());
        var parDominante = int.Parse(padraoDominante[2].ToString());
        var frequencias = _frequencia.Analisar(sorteios);
        var atrasos = _atraso.Analisar(sorteios);
        var padraoQuadrante = _quadrantes.ContarPadroes(sorteios).First().Key;

        var jogos = new List<JogoComScore>();

        for (int t = 0; t < tentativas && jogos.Count < quantidade; t++)
        {
            var dezenas = GerarDezenas(faixasCasas);
            if (dezenas.Count != 6) continue;

            var score = CalcularScore(
                dezenas, sorteios, frequencias, atrasos,
                imparDominante, parDominante, padraoQuadrante);

            if (score.Score >= 40) // só aceita jogos com score mínimo
                jogos.Add(score);
        }

        return jogos.OrderByDescending(j => j.Score).Take(quantidade).ToList();
    }

    private List<int> GerarDezenas(List<ResultadoCasa> faixas)
    {
        var dezenas = new List<int>();
        foreach (var faixa in faixas)
        {
            int num;
            int t = 0;
            do { num = _random.Next(faixa.Minimo, faixa.Maximo + 1); t++; }
            while (dezenas.Contains(num) && t < 50);
            if (!dezenas.Contains(num)) dezenas.Add(num);
        }
        return dezenas.OrderBy(n => n).ToList();
    }

    private JogoComScore CalcularScore(
        List<int> dezenas,
        List<Sorteio> sorteios,
        List<ResultadoFrequencia> frequencias,
        List<ResultadoAtraso> atrasos,
        int imparEsperado, int parEsperado,
        string padraoQuadranteEsperado)
    {
        var detalhes = new Dictionary<string, double>();
        double total = 0;

        // 1. Par/Ímpar (peso 25)
        var impares = dezenas.Count(n => n % 2 != 0);
        var pares = dezenas.Count(n => n % 2 == 0);
        var scorePI = impares == imparEsperado ? 25 : Math.Max(0, 25 - Math.Abs(impares - imparEsperado) * 8);
        detalhes["Par/Ímpar"] = scorePI;
        total += scorePI;

        // 2. Quadrantes (peso 20)
        var rq = _quadrantes.AnalisarSorteio(dezenas);
        var padraoAtual = rq.Padrao;
        var scoreQ = padraoAtual == padraoQuadranteEsperado ? 20 : 10;
        detalhes["Quadrantes"] = scoreQ;
        total += scoreQ;

        // 3. Bordas (peso 15) - ideal 2 a 4
        var bordas = dezenas.Count(n => _quadrantes.EhBorda(n));
        var scoreB = bordas is >= 2 and <= 4 ? 15 : Math.Max(0, 15 - Math.Abs(bordas - 3) * 5);
        detalhes["Bordas"] = scoreB;
        total += scoreB;

        // 4. Mix quente/frio (peso 20) - ideal ter 1-2 frios e resto quentes/normais
        var frios = dezenas.Count(d => frequencias.First(f => f.Numero == d).Status == "Frio");
        var scoreMix = frios is 1 or 2 ? 20 : frios == 0 ? 10 : Math.Max(0, 20 - frios * 5);
        detalhes["Mix Quente/Frio"] = scoreMix;
        total += scoreMix;

        // 5. Não repetir último sorteio (peso 20)
        var ultimo = sorteios.OrderByDescending(s => s.Concurso).First();
        var ultimosNums = ultimo.Numeros.Select(n => n.Valor).ToList();
        var repeticoes = dezenas.Count(d => ultimosNums.Contains(d));
        var scoreRep = repeticoes == 0 ? 20 : Math.Max(0, 20 - repeticoes * 7);
        detalhes["Sem repetição"] = scoreRep;
        total += scoreRep;

        return new JogoComScore
        {
            Dezenas = dezenas,
            Score = Math.Round(total, 1),
            Detalhes = detalhes
        };
    }
}