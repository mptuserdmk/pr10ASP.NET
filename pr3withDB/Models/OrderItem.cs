using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;


namespace pr3withDB.Models;

public partial class OrderItem
{
    public int IdOrderItems { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    [Required(ErrorMessage = "Количество обязательно")]
    [Range(1, 1000, ErrorMessage = "Количество должно быть от 1 до 1000")]
    public int OrderItemQuantity { get; set; }

    [Required(ErrorMessage = "Цена обязательна")]
    [Range(0.01, 1000000, ErrorMessage = "Цена должна быть положительной")]
    public decimal OrderItemPriceAtPurchase { get; set; }

    [ValidateNever]
    public virtual Order Order { get; set; } = null!;

    [ValidateNever]
    public virtual Product Product { get; set; } = null!;
}
