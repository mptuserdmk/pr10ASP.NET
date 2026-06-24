using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pr3withDB.Models;

namespace pr3withDB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderItemsController : Controller
    {
        private readonly ShopDbContext _context;

        public OrderItemsController(ShopDbContext context) => _context = context;

        // Список
        public async Task<IActionResult> Index()
        {
            var items = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .ToListAsync();
            return View(items);
        }

        // Создание (Форма)
        public IActionResult Create()
        {
            ViewBag.OrderId = new SelectList(_context.Orders, "IdOrders", "IdOrders");
            ViewBag.ProductId = new SelectList(_context.Products, "IdProducts", "ProductName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderItem item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OrderId = new SelectList(_context.Orders, "IdOrders", "IdOrders", item.OrderId);
            ViewBag.ProductId = new SelectList(_context.Products, "IdProducts", "ProductName", item.ProductId);
            return View(item);
        }

        // Редактирование (Форма)
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.OrderId = new SelectList(_context.Orders, "IdOrders", "IdOrders", item.OrderId);
            ViewBag.ProductId = new SelectList(_context.Products, "IdProducts", "ProductName", item.ProductId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrderItem item)
        {
            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OrderId = new SelectList(_context.Orders, "IdOrders", "IdOrders", item.OrderId);
            ViewBag.ProductId = new SelectList(_context.Products, "IdProducts", "ProductName", item.ProductId);
            return View(item);
        }

        // Удаление
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item != null) _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
