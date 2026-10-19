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
/// Тонкий API контроллер для оформления и управления заказами.
/// Делегирует всю бизнес-логику (проверка и списание остатков, создание позиций) в IOrderService.
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[AllowAnonymous]
public class OrdersApiController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersApiController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Получение списка заказов с пагинацией и возвратом общего количества в X-Total-Count.
    /// </summary>
    /// <param name="page">Номер страницы (по умолчанию 1).</param>
    /// <param name="pageSize">Размер страницы (по умолчанию 10).</param>
    /// <param name="userId">Фильтрация по ID пользователя (опционально).</param>
    /// <response code="200">Возвращает список заказов с деталями позиций и заголовок X-Total-Count.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderReadDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? userId = null)
    {
        var result = await _orderService.GetPagedOrdersAsync(page, pageSize, userId);

        Response.Headers.Append("X-Total-Count", result.Data!.TotalCount.ToString());
        Response.Headers.Append("Access-Control-Expose-Headers", "X-Total-Count");

        return Ok(result.Data.Items);
    }

    /// <summary>
    /// Получение заказа по идентификатору со всеми позициями и товарами.
    /// </summary>
    /// <param name="id">Идентификатор заказа.</param>
    /// <response code="200">Заказ найден.</response>
    /// <response code="404">Заказ не найден.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderReadDto>> GetById(int id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Оформление нового заказа (Бизнес-операция):
    /// Проверяет наличие товаров, списывает остатки и создает заказ в рамках единого сохранения.
    /// </summary>
    /// <param name="dto">DTO с позициями заказа и ID пользователя.</param>
    /// <response code="201">Заказ успешно оформлен.</response>
    /// <response code="400">Ошибка валидации или недостаточно товара на складе.</response>
    /// <response code="404">Товар или пользователь не найдены.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderReadDto>> Create([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _orderService.CreateOrderAsync(dto);
        if (!result.IsSuccess)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Отмена заказа (Обратная бизнес-операция):
    /// Возвращает списанные товары на склад и переводит статус заказа в Cancelled.
    /// </summary>
    /// <param name="id">Идентификатор заказа.</param>
    /// <response code="200">Заказ успешно отменен, остатки возвращены на склад.</response>
    /// <response code="400">Заказ уже был отменен ранее или конфликт состояния.</response>
    /// <response code="404">Заказ не найден.</response>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(OrderReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderReadDto>> Cancel(int id)
    {
        var result = await _orderService.CancelOrderAsync(id);
        if (!result.IsSuccess)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Удаление заказа по идентификатору.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _orderService.DeleteOrderAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }
}
