# Отчет о реализации RESTful Web API с DTO и полным CRUD

## 1. Описание выполненных шагов

В рамках задачи в проекте ASP.NET Core 8.0 с использованием существующей базы данных и моделей Entity Framework Core было разработано полноценное RESTful Web API для всех сущностей предметной области (`Product`, `Category`, `Role`, `User`, `Order`, `OrderItem`, `Cart`, `CartItem`) с соблюдением строгих архитектурных требований:

1. **Изоляция Entity-слоя через паттерн DTO**:
   - Сущности Entity Framework Core полностью изолированы от внешнего потребителя.
   - Для каждой сущности созданы специализированные DTO-классы:
     - `{Entity}DTO` — возврат данных клиенту (GET). Содержит все поля модели, включая `Id` и `CreatedAt` (где применимо);
     - `Create{Entity}DTO` — создание записи (POST). Содержит поля модели, кроме `Id` и `CreatedAt` (эти поля формируются на сервере);
     - `Update{Entity}DTO` — полное обновление (PUT). Все поля обязательные с валидационными атрибутами;
     - `Patch{Entity}DTO` — частичное обновление (PATCH). Все поля объявлены как nullable (`?`), чтобы передавать только изменяемые свойства.
2. **Статические мапперы**:
   - Для каждой сущности реализован статический класс `{Entity}Mapper` с методами `ToDto`, `ToEntity`, `ApplyUpdate` и `ApplyPatch`.
   - Для пользователей пароли хэшируются с использованием алгоритма `BCrypt` и не передаются в DTO ответа.
   - Даты создания (`CreatedAt` / `OrderDate` / `UserCreatedAt` / `CartCreatedAt`) устанавливаются на сервере (`DateTime.UtcNow`).
3. **Контроллеры API и маршрутизация**:
   - Контроллеры размещены по маршрутам `api/{entities}` и используют контекст базы данных `ShopDbContext`, внедренный через Dependency Injection.
   - Для каждого контроллера реализованы 6 стандартных эндпоинтов:
     1. `GET api/{entities}?page=1&pageSize=10` — пагинация и сортировка по Id (`200 OK`);
     2. `GET api/{entities}/{id}` — получение по первичному ключу (`200 OK` или `404 NotFound`);
     3. `POST api/{entities}` — создание записи (`201 Created` с `CreatedAtAction` на `GetById`);
     4. `PUT api/{entities}/{id}` — полное обновление всех полей (`204 NoContent` или `404 NotFound`);
     5. `PATCH api/{entities}/{id}` — частичное обновление полей, не равных null (`204 NoContent` или `404 NotFound`);
     6. `DELETE api/{entities}/{id}` — удаление записи (`204 NoContent` или `404 NotFound`).
4. **Интерактивная документация Swagger / OpenAPI**:
   - Настроена генерация документации через Swashbuckle, позволяющая выполнять интерактивное тестирование всех CRUD-операций через веб-интерфейс Swagger UI.

---

## 2. Архитектура созданных DTO, мапперов и контроллеров

| Сущность | Маршрут API | Файл DTO | Файл Маппера | Файл Контроллера |
|---|---|---|---|---|
| **Product** | `/api/products` | [`DTOs/ProductDTOs.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/ProductDTOs.cs) | [`DTOs/ProductMapper.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/ProductMapper.cs) | [`Controllers/Api/ProductsApiController.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Controllers/Api/ProductsApiController.cs) |
| **Category** | `/api/categories` | [`DTOs/CategoryDTOs.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/CategoryDTOs.cs) | [`DTOs/CategoryMapper.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/CategoryMapper.cs) | [`Controllers/Api/CategoriesApiController.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Controllers/Api/CategoriesApiController.cs) |
| **Role** | `/api/roles` | [`DTOs/RoleDTOs.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/RoleDTOs.cs) | [`DTOs/RoleMapper.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/RoleMapper.cs) | [`Controllers/Api/RolesApiController.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Controllers/Api/RolesApiController.cs) |
| **User** | `/api/users` | [`DTOs/UserDTOs.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/UserDTOs.cs) | [`DTOs/UserMapper.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/UserMapper.cs) | [`Controllers/Api/UsersApiController.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Controllers/Api/UsersApiController.cs) |
| **Order** | `/api/orders` | [`DTOs/OrderDTOs.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/OrderDTOs.cs) | [`DTOs/OrderMapper.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/OrderMapper.cs) | [`Controllers/Api/OrdersApiController.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Controllers/Api/OrdersApiController.cs) |
| **OrderItem** | `/api/order-items` | [`DTOs/OrderItemDTOs.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/OrderItemDTOs.cs) | [`DTOs/OrderItemMapper.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/OrderItemMapper.cs) | [`Controllers/Api/OrderItemsApiController.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Controllers/Api/OrderItemsApiController.cs) |
| **Cart** | `/api/carts` | [`DTOs/CartDTOs.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/CartDTOs.cs) | [`DTOs/CartMapper.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/CartMapper.cs) | [`Controllers/Api/CartsApiController.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Controllers/Api/CartsApiController.cs) |
| **CartItem** | `/api/cart-items` | [`DTOs/CartItemDTOs.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/CartItemDTOs.cs) | [`DTOs/CartItemMapper.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/CartItemMapper.cs) | [`Controllers/Api/CartItemsApiController.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Controllers/Api/CartItemsApiController.cs) |

