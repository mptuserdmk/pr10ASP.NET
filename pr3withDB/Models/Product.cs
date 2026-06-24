using pr3withDB.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace pr3withDB.Models;

[PriceStockDependency(100000, 5)]
public partial class Product
{
    public int IdProducts { get; set; }

    [Required(ErrorMessage = "Введите название товара!")]
    [NoHtmlTags(ErrorMessage = "Название не должно содержать HTML-теги для безопасности.")]
    [ForbiddenWords("fake", "scam", "подделка", ErrorMessage = "В названии использованы недопустимые или рекламные слова.")]
    [UniqueProductName]
    public string ProductName { get; set; } = null!;

    [MinWords(3, ErrorMessage = "Описание слишком короткое. Расскажите о товаре подробнее (минимум 3 слова).")]
    [NoHtmlTags]
    public string? ProductDescription { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "Укажите реальную цену от 0.01 ₽")]
    public decimal ProductPrice { get; set; }

    [Range(0, 10000, ErrorMessage = "Количество не может быть отрицательным.")]
    public int ProductStockQuantity { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите корректную категорию")]
    public int CategoryId { get; set; }

    public string? ProductImageUrl { get; set; }

    [NotMapped]
    [AllowedExtensions(".jpg,.jpeg,.png,.webp,.gif", ErrorMessage = "Разрешены только форматы: JPG, JPEG, PNG, WEBP, GIF")]
    [MaxFileSize(2, ErrorMessage = "Максимальный размер файла — 2 МБ")]
    [Display(Name = "Файл изображения")]
    public IFormFile? ImageFile { get; set; }

    [ValidateNever]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [ValidateNever]
    public virtual Category Category { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
