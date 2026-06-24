using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using pr3withDB.Attributes;


namespace pr3withDB.Models;

public partial class Role
{
    public int IdRoles { get; set; }

    [Required(ErrorMessage = "Название роли обязательно")]
    [StringLength(50, ErrorMessage = "Название роли не может превышать 50 символов")]
    [UniqueRoleName]
    [NoHtmlTags]
    public string RoleName { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
