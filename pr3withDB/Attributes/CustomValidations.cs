using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Linq;
using pr3withDB.Models;
using Microsoft.AspNetCore.Http;

namespace pr3withDB.Attributes
{
    // 1.проверка отстутсвтия тегов хтмл
    public class NoHtmlTagsAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string strValue)
            {
                if (Regex.IsMatch(strValue, @"<.*?>"))
                {
                    return new ValidationResult(ErrorMessage ?? "Поле не должно содержать HTML-теги.");
                }
            }
            return ValidationResult.Success;
        }
    }
    public class MinWordsAttribute : ValidationAttribute
    {
        private readonly int _minWords;

        public MinWordsAttribute(int minWords)
        {
            _minWords = minWords;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string strValue && !string.IsNullOrWhiteSpace(strValue))
            {
                var wordCount = strValue.Split(new[] { ' ', '\r', '\n', '\t', '.', ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (wordCount < _minWords)
                {
                    return new ValidationResult(ErrorMessage ?? $"Текст должен содержать минимум {_minWords} слов(а).");
                }
            }
            return ValidationResult.Success;
        }
    }
    public class ForbiddenWordsAttribute : ValidationAttribute
    {
        private readonly string[] _forbiddenWords;

        public ForbiddenWordsAttribute(params string[] forbiddenWords)
        {
            _forbiddenWords = forbiddenWords;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string strValue && !string.IsNullOrWhiteSpace(strValue))
            {
                var lowerValue = strValue.ToLowerInvariant();
                var foundWords = _forbiddenWords.Where(w => lowerValue.Contains(w.ToLowerInvariant())).ToList();
                
                if (foundWords.Any())
                {
                    return new ValidationResult(ErrorMessage ?? $"Поле содержит запрещенные слова: {string.Join(", ", foundWords)}.");
                }
            }
            return ValidationResult.Success;
        }
    }
    public class UniqueProductNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string name)
            {
                var dbContext = (ShopDbContext?)validationContext.GetService(typeof(ShopDbContext));
                if (dbContext != null)
                {
                    var currentProduct = (Product)validationContext.ObjectInstance;
                    
                    bool nameExists = dbContext.Products.Any(p => p.ProductName.ToLower() == name.ToLower() && p.IdProducts != currentProduct.IdProducts);
                    if (nameExists)
                    {
                        return new ValidationResult(ErrorMessage ?? "Товар с таким названием уже существует в базе данных.");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
    public class PriceStockDependencyAttribute : ValidationAttribute
    {
        private readonly decimal _luxuryThreshold;
        private readonly int _maxStockForLuxury;

        public PriceStockDependencyAttribute(double luxuryThreshold, int maxStockForLuxury)
        {
            _luxuryThreshold = (decimal)luxuryThreshold;
            _maxStockForLuxury = maxStockForLuxury;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is Product product)
            {
                if (product.ProductPrice >= _luxuryThreshold && product.ProductStockQuantity > _maxStockForLuxury)
                {
                    return new ValidationResult(ErrorMessage ?? $"Премиум-товары (цена от {_luxuryThreshold} ₽) выпускаются ограниченным тиражом. Остаток не может превышать {_maxStockForLuxury} шт.");
                }
            }
            return ValidationResult.Success;
        }
    }

    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedExtensionsAttribute(string extensions)
        {
            _extensions = extensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLower()).ToArray();
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is Microsoft.AspNetCore.Http.IFormFile file)
            {
                var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
                if (!_extensions.Contains(extension))
                {
                    return new ValidationResult(ErrorMessage ?? $"Разрешены только следующие расширения: {string.Join(", ", _extensions)}");
                }
            }
            return ValidationResult.Success;
        }
    }
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSizeInMb)
        {
            _maxFileSize = maxFileSizeInMb;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is Microsoft.AspNetCore.Http.IFormFile file)
            {
                if (file.Length > _maxFileSize * 1024 * 1024)
                {
                    return new ValidationResult(ErrorMessage ?? $"Максимальный размер файла: {_maxFileSize} МБ.");
                }
            }
            return ValidationResult.Success;
        }
    }
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;
        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                if (dateOfBirth.AddYears(_minimumAge) > DateTime.Now)
                {
                    return new ValidationResult(ErrorMessage ?? $"Для регистрации вам должно быть не менее {_minimumAge} лет.");
                }
            }
            return ValidationResult.Success;
        }
    }
    public class SkuAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string sku && !string.IsNullOrWhiteSpace(sku))
            {
                if (!Regex.IsMatch(sku, @"^[A-Z]{3}-\d{5}$"))
                {
                    return new ValidationResult(ErrorMessage ?? "Артикул должен быть в формате 'AAA-00000' (3 заглавные буквы, дефис, 5 цифр).");
                }
            }
            return ValidationResult.Success;
        }
    }

    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;
        private readonly object _targetValue;

        public RequiredIfAttribute(string otherProperty, object targetValue)
        {
            _otherProperty = otherProperty;
            _targetValue = targetValue;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_otherProperty);
            if (property == null)
                return new ValidationResult($"Свойство {_otherProperty} не найдено.");

            var otherValue = property.GetValue(validationContext.ObjectInstance);

            if (otherValue?.ToString() == _targetValue.ToString())
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage ?? $"Поле обязательно для заполнения, когда {_otherProperty} равно {_targetValue}.");
                }
            }

            return ValidationResult.Success;
        }
    }

    public class UniqueUserLoginAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string login)
            {
                var dbContext = (ShopDbContext?)validationContext.GetService(typeof(ShopDbContext));
                if (dbContext != null)
                {
                    var currentUser = (User)validationContext.ObjectInstance;
                    bool loginExists = dbContext.Users.Any(u => u.UserLogin.ToLower() == login.ToLower() && u.IdUsers != currentUser.IdUsers);
                    if (loginExists)
                    {
                        return new ValidationResult(ErrorMessage ?? "Пользователь с таким логином уже существует.");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }

    public class UniqueCategoryNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string name)
            {
                var dbContext = (ShopDbContext?)validationContext.GetService(typeof(ShopDbContext));
                if (dbContext != null)
                {
                    var currentCategory = (Category)validationContext.ObjectInstance;
                    bool nameExists = dbContext.Categories.Any(c => c.CategoryName.ToLower() == name.ToLower() && c.IdCategories != currentCategory.IdCategories);
                    if (nameExists)
                    {
                        return new ValidationResult(ErrorMessage ?? "Категория с таким названием уже существует.");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }

    public class UniqueRoleNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string name)
            {
                var dbContext = (ShopDbContext?)validationContext.GetService(typeof(ShopDbContext));
                if (dbContext != null)
                {
                    var currentRole = (Role)validationContext.ObjectInstance;
                    bool nameExists = dbContext.Roles.Any(r => r.RoleName.ToLower() == name.ToLower() && r.IdRoles != currentRole.IdRoles);
                    if (nameExists)
                    {
                        return new ValidationResult(ErrorMessage ?? "Роль с таким названием уже существует.");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}
