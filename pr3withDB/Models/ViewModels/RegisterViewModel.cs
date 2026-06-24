using System.ComponentModel.DataAnnotations;

namespace pr3withDB.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Пожалуйста, введите логин")]
    [Display(Name = "Логин")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Пожалуйста, введите пароль")]
    [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Подтвердите пароль")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
    public string ConfirmPassword { get; set; } = null!;
}

