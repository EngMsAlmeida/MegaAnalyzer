using MegaAnalyzer.Services.Analysis;

namespace MegaAnalyzer.Services;

public class ResultadoAgente
{
    public List<JogoComScore> JogosMesclados { get; set; } = new();
    public string Raciocinio { get; set; } = string.Empty;
}

public class AgenteIA
{
    private readonly Random _random = new();
    private readonly AnaliseQuadrantes _quadrantes = new();

    public Task<ResultadoAgente> MesclarJogosAsync(List<JogoComScore> jogos)
    {
        var resultado = new ResultadoAgente();
        var raciocinio = new List<string>();

        // Mapeia frequência de cada dezena nos jogos selecionados
        var frequencia = new Dictionary<int, int>();
        foreach (var jogo in jogos)
            foreach (var d in jogo.Dezenas)
                frequencia[d] = frequencia.GetValueOrDefault(d, 0) + 1;

        // Dezenas que aparecem em 2+ jogos = candidatas prioritárias
        var prioritarias = frequencia
            .Where(f => f.Value >= 2)
            .OrderByDescending(f => f.Value)
            .Select(f => f.Key)
            .ToList();

        // Todas as dezenas dos jogos como pool
        var pool = frequencia.Keys.OrderByDescending(k => frequencia[k]).ToList();

        raciocinio.Add($"Dezenas em comum (2+ jogos): {string.Join(", ", prioritarias.Select(p => p.ToString("D2")))}");
        raciocinio.Add($"Pool total de dezenas: {pool.Count}");

        // Gera 3 jogos mesclados
        for (int i = 0; i < 3; i++)
        {
            var jogo = GerarJogoMesclado(prioritarias, pool, jogos, i);
            resultado.JogosMesclados.Add(jogo);
            raciocinio.Add($"Jogo {(char)('A' + i)}: {jogo.DezenasFormatadas}");
        }

        resultado.Raciocinio = string.Join("\n", raciocinio);
        return Task.FromResult(resultado);
    }

    private JogoComScore GerarJogoMesclado(
        List<int> prioritarias,
        List<int> pool,
        List<JogoComScore> jogosOriginais,
        int variacao)
    {
        var dezenas = new HashSet<int>();

        switch (variacao)
        {
            case 0: // Variação A: foca nas dezenas em comum + complementa do jogo 1
                foreach (var p in prioritarias.Take(3))
                    dezenas.Add(p);
                foreach (var d in jogosOriginais[0].Dezenas)
                {
                    if (dezenas.Count >= 6) break;
                    dezenas.Add(d);
                }
                break;

            case 1: // Variação B: 1 dezena em comum + mistura dos jogos 2 e 3
                foreach (var p in prioritarias.Take(1))
                    dezenas.Add(p);
                foreach (var d in jogosOriginais[1].Dezenas)
                {
                    if (dezenas.Count >= 6) break;
                    dezenas.Add(d);
                }
                foreach (var d in jogosOriginais[2].Dezenas.OrderBy(_ => _random.Next()))
                {
                    if (dezenas.Count >= 6) break;
                    dezenas.Add(d);
                }
                break;

            case 2: // Variação C: 2 dezenas em comum + dezenas únicas de cada jogo
                foreach (var p in prioritarias.Take(2))
                    dezenas.Add(p);
                foreach (var jogo in jogosOriginais)
                {
                    var unicas = jogo.Dezenas
                        .Where(d => !prioritarias.Contains(d))
                        .OrderBy(_ => _random.Next())
                        .Take(2);
                    foreach (var d in unicas)
                    {
                        if (dezenas.Count >= 6) break;
                        dezenas.Add(d);
                    }
                }
                break;
        }

        // Completa se necessário com dezenas aleatórias do pool
        foreach (var d in pool.OrderBy(_ => _random.Next()))
        {
            if (dezenas.Count >= 6) break;
            dezenas.Add(d);
        }

        var lista = dezenas.Take(6).OrderBy(n => n).ToList();

        var bordas = lista.Count(n => _quadrantes.EhBorda(n));
        var pares = lista.Count(n => n % 2 == 0);
        var score = 50.0
            + (bordas is >= 2 and <= 4 ? 15 : 0)
            + (pares is 2 or 3 or 4 ? 15 : 0)
            + (prioritarias.Count(p => lista.Contains(p)) * 5);

        var descricoes = new[] {
            "Foco nas dezenas compartilhadas + complemento do jogo 1",
            "Mistura equilibrada dos jogos 2 e 3 com 1 dezena comum",
            "2 dezenas comuns + dezenas únicas de cada jogo"
        };

        return new JogoComScore
        {
            Dezenas = lista,
            Score = Math.Round(score, 1),
            GeradoPorIA = true,
            ExplicacaoIA = descricoes[variacao]
        };
    }
}