using pr3withDB.Models;

namespace pr3withDB.DTOs;

/// <summary>
/// Статический класс-маппер для преобразования между Entity Product и его DTO.
/// Изолирует внутреннюю структуру базы данных от внешнего API контракта.
/// </summary>
public static class ProductMapper
{
    /// <summary>
    /// Преобразует сущность Product в ProductDTO для ответа API.
    /// </summary>
    public static ProductDTO ToDto(Product entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new ProductDTO
        {
            Id = entity.IdProducts,
            ProductName = entity.ProductName,
            ProductDescription = entity.ProductDescription,
            ProductPrice = entity.ProductPrice,
            ProductStockQuantity = entity.ProductStockQuantity,
            CategoryId = entity.CategoryId,
            ProductImageUrl = entity.ProductImageUrl
        };
    }

    /// <summary>
    /// Преобразует входной CreateProductDTO в новую сущность Product.
    /// </summary>
    public static Product ToEntity(CreateProductDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Product
        {
            ProductName = dto.ProductName,
            ProductDescription = dto.ProductDescription,
            ProductPrice = dto.ProductPrice,
            ProductStockQuantity = dto.ProductStockQuantity,
            CategoryId = dto.CategoryId,
            ProductImageUrl = dto.ProductImageUrl
        };
    }

    /// <summary>
    /// Применяет полное обновление полей сущности Product из UpdateProductDTO (для PUT).
    /// </summary>
    public static void ApplyUpdate(Product entity, UpdateProductDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        entity.ProductName = dto.ProductName;
        entity.ProductDescription = dto.ProductDescription;
        entity.ProductPrice = dto.ProductPrice;
        entity.ProductStockQuantity = dto.ProductStockQuantity;
        entity.CategoryId = dto.CategoryId;
        entity.ProductImageUrl = dto.ProductImageUrl;
    }

    /// <summary>
    /// Применяет частичное обновление только переданных полей сущности Product из PatchProductDTO (для PATCH).
    /// </summary>
    public static void ApplyPatch(Product entity, PatchProductDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        if (dto.ProductName != null)
        {
            entity.ProductName = dto.ProductName;
        }

        if (dto.ProductDescription != null)
        {
            entity.ProductDescription = dto.ProductDescription;
        }

        if (dto.ProductPrice.HasValue)
        {
            entity.ProductPrice = dto.ProductPrice.Value;
        }

        if (dto.ProductStockQuantity.HasValue)
        {
            entity.ProductStockQuantity = dto.ProductStockQuantity.Value;
        }

        if (dto.CategoryId.HasValue)
        {
            entity.CategoryId = dto.CategoryId.Value;
        }

        if (dto.ProductImageUrl != null)
        {
            entity.ProductImageUrl = dto.ProductImageUrl;
        }
    }
}
