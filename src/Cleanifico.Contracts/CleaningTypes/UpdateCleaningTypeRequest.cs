using System.ComponentModel.DataAnnotations;

namespace Cleanifico.Contracts.CleaningTypes;

public sealed class UpdateCleaningTypeRequest
{
    [Required(ErrorMessage = "Der Name ist erforderlich.")]
    [StringLength(200, ErrorMessage = "Der Name darf höchstens 200 Zeichen lang sein.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Das Kürzel ist erforderlich.")]
    [StringLength(20, ErrorMessage = "Das Kürzel darf höchstens 20 Zeichen lang sein.")]
    public string Code { get; set; } = string.Empty;

    [StringLength(1_000, ErrorMessage = "Die Beschreibung darf höchstens 1000 Zeichen lang sein.")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Die Sortierung darf nicht negativ sein.")]
    public int SortOrder { get; set; }
}
