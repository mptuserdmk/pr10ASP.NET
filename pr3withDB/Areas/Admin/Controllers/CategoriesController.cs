using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using pr3withDB.Models;

namespace pr3withDB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ShopDbContext _context;
        private readonly IMemoryCache _cache;

        public CategoriesController(ShopDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Список
        public async Task<IActionResult> Index() => View(await _context.Categories.ToListAsync());

        // Создание (Форма)
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                _cache.Remove("categories_list"); // Сброс кэша
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Редактирование (Форма)
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            return category == null ? NotFound() : View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                _cache.Remove("categories_list"); // Сброс кэша
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Удаление
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                _cache.Remove("categories_list"); // Сброс кэша
            }
            return RedirectToAction(nameof(Index));
        }
    }
}