using pr3withDB.Models;

namespace pr3withDB.DTOs;

/// <summary>
/// Статический класс-маппер для сущности Cart.
/// </summary>
public static class CartMapper
{
    public static CartDTO ToDto(Cart entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new CartDTO
        {
            Id = entity.IdCarts,
            UserId = entity.UserId,
            CreatedAt = entity.CartCreatedAt
        };
    }

    public static Cart ToEntity(CreateCartDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Cart
        {
            UserId = dto.UserId,
            CartCreatedAt = DateTime.UtcNow
        };
    }

    public static void ApplyUpdate(Cart entity, UpdateCartDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        entity.UserId = dto.UserId;
    }

    public static void ApplyPatch(Cart entity, PatchCartDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        if (dto.UserId.HasValue)
        {
            entity.UserId = dto.UserId.Value;
        }
    }
}
