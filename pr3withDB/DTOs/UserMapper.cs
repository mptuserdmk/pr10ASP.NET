using BCrypt.Net;
using pr3withDB.Models;

namespace pr3withDB.DTOs;

/// <summary>
/// Статический класс-маппер для сущности User.
/// </summary>
public static class UserMapper
{
    public static UserDTO ToDto(User entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new UserDTO
        {
            Id = entity.IdUsers,
            UserLogin = entity.UserLogin,
            RoleId = entity.RoleId,
            CreatedAt = entity.UserCreatedAt
        };
    }

    public static User ToEntity(CreateUserDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new User
        {
            UserLogin = dto.UserLogin,
            UserPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.RoleId,
            UserCreatedAt = DateTime.UtcNow
        };
    }

    public static void ApplyUpdate(User entity, UpdateUserDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        entity.UserLogin = dto.UserLogin;
        entity.UserPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        entity.RoleId = dto.RoleId;
    }

    public static void ApplyPatch(User entity, PatchUserDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        if (dto.UserLogin != null)
        {
            entity.UserLogin = dto.UserLogin;
        }

        if (!string.IsNullOrEmpty(dto.Password))
        {
            entity.UserPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        if (dto.RoleId.HasValue)
        {
            entity.RoleId = dto.RoleId.Value;
        }
    }
}
