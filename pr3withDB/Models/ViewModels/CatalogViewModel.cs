using System.Collections.Generic;
using pr3withDB.Models;

namespace pr3withDB.Models.ViewModels
{
    public class CatalogViewModel
    {
        public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? SearchString { get; set; }
        public int? CategoryId { get; set; }
        public string? SortBy { get; set; }
    }
}
