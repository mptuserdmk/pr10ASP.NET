using pr3withDB.Models;

namespace pr3withDB.DTOs;

/// <summary>
/// Статический класс-маппер для сущности CartItem.
/// </summary>
public static class CartItemMapper
{
    public static CartItemDTO ToDto(CartItem entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new CartItemDTO
        {
            Id = entity.IdCartItems,
            CartId = entity.CartId,
            UserId = entity.UserId,
            ProductId = entity.ProductId,
            CartItemQuantity = entity.CartItemQuantity
        };
    }

    public static CartItem ToEntity(CreateCartItemDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new CartItem
        {
            CartId = dto.CartId,
            UserId = dto.UserId,
            ProductId = dto.ProductId,
            CartItemQuantity = dto.CartItemQuantity
        };
    }

    public static void ApplyUpdate(CartItem entity, UpdateCartItemDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        entity.CartId = dto.CartId;
        entity.UserId = dto.UserId;
        entity.ProductId = dto.ProductId;
        entity.CartItemQuantity = dto.CartItemQuantity;
    }

    public static void ApplyPatch(CartItem entity, PatchCartItemDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        if (dto.CartId.HasValue)
        {
            entity.CartId = dto.CartId.Value;
        }

        if (dto.UserId != null)
        {
            entity.UserId = dto.UserId;
        }

        if (dto.ProductId.HasValue)
        {
            entity.ProductId = dto.ProductId.Value;
        }

        if (dto.CartItemQuantity.HasValue)
        {
            entity.CartItemQuantity = dto.CartItemQuantity.Value;
        }
    }
}
