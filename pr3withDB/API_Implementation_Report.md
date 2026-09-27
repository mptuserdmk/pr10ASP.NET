# Отчет о реализации RESTful Web API с DTO и полным CRUD

## 1. Описание выполненных шагов

В рамках задачи в проекте ASP.NET Core 8.0 MVC было разработано полноценное RESTful Web API для сущности **Product** с соблюдением строгих ограничений архитектуры:
- **Изоляция Entity-слоя**: Сущности Entity Framework Core (`Product`) полностью скрыты от внешнего потребителя. Взаимодействие происходит исключительно через типизированные DTO.
- **Поддержка HTTP-кодов**: Реализованы стандартные статус-коды REST:
  - `200 OK` — для успешного получения списка и единичной записи;
  - `201 Created` — для успешного создания нового ресурса с заголовком `Location`;
  - `204 NoContent` — для успешного обновления (`PUT`, `PATCH`) и удаления (`DELETE`);
  - `400 BadRequest` — при непройденной валидации модели;
  - `404 NotFound` — при отсутствии запрашиваемого ресурса в базе данных.
- **Маппинг**: Создан статический маппер `ProductMapper`, выполняющий трансформацию между DTO и сущностями БД.
- **Swagger / OpenAPI**: Подключена и сконфигурирована спецификация Swagger UI для удобного интерактивного тестирования всех эндпоинтов API.

---

## 2. Список созданных и модифицированных файлов

1. [`DTOs/ProductDTOs.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/ProductDTOs.cs):
   - `ProductDTO` — модель для чтения данных (все поля, включая ID);
   - `CreateProductDTO` — модель для создания товара (без ID);
   - `UpdateProductDTO` — модель для полного обновления (PUT, все поля обязательны);
   - `PatchProductDTO` — модель для частичного обновления (PATCH, все поля nullable).
2. [`DTOs/ProductMapper.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/DTOs/ProductMapper.cs):
   - Статические методы: `ToDto`, `ToEntity`, `ApplyUpdate`, `ApplyPatch`.
3. [`Controllers/Api/ProductsApiController.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Controllers/Api/ProductsApiController.cs):
   - Контроллер `ProductsApiController` с маршрутом `api/products` и 6 CRUD-эндпоинтами.
4. [`Program.cs`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/Program.cs):
   - Подключение Swagger (`AddEndpointsApiExplorer`, `AddSwaggerGen`, `UseSwagger`, `UseSwaggerUI`) и маппинг контроллеров (`MapControllers`).
5. [`pr3withDB.csproj`](file:///c:/Users/User/source/repos/pr3withDB/pr3withDB/pr3withDB.csproj):
   - Добавлен пакет `Swashbuckle.AspNetCore`.

---

## 3. Описание реализованных эндпоинтов

| HTTP Метод | Маршрут | Описание | Ожидаемый статус-код |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/products?page=1&pageSize=10` | Получение списка товаров с пагинацией и сортировкой | `200 OK` |
| **GET** | `/api/products/{id}` | Получение товара по ID | `200 OK` / `404 NotFound` |
| **POST** | `/api/products` | Создание нового товара | `201 Created` / `400 BadRequest` |
| **PUT** | `/api/products/{id}` | Полная замена данных товара | `204 NoContent` / `404 NotFound` |
| **PATCH** | `/api/products/{id}` | Частичное обновление полей товара | `204 NoContent` / `404 NotFound` |
| **DELETE** | `/api/products/{id}` | Удаление товара по ID | `204 NoContent` / `404 NotFound` |

---

## 4. Шаблоны для скриншотов из Swagger UI

> Запустите проект командой `dotnet run` (или нажмите F5 в Visual Studio), перейдите по адресу `https://localhost:<порт>/swagger` и протестируйте методы, вставив соответствующие скриншоты ниже.

### 4.1. GET /api/products — Получение списка (200 OK)
> *Выполните запрос с параметрами `page=1` и `pageSize=10`.*

```
[ВСТАВИТЬ СКРИНШОТ: GET /api/products -> 200 OK]
```

---

### 4.2. GET /api/products/{id} — Получение по ID (200 OK и 404 NotFound)
> *Выполните запрос с существующим ID, затем с несуществующим ID (например, 99999).*

```
[ВСТАВИТЬ СКРИНШОТ: GET /api/products/{id} -> 200 OK]
```

```
[ВСТАВИТЬ СКРИНШОТ: GET /api/products/99999 -> 404 Not Found]
```

---

### 4.3. POST /api/products — Создание товара (201 Created)
> *Передайте JSON нового товара без Id. Проверьте статус 201 и заголовок Location в ответе.*

```
[ВСТАВИТЬ СКРИНШОТ: POST /api/products -> 201 Created]
```

---

### 4.4. PUT /api/products/{id} — Полное обновление (204 No Content)
> *Передайте все поля товара с обновленными данными для существующего ID.*

```
[ВСТАВИТЬ СКРИНШОТ: PUT /api/products/{id} -> 204 No Content]
```

---

### 4.5. PATCH /api/products/{id} — Частичное обновление (204 No Content)
> *Передайте только изменяемое поле (например, только `productPrice` или `productStockQuantity`).*

```
[ВСТАВИТЬ СКРИНШОТ: PATCH /api/products/{id} -> 204 No Content]
```

---

### 4.6. DELETE /api/products/{id} — Удаление товара (204 No Content)
> *Выполните удаление созданного ранее тестового товара.*

```
[ВСТАВИТЬ СКРИНШОТ: DELETE /api/products/{id} -> 204 No Content]
```

---

## 5. Инструкция по запуску

1. Запустите проект в терминале:
   ```bash
   dotnet run --project pr3withDB/pr3withDB.csproj
   ```
2. Откройте Swagger UI в браузере:
   `https://localhost:<порт>/swagger`
3. Выполните запросы и прикрепите скриншоты ответов в разделы выше.
