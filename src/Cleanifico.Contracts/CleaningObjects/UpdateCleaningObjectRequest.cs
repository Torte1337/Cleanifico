using System.ComponentModel.DataAnnotations;

namespace Cleanifico.Contracts.CleaningObjects;

public sealed class UpdateCleaningObjectRequest
{
    [Required, MaxLength(50)] public string? ObjectNumber { get; set; }
    public Guid CustomerId { get; set; }
    [Required, MaxLength(200)] public string? Name { get; set; }
    [MaxLength(200)] public string? Street { get; set; }
    [MaxLength(20)] public string? PostalCode { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? Country { get; set; }
    [MaxLength(100)] public string? ContactFirstName { get; set; }
    [MaxLength(100)] public string? ContactLastName { get; set; }
    [EmailAddress, MaxLength(320)] public string? ContactEmail { get; set; }
    [MaxLength(50)] public string? ContactPhone { get; set; }
    [MaxLength(2000)] public string? AccessNotes { get; set; }
    [MaxLength(2000)] public string? CleaningNotes { get; set; }
}
