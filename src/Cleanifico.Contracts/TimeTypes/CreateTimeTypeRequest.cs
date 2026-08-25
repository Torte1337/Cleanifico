using System.ComponentModel.DataAnnotations;

namespace Cleanifico.Contracts.TimeTypes;

public sealed class CreateTimeTypeRequest
{
    [Required(ErrorMessage = "Der Name ist erforderlich.")]
    [StringLength(200, ErrorMessage = "Der Name darf höchstens 200 Zeichen lang sein.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Das Kürzel ist erforderlich.")]
    [StringLength(20, ErrorMessage = "Das Kürzel darf höchstens 20 Zeichen lang sein.")]
    public string Code { get; set; } = string.Empty;

    [StringLength(1_000, ErrorMessage = "Die Beschreibung darf höchstens 1000 Zeichen lang sein.")]
    public string? Description { get; set; }

    public bool CountsAsWorkTime { get; set; }

    public bool IsPaid { get; set; }

    public bool RequiresObject { get; set; }

    public bool IsAbsence { get; set; }

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Die Farbe muss als Hex-Wert im Format #RRGGBB angegeben werden.")]
    public string? Color { get; set; }

    public int SortOrder { get; set; }
}
