using System.ComponentModel.DataAnnotations;

namespace MegaAnalyzer.Models;

public class Sorteio
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int Concurso { get; set; }

    public DateTime Data { get; set; }

    public List<NumeroSorteado> Numeros { get; set; } = new();
}

public class NumeroSorteado
{
    public int Id { get; set; }
    public int SorteioId { get; set; }
    public Sorteio? Sorteio { get; set; }
    public int Valor { get; set; }
    public int Posicao { get; set; } // Casa 1 a 6 (ordenado)
}