---

## 3. Таблица эндпоинтов REST API

| Метод | Маршрут | Описание | Код успешного ответа | Коды ошибок |
|---|---|---|---|---|
| **GET** | `/api/{entity}?page=1&pageSize=10` | Получение списка сущностей с пагинацией и сортировкой по ID | `200 OK` | `400 BadRequest` |
| **GET** | `/api/{entity}/{id}` | Получение конкретной сущности по ID | `200 OK` | `404 NotFound` |
| **POST** | `/api/{entity}` | Создание новой записи (принимает `Create{Entity}DTO`) | `201 Created` (`Location: GetById`) | `400 BadRequest` |
| **PUT** | `/api/{entity}/{id}` | Полное обновление (принимает `Update{Entity}DTO`) | `204 NoContent` | `400 BadRequest`, `404 NotFound` |
| **PATCH** | `/api/{entity}/{id}` | Частичное обновление полей (принимает `Patch{Entity}DTO`) | `204 NoContent` | `400 BadRequest`, `404 NotFound` |
| **DELETE** | `/api/{entity}/{id}` | Удаление записи по ID | `204 NoContent` | `404 NotFound` |

---

## 4. Шаблоны для скриншотов из Swagger UI / Postman

> Запустите проект командой `dotnet run --project pr3withDB/pr3withDB.csproj` и откройте Swagger UI по адресу `https://localhost:<порт>/swagger`. Выполните запросы для сущностей и прикрепите снимки экрана в соответствующие блоки.

### 4.1. Категории (`/api/categories`)
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/categories -> 200 OK (список)]
```
```
[ВСТАВИТЬ СКРИНШОТ: POST /api/categories -> 201 Created]
```
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/categories/{id} -> 200 OK]
```
```
[ВСТАВИТЬ СКРИНШОТ: PUT /api/categories/{id} -> 204 No Content]
```
```
[ВСТАВИТЬ СКРИНШОТ: PATCH /api/categories/{id} -> 204 No Content]
```
```
[ВСТАВИТЬ СКРИНШОТ: DELETE /api/categories/{id} -> 204 No Content]
```

### 4.2. Роли (`/api/roles`)
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/roles -> 200 OK]
```
```
[ВСТАВИТЬ СКРИНШОТ: POST /api/roles -> 201 Created]
```

### 4.3. Пользователи (`/api/users`)
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/users -> 200 OK (хэш скрыт, есть CreatedAt)]
```
```
[ВСТАВИТЬ СКРИНШОТ: POST /api/users -> 201 Created (с паролем)]
```
```
[ВСТАВИТЬ СКРИНШОТ: PATCH /api/users/{id} -> 204 No Content]
```

### 4.4. Товары (`/api/products`)
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/products -> 200 OK]
```
```
[ВСТАВИТЬ СКРИНШОТ: POST /api/products -> 201 Created]
```
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/products/99999 -> 404 Not Found]
```

### 4.5. Заказы и позиции (`/api/orders`, `/api/order-items`)
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/orders -> 200 OK]
```
```
[ВСТАВИТЬ СКРИНШОТ: POST /api/orders -> 201 Created]
```
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/order-items -> 200 OK]
```

### 4.6. Корзина и позиции корзины (`/api/carts`, `/api/cart-items`)
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/carts -> 200 OK]
```
```
[ВСТАВИТЬ СКРИНШОТ: GET /api/cart-items -> 200 OK]
```

---

## 5. Инструкция по запуску

1. Сборка проекта:
   ```bash
   dotnet build pr3withDB/pr3withDB.csproj
   ```
2. Запуск приложения:
   ```bash
   dotnet run --project pr3withDB/pr3withDB.csproj
   ```
3. Интерактивная документация Swagger:
   Откройте в браузере: `https://localhost:<порт>/swagger`
