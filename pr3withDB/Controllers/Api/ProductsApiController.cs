using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pr3withDB.DTOs;
using pr3withDB.Models;

namespace pr3withDB.Controllers.Api;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
[AllowAnonymous]
public class ProductsApiController : ControllerBase
{
    private readonly ShopDbContext _context;

    public ProductsApiController(ShopDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Получение списка товаров с пагинацией и сортировкой по Id.
    /// </summary>
    /// <param name="page">Номер страницы (по умолчанию 1).</param>
    /// <param name="pageSize">Количество элементов на странице (по умолчанию 10).</param>
    /// <response code="200">Возвращает список товаров в формате DTO.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.IdProducts)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = products.Select(ProductMapper.ToDto).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Получение конкретного товара по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <response code="200">Товар найден и возвращен в формате DTO.</response>
    /// <response code="404">Товар с указанным ID не найден.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDTO>> GetById(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProducts == id);

        if (product == null)
        {
            return NotFound(new { message = $"Товар с ID {id} не найден." });
        }

        return Ok(ProductMapper.ToDto(product));
    }

    /// <summary>
    /// Создание нового товара.
    /// </summary>
    /// <param name="dto">Данные для создания товара.</param>
    /// <response code="201">Товар успешно создан. Возвращает созданный объект и заголовок Location.</response>
    /// <response code="400">Ошибка валидации входных данных.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDTO>> Create([FromBody] CreateProductDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = ProductMapper.ToEntity(dto);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var createdDto = ProductMapper.ToDto(product);
        return CreatedAtAction(nameof(GetById), new { id = product.IdProducts }, createdDto);
    }

    /// <summary>
    /// Полное обновление товара (PUT).
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="dto">Полные новые данные товара.</param>
    /// <response code="204">Товар успешно обновлен.</response>
    /// <response code="400">Ошибка валидации входных данных.</response>
    /// <response code="404">Товар с указанным ID не найден.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Товар с ID {id} не найден." });
        }

        ProductMapper.ApplyUpdate(product, dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Частичное обновление товара (PATCH).
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="dto">Поля для частичного обновления.</param>
    /// <response code="204">Товар успешно частично обновлен.</response>
    /// <response code="400">Ошибка валидации входных данных.</response>
    /// <response code="404">Товар с указанным ID не найден.</response>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchProductDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Товар с ID {id} не найден." });
        }

        ProductMapper.ApplyPatch(product, dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удаление товара по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <response code="204">Товар успешно удален.</response>
    /// <response code="404">Товар с указанным ID не найден.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Товар с ID {id} не найден." });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
