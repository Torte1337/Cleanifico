using System.ComponentModel.DataAnnotations;

namespace Cleanifico.Contracts.Users;

public sealed class UpdateUserRolesRequest
{
    [MinLength(1, ErrorMessage = "Mindestens eine Rolle ist erforderlich.")]
    public List<string> Roles { get; set; } = [];
}
