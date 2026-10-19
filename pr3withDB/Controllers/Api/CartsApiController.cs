using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pr3withDB.DTOs;
using pr3withDB.Models;

namespace pr3withDB.Controllers.Api;

[ApiController]
[Route("api/carts")]
[Produces("application/json")]
[AllowAnonymous]
public class CartsApiController : ControllerBase
{
    private readonly ShopDbContext _context;

    public CartsApiController(ShopDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Получение списка корзин с пагинацией и сортировкой по Id.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CartDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CartDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var carts = await _context.Carts
            .AsNoTracking()
            .OrderBy(c => c.IdCarts)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = carts.Select(CartMapper.ToDto).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Получение корзины по идентификатору.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CartDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDTO>> GetById(int id)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCarts == id);

        if (cart == null)
        {
            return NotFound(new { message = $"Корзина с ID {id} не найдена." });
        }

        return Ok(CartMapper.ToDto(cart));
    }

    /// <summary>
    /// Создание новой корзины.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CartDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDTO>> Create([FromBody] CreateCartDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userExists = await _context.Users.AnyAsync(u => u.IdUsers == dto.UserId);
        if (!userExists)
        {
            ModelState.AddModelError(nameof(dto.UserId), $"Пользователь с ID {dto.UserId} не существует.");
            return BadRequest(ModelState);
        }

        var cartAlreadyExists = await _context.Carts.AnyAsync(c => c.UserId == dto.UserId);
        if (cartAlreadyExists)
        {
            ModelState.AddModelError(nameof(dto.UserId), $"Корзина для пользователя с ID {dto.UserId} уже существует.");
            return BadRequest(ModelState);
        }

        var cart = CartMapper.ToEntity(dto);
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        var createdDto = CartMapper.ToDto(cart);
        return CreatedAtAction(nameof(GetById), new { id = cart.IdCarts }, createdDto);
    }

    /// <summary>
    /// Полное обновление корзины (PUT).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCartDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var cart = await _context.Carts.FindAsync(id);
        if (cart == null)
        {
            return NotFound(new { message = $"Корзина с ID {id} не найдена." });
        }

        var userExists = await _context.Users.AnyAsync(u => u.IdUsers == dto.UserId);
        if (!userExists)
        {
            ModelState.AddModelError(nameof(dto.UserId), $"Пользователь с ID {dto.UserId} не существует.");
            return BadRequest(ModelState);
        }

        var userHasOtherCart = await _context.Carts.AnyAsync(c => c.UserId == dto.UserId && c.IdCarts != id);
        if (userHasOtherCart)
        {
            ModelState.AddModelError(nameof(dto.UserId), $"У пользователя с ID {dto.UserId} уже есть другая корзина.");
            return BadRequest(ModelState);
        }

        CartMapper.ApplyUpdate(cart, dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Частичное обновление корзины (PATCH).
    /// </summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchCartDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var cart = await _context.Carts.FindAsync(id);
        if (cart == null)
        {
            return NotFound(new { message = $"Корзина с ID {id} не найдена." });
        }

        if (dto.UserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.IdUsers == dto.UserId.Value);
            if (!userExists)
            {
                ModelState.AddModelError(nameof(dto.UserId), $"Пользователь с ID {dto.UserId.Value} не существует.");
                return BadRequest(ModelState);
            }

            var userHasOtherCart = await _context.Carts.AnyAsync(c => c.UserId == dto.UserId.Value && c.IdCarts != id);
            if (userHasOtherCart)
            {
                ModelState.AddModelError(nameof(dto.UserId), $"У пользователя с ID {dto.UserId.Value} уже есть другая корзина.");
                return BadRequest(ModelState);
            }
        }

        CartMapper.ApplyPatch(cart, dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удаление корзины по идентификатору.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var cart = await _context.Carts.FindAsync(id);
        if (cart == null)
        {
            return NotFound(new { message = $"Корзина с ID {id} не найдена." });
        }

        try
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "Невозможно удалить корзину, так как в ней содержатся товары." });
        }

        return NoContent();
    }
}
