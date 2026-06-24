using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pr3withDB.Models;

namespace pr3withDB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ShopDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ShopDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Список товаров
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // Создание (Форма)
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "IdCategories", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // Поле ImageFile необязательно при создании
            if (product.ImageFile == null)
                ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                // Обработка загрузки изображения
                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ProductImageUrl = uniqueFileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "IdCategories", "CategoryName", product.CategoryId);
            return View(product);
        }

        // Редактирование (Форма)
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "IdCategories", "CategoryName", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            // Если новый файл не загружен, убираем ошибку из ModelState
            if (product.ImageFile == null)
                ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                // Загружаем текущий товар из БД
                var existingProduct = await _context.Products.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdProducts == product.IdProducts);

                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    // Удаляем старое изображение с диска
                    if (!string.IsNullOrEmpty(existingProduct?.ProductImageUrl))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, "images", existingProduct.ProductImageUrl);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // Сохраняем новое изображение
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ProductImageUrl = uniqueFileName;
                }
                else
                {
                    // Оставляем старый URL изображения без изменений
                    product.ProductImageUrl = existingProduct?.ProductImageUrl;
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "IdCategories", "CategoryName", product.CategoryId);
            return View(product);
        }

        // Удаление
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(m => m.IdProducts == id);
            if (product != null)
            {
                // Удаляем физический файл изображения с диска
                if (!string.IsNullOrEmpty(product.ProductImageUrl))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, "images", product.ProductImageUrl);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
