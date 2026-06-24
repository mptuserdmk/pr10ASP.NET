using System;
using System.Collections.Generic;

namespace pr3withDB.Models;

public partial class CartItem
{
    public int IdCartItems { get; set; }

    public int? CartId { get; set; }

    public string UserId { get; set; } = null!;

    public int ProductId { get; set; }

    public int CartItemQuantity { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Product Product { get; set; } = null!;
}
