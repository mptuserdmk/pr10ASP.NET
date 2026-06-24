using System.Collections.Generic;
using System.Linq;

namespace pr3withDB.Models.ViewModels
{
    public class CartViewModel
    {
        public IEnumerable<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        
        public decimal TotalSum => Items.Sum(item => item.Sum);
    }
}
