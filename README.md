# Практическая работа №10
## Внедрение слоистой архитектуры, AutoMapper, Repository Pattern и Service Layer в ASP.NET Core 8.0

Данный репозиторий представляет собой веб-приложение интернет-магазина на платформе **ASP.NET Core 8.0**, модернизированное в соответствии с принципами чистой слоистой архитектуры (Layered Architecture).

В проекте реализовано полное разделение ответственности: тонкие контроллеры, слой сервисов бизнес-логики, абстракция доступа к данным через паттерн «Репозиторий» (Repository Pattern), автоматический маппинг сущностей через **AutoMapper**, а также атомарные бизнес-операции без явных транзакций и UnitOfWork.

---

## 🚀 Основной стек технологий
* **Платформа:** .NET 8.0 (C# 12)
* **Веб-фреймворк:** ASP.NET Core 8.0 (MVC, Web API, Razor Views)
* **База данных:** MS SQL Server (LocalDB / Express)
* **ORM:** Entity Framework Core 8.0 (`Microsoft.EntityFrameworkCore.SqlServer`, `Tools`, `Design`)
* **Объектно-реляционный маппинг DTO:** `AutoMapper` 16.x
* **Аутентификация и безопасность:** Cookie Authentication, BCrypt.Net-Next (хеширование паролей)
* **Документация и тестирование API:** Swagger / OpenAPI (`Swashbuckle.AspNetCore` 10.2.3)
* **Фронтенд:** Razor Views (CSHTML), Bootstrap 5, HTML5, CSS3

---

## 🏛️ Слоистая архитектура проекта

```
                       ┌──────────────────────────────────────────────┐
                       │           HTTP Клиенты / Swagger UI          │
                       └──────────────────────┬───────────────────────┘
                                              │ HTTP-запросы / DTO
                                              ▼
┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│ 1. Слой представления (Presentation Layer / Thin Controllers)                               │
│    - ProductsApiController, CategoriesApiController, OrdersApiController                    │
│    - Задачи: валидация ModelState, вызов сервисов, возврат HTTP-кодов (200, 201, 204, 400), │
│      добавление пагинационных заголовков X-Total-Count и Access-Control-Expose-Headers      │
└─────────────────────────────────────────────┬───────────────────────────────────────────────┘
                                              │ Вызовы методов / DTO
                                              ▼
┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│ 2. Слой бизнес-логики (Service Layer)                                                       │
│    - IProductService / ProductService, IOrderService / OrderService, ICategoryService        │
│    - Задачи: валидация остатков на складе, расчет стоимости, бизнес-правила,                │
│      возврат типизированных результатов ServiceResult<T> и PagedResult<T>                   │
│    - AutoMapper (MappingProfile): изоляция доменных сущностей через DTO (Read/Create/Update)│
└─────────────────────────────────────────────┬───────────────────────────────────────────────┘
                                              │ Запросы к сущностям
                                              ▼
┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│ 3. Слой доступа к данным (Data Access Layer / Repository Pattern)                           │
│    - IRepository<T> / Repository<T> (Generic CRUD, пагинация, предикаты)                    │
│    - IOrderRepository / OrderRepository (выборки со сложными Include)                       │
│    - ЕДИНЫЙ вызов SaveChangesAsync() в рамках Scoped DbContext                              │
└─────────────────────────────────────────────┬───────────────────────────────────────────────┘
                                              │ Change Tracking
                                              ▼
┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│ 4. Контекст данных и БД (ShopDbContext & MS SQL Server)                                     │
│    - Единый экземпляр ShopDbContext на HTTP-запрос (Scoped)                                 │
│    - Автоматическая транзакция SQL Server при коммите изменений                             │
└─────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 📦 Архитектура DTO и AutoMapper

Все DTO строго разделены по назначению:
1. **ReadDto (Чтение):**
   * Включают вложенные и денормализованные данные для удобства клиента.
   * `ProductReadDto`: содержит `CategoryName`.
   * `CategoryReadDto`: содержит `ProductsCount`.
   * `OrderReadDto`: содержит имя заказчика `UserLogin` и коллекцию позиций `List<OrderItemReadDto>` (каждая с `ProductName` и расчетным `Subtotal`).
2. **WriteDto / CreateDto / UpdateDto (Запись):**
   * Содержат только внешние идентификаторы (`CategoryId`, `ProductId`, `UserId`).
   * Не содержат навигационных свойств сущностей, что исключает циклические ссылки и конфликты трекера EF Core.
3. **AutoMapper (`MappingProfile.cs`):**
   * Централизованная конфигурация маппинга без бойлерплейта и ручных конвертеров.

---

## ⚡ Бизнес-операции без явных транзакций (Единый `SaveChangesAsync`)

Отказ от явного создания `BeginTransactionAsync()` и громоздкого паттерна UnitOfWork достигается благодаря использованию **Scoped-времени жизни** в DI-контейнере:

### 1. Оформление заказа (`OrderService.CreateOrderAsync`)
* Сервис принимает `CreateOrderDto` со списком позиций и ID пользователя.
* Проверяется наличие пользователя и остаток каждого запрашиваемого товара на складе (`product.ProductStockQuantity >= item.Quantity`).
* Товар списывается в памяти (`product.ProductStockQuantity -= item.Quantity`), репозиторий помечает сущность как измененную через `_productRepository.Update(product)`.
* Создается заказ `Order` со связанными позициями `OrderItem`.
* **Вызывается `_orderRepository.SaveChangesAsync()` ровно 1 раз в конце операции**. Поскольку `_productRepository` и `_orderRepository` используют **один и тот же Scoped-экземпляр `ShopDbContext`**, EF Core выполняет все `INSERT` и `UPDATE` в единой неявной транзакции БД.

### 2. Отмена заказа (`OrderService.CancelOrderAsync`)
* Проверяется существование заказа и его статус (защита от повторной отмены).
* Для каждой позиции заказа возвращается списанное количество товаров обратно на склад (`product.ProductStockQuantity += item.Quantity`).
* Статус заказа обновляется на `Cancelled`.
* **Вызывается `SaveChangesAsync()` ровно 1 раз**, гарантируя атомарность возврата остатков.

---

## 🌐 Спецификация RESTful API

| Метод | Маршрут | Описание | Заголовки ответа | Коды ответов |
|---|---|---|---|---|
| **GET** | `/api/products?page=1&pageSize=10` | Каталог товаров с пагинацией и поиском | `X-Total-Count`, `Access-Control-Expose-Headers` | `200 OK` |
| **GET** | `/api/products/{id}` | Товар по ID с названием категории | — | `200 OK`, `404 NotFound` |
| **POST** | `/api/products` | Создание товара | `Location` | `201 Created`, `400 BadRequest` |
| **PUT** | `/api/products/{id}` | Полное обновление товара | — | `204 NoContent`, `400 BadRequest`, `404 NotFound` |
| **PATCH** | `/api/products/{id}` | Частичное обновление полей товара | — | `204 NoContent`, `400 BadRequest`, `404 NotFound` |
| **DELETE** | `/api/products/{id}` | Удаление товара | — | `204 NoContent`, `404 NotFound` |
| **GET** | `/api/categories` | Список категорий | — | `200 OK` |
| **GET** | `/api/categories/{id}` | Категория по ID | — | `200 OK`, `404 NotFound` |
| **POST** | `/api/categories` | Создание категории | `Location` | `201 Created`, `400 BadRequest` |
| **PUT** | `/api/categories/{id}` | Обновление категории | — | `204 NoContent`, `400 BadRequest`, `404 NotFound` |
| **PATCH** | `/api/categories/{id}` | Частичное обновление категории | — | `204 NoContent`, `400 BadRequest`, `404 NotFound` |
| **DELETE** | `/api/categories/{id}` | Удаление категории | — | `204 NoContent`, `404 NotFound` |
| **GET** | `/api/orders?page=1&pageSize=10` | Заказы с пагинацией, фильтром по пользователю и позициями | `X-Total-Count`, `Access-Control-Expose-Headers` | `200 OK` |
| **GET** | `/api/orders/{id}` | Детали заказа с позициями и товарами | — | `200 OK`, `404 NotFound` |
| **POST** | `/api/orders` | **Бизнес-операция:** создание заказа со списанием остатков | `Location` | `201 Created`, `400 BadRequest`, `404 NotFound` |
| **POST** | `/api/orders/{id}/cancel` | **Обратная операция:** отмена заказа с возвратом товаров | — | `200 OK`, `400 BadRequest`, `404 NotFound` |
| **DELETE** | `/api/orders/{id}` | Удаление заказа | — | `204 NoContent`, `404 NotFound` |

---

## ⚙️ Структура каталогов проекта

```
pr3withDB/
├── Common/                      # Базовые архитектурные модели
│   ├── PagedResult.cs           # Структура ответа с пагинацией и TotalCount
│   └── ServiceResult.cs         # Результат выполнения сервиса (Success / Failed / NotFound)
├── Controllers/
│   ├── Api/                     # Тонкие REST API контроллеры
│   │   ├── ProductsApiController.cs
│   │   ├── CategoriesApiController.cs
│   │   ├── OrdersApiController.cs
│   │   └── ...
│   ├── ProductsController.cs    # Публичный MVC каталог
│   ├── CartController.cs        # Публичная MVC корзина
│   └── AccountController.cs     # Аутентификация и авторизация
├── DTOs/                        # Контракты данных (Read / Create / Update / Patch)
│   ├── ProductDTOs.cs
│   ├── CategoryDTOs.cs
│   ├── OrderDTOs.cs
│   ├── OrderItemDTOs.cs
│   ├── UserDTOs.cs
│   └── RoleDTOs.cs
├── Mapping/                     # Профили сопоставления AutoMapper
│   └── MappingProfile.cs
├── Models/                      # Сущности Entity Framework Core и DbContext
│   ├── Product.cs
│   ├── Category.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── User.cs
│   ├── Role.cs
│   └── ShopDbContext.cs
├── Repositories/                # Слой доступа к данным (Repository Pattern)
│   ├── IRepository.cs           # Универсальный контракт CRUD и пагинации
│   ├── Repository.cs            # Базовая Generic реализация
│   ├── IOrderRepository.cs      # Контракт репозитория заказов
│   └── OrderRepository.cs       # Реализация с жадной загрузкой связанных сущностей
├── Services/                    # Слой бизнес-логики (Service Layer)
│   ├── IProductService.cs / ProductService.cs
│   ├── ICategoryService.cs / CategoryService.cs
│   └── IOrderService.cs / OrderService.cs
├── Areas/Admin/                 # Панель администратора (MVC)
├── Views/                       # Razor-представления
└── Program.cs                   # Конфигурация DI (Scoped) и Middleware
```

---

## 🛠️ Запуск проекта

### 1. Предварительные требования
* **.NET 8.0 SDK**
* СУБД **MS SQL Server** (LocalDB или SQLEXPRESS)

### 2. Настройка подключения
В файле `pr3withDB/appsettings.json` настройте строку подключения `DefaultConnection`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=ВАШ_СЕРВЕР; Database=ShopDB; Integrated Security=True; TrustServerCertificate=True"
}
```

### 3. Сборка и запуск
Выполните команды в терминале:
```powershell
# Сборка проекта
dotnet build pr3withDB/pr3withDB.csproj

# Запуск приложения
dotnet run --project pr3withDB/pr3withDB.csproj
```

### 4. Доступ к интерфейсам
* **Swagger UI (Интерактивная документация REST API):** `https://localhost:7198/swagger`
* **Каталог товаров (Публичный интерфейс):** `https://localhost:7198/`
* **Административная панель:** `https://localhost:7198/Admin/Products`
  * Логин по умолчанию: `admin`
  * Пароль по умолчанию: `admin123`
