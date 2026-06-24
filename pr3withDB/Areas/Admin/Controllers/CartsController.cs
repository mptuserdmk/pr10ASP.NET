using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pr3withDB.Models;

namespace pr3withDB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CartsController : Controller
    {
        private readonly ShopDbContext _context;

        public CartsController(ShopDbContext context) => _context = context;

        // Список
        public async Task<IActionResult> Index()
        {
            var carts = await _context.Carts.Include(c => c.User).ToListAsync();
            return View(carts);
        }

        // Детали корзины
        public async Task<IActionResult> Details(int id)
        {
            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.IdCarts == id);

            if (cart == null) return NotFound();
            return View(cart);
        }

        // Создание (Форма)
        public IActionResult Create()
        {
            ViewBag.UserId = new SelectList(_context.Users, "IdUsers", "UserLogin");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserId = new SelectList(_context.Users, "IdUsers", "UserLogin", cart.UserId);
            return View(cart);
        }

        // Редактирование (Форма)
        public async Task<IActionResult> Edit(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null) return NotFound();
            ViewBag.UserId = new SelectList(_context.Users, "IdUsers", "UserLogin", cart.UserId);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Update(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserId = new SelectList(_context.Users, "IdUsers", "UserLogin", cart.UserId);
            return View(cart);
        }

        // Удаление
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.IdCarts == id);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
