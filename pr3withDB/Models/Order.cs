using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using pr3withDB.Attributes;


namespace pr3withDB.Models;

public partial class Order
{
    public int IdOrders { get; set; }

    public int UserId { get; set; }

    public DateTime OrderDate { get; set; }

    [Required(ErrorMessage = "Сумма заказа обязательна")]
    [Range(0, 10000000, ErrorMessage = "Сумма заказа должна быть положительной")]
    public decimal OrderTotalAmount { get; set; }

    [Required(ErrorMessage = "Статус заказа обязателен")]
    [StringLength(50, ErrorMessage = "Статус не может превышать 50 символов")]
    [NoHtmlTags]
    public string OrderStatus { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
