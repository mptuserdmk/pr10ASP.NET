using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using pr3withDB.Attributes;


namespace pr3withDB.Models;

public partial class Category
{
    public int IdCategories { get; set; }

    [Required(ErrorMessage = "Название категории обязательно")]
    [StringLength(100, ErrorMessage = "Название категории не может превышать 100 символов")]
    [UniqueCategoryName]
    [NoHtmlTags]
    public string CategoryName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Описание не может превышать 500 символов")]
    [NoHtmlTags]
    public string? CategoryDescription { get; set; }

    [ValidateNever]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
