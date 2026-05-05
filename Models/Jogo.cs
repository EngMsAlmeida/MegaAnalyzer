using System.ComponentModel.DataAnnotations;

namespace MegaAnalyzer.Models;

public class Jogo
{
    [Key]
    public int Id { get; set; }

    public DateTime GeradoEm { get; set; } = DateTime.Now;

    public string Dezenas { get; set; } = string.Empty; // "05,12,23,34,45,56"

    public double Score { get; set; } // 0 a 100

    public string Filtros { get; set; } = string.Empty; // quais filtros passou

    public bool Selecionado { get; set; } = false;

    public bool GeradoPorIA { get; set; } = false;

    public string? ExplicacaoIA { get; set; }
}