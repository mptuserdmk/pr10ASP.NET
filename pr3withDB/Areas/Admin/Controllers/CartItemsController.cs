using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pr3withDB.Models;

namespace pr3withDB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CartItemsController : Controller
    {
        private readonly ShopDbContext _context;

        public CartItemsController(ShopDbContext context) => _context = context;

        // Список
        public async Task<IActionResult> Index()
        {
            var items = await _context.CartItems
                .Include(ci => ci.Cart)
                    .ThenInclude(c => c!.User)
                .Include(ci => ci.Product)
                .ToListAsync();
            return View(items);
        }

        // Создание (Форма)
        public IActionResult Create()
        {
            ViewBag.CartId = new SelectList(_context.Carts, "IdCarts", "IdCarts");
            ViewBag.ProductId = new SelectList(_context.Products, "IdProducts", "ProductName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CartItem item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CartId = new SelectList(_context.Carts, "IdCarts", "IdCarts", item.CartId);
            ViewBag.ProductId = new SelectList(_context.Products, "IdProducts", "ProductName", item.ProductId);
            return View(item);
        }

        // Редактирование (Форма)
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.CartId = new SelectList(_context.Carts, "IdCarts", "IdCarts", item.CartId);
            ViewBag.ProductId = new SelectList(_context.Products, "IdProducts", "ProductName", item.ProductId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CartItem item)
        {
            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CartId = new SelectList(_context.Carts, "IdCarts", "IdCarts", item.CartId);
            ViewBag.ProductId = new SelectList(_context.Products, "IdProducts", "ProductName", item.ProductId);
            return View(item);
        }

        // Удаление
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item != null) _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
