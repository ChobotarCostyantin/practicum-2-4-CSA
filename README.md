# Nimble.Modulith

**Nimble.Modulith** — це [демонстраційний лабораторний проект](https://github.com/NimblePros/ModularMonolithWorkshop), що ілюструє побудову архітектури **Modular Monolith** на платформі .NET 10.

Проект розділений на ізольовані, слабо пов'язані модулі, кожен з яких відповідає за певну бізнес-доменну область, має власну базу даних (або схему) та комунікує з іншими частинами системи виключно через чітко визначені контракти та події.

## Стек технологій та патернів

- **Framework:** .NET 10
- **Orchestration:** [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- **API Architecture:** [FastEndpoints](https://fast-endpoints.com/) (реалізація патерну REPR — Request-Endpoint-Response)
- **Міжмодульна комунікація:**
    - **Синхронна:** `Mediator` (Source Generated) для внутрішніх запитів (наприклад, отримання ціни з модуля Products).
    - **Асинхронна (Event-Driven):** Domain/Integration Events для публікації подій (наприклад, сповіщення про створення замовлення).
- **Робота з даними:**
    - **Entity Framework Core:** з підтримкою Resilience/Retry політик.
    - **Dapper:** для високоефективних аналітичних запитів.
    - **MS SQL Server:** як база даних.

## Структура рішення

Проект реалізує суворе розділення на контракти та реалізацію для запобігання неявному зв'язуванню модулів:

```text
Nimble.Modulith.Solution/
├── Nimble.Modulith.ModuleName/           # Implementation (Logic, Data, API)
└── Nimble.Modulith.ModuleName.Contracts/ # Contracts (DTOs, Commands, Events)
```

## Опис модулів

1.  **Users:** Управління обліковими записами, JWT-автентифікація та адміністрування ролей.
2.  **Products:** Управління каталогом товарів, описом та ціноутворенням.
3.  **Customers:** Профілі клієнтів та повний життєвий цикл управління замовленнями.
4.  **Email:** Асинхронна відправка повідомлень через фоновий воркер та чергу (BackgroundService).
5.  **Reporting:** Аналітичний модуль, що використовує Star Schema та Dapper для формування звітів у JSON та CSV.

## Архітектурні рішення

### Розділення на Contracts та Implementation

Кожен модуль розділений на два проекти. `Contracts` містить лише описи інтерфейсів, команд та подій. `Implementation` містить бізнес-логіку та доступ до даних. Це унеможливлює пряме звернення одного модуля до внутрішніх деталей іншого.

### Автономність даних

Кожен модуль володіє власною схемою в `SQL Server` (Users, Products, Customers, Reporting). Прямі JOIN-запити між таблицями різних модулів заборонені. Доступ до даних іншого модуля можливий лише через `MediatR Query` або синхронізацію через події.

### Патерн Producer-Consumer (Email Module)

Відправка пошти не блокує основний потік API. Повідомлення потрапляють у чергу `Channel<T>` і обробляються фоновим процесом (`BackgroundService`). Це підвищує продуктивність системи та стійкість до збоїв SMTP-сервера.

### Модель аналітики (Star Schema)

Модуль Reporting використовує денормалізовану схему "Зірка":

* **Dimensions:** DimDate, DimCustomer, DimProduct.
* **Facts:** FactOrders.

Дані потрапляють у сховище через обробку події `OrderCreatedEvent`. Для вибірки використовується Dapper, що забезпечує максимальну швидкість агрегації даних.

## Архітектурні діаграми (C4 Model)

### System Context

![System context](./docs/system-context.png)

### Container

![Container](./docs/container.png)

### Component

![Component](./docs/component.png)

-----

## Інструкція з розгортання

### Попередні вимоги

* .NET 10 SDK
* Docker Desktop

### Запуск

1.  Клонуйте репозиторій:
    ```bash
    git clone [https://github.com/TeseySTD/practicum-2-4-Enterprise-Arch-](https://github.com/TeseySTD/practicum-2-4-Enterprise-Arch-)
    cd practicum-2-4-Enterprise-Arch
    ```
2.  Виконайте збірку:
    ```bash
    dotnet build
    ```
3.  Запустіть проект через Aspire AppHost:
    ```bash
    dotnet run --project Nimble.Modulith.AppHost/Nimble.Modulith.AppHost.csproj
    ```
4.  Перейдіть до Aspire Dashboard за посиланням у консолі для доступу до:
    * **webapi**: Swagger UI для тестування API.
    * **papercut**: Інтерфейс для перегляду відправлених листів.