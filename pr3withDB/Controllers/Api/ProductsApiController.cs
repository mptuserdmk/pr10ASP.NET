using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pr3withDB.Common;
using pr3withDB.DTOs;
using pr3withDB.Services;

namespace pr3withDB.Controllers.Api;

/// <summary>
/// Тонкий API контроллер для управления товарами.
/// Делегирует всю бизнес-логику слою сервисов (IProductService).
/// </summary>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
[AllowAnonymous]
public class ProductsApiController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsApiController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Получение списка товаров с пагинацией и возвратом общего количества в заголовке X-Total-Count.
    /// </summary>
    /// <param name="page">Номер страницы (по умолчанию 1).</param>
    /// <param name="pageSize">Количество элементов на странице (по умолчанию 10).</param>
    /// <param name="categoryId">Фильтр по категории (опционально).</param>
    /// <param name="search">Поисковый запрос по названию и описанию (опционально).</param>
    /// <response code="200">Возвращает список товаров в формате DTO и заголовок X-Total-Count.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null)
    {
        var result = await _productService.GetPagedProductsAsync(page, pageSize, categoryId, search);

        // Добавляем заголовок X-Total-Count и выставляем Access-Control-Expose-Headers
        Response.Headers.Append("X-Total-Count", result.Data!.TotalCount.ToString());
        Response.Headers.Append("Access-Control-Expose-Headers", "X-Total-Count");

        return Ok(result.Data.Items);
    }

    /// <summary>
    /// Получение конкретного товара по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <response code="200">Товар найден и возвращен в формате DTO.</response>
    /// <response code="404">Товар с указанным ID не найден.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductReadDto>> GetById(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Создание нового товара.
    /// </summary>
    /// <param name="dto">DTO с данными для создания товара.</param>
    /// <response code="201">Товар успешно создан.</response>
    /// <response code="400">Ошибка валидации переданных данных.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductReadDto>> Create([FromBody] ProductCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _productService.CreateProductAsync(dto);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Полное обновление существующего товара (PUT).
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="dto">DTO с новыми данными товара.</param>
    /// <response code="204">Товар успешно обновлен.</response>
    /// <response code="400">Ошибка валидации данных.</response>
    /// <response code="404">Товар не найден.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _productService.UpdateProductAsync(id, dto);
        if (!result.IsSuccess)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    /// <summary>
    /// Частичное обновление полей товара (PATCH).
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="dto">DTO с полями для обновления.</param>
    /// <response code="204">Товар успешно обновлен.</response>
    /// <response code="400">Ошибка валидации.</response>
    /// <response code="404">Товар не найден.</response>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchProductDTO dto)
    {
        var result = await _productService.PatchProductAsync(id, dto);
        if (!result.IsSuccess)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    /// <summary>
    /// Удаление товара по идентификатору (DELETE).
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <response code="204">Товар успешно удален.</response>
    /// <response code="404">Товар не найден.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }
}
