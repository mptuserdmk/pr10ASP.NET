using System.ComponentModel.DataAnnotations;

namespace pr3withDB.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Пожалуйста, введите логин")]
    [Display(Name = "Логин")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Пожалуйста, введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = null!;

    [Display(Name = "Запомнить меня?")]
    public bool RememberMe { get; set; }
}

