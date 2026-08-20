# FunBooksAndVideos

An e-commerce shop where customers can buy books, watch online videos, and purchase
club memberships (Book Club, Video Club, or Premium). Implemented as a REST API
(no UI) using Clean Architecture, .NET 10, and Dapper over SQL Server.

## Architecture

```
src/
  FunBooksAndVideos.Domain          # Entities, business logic, no dependencies
  FunBooksAndVideos.Application     # Use cases, business rules, repository abstractions
  FunBooksAndVideos.Infrastructure  # Dapper repositories, SQL Server, schema bootstrap
  FunBooksAndVideos.Api             # ASP.NET Core controllers, middleware, DI composition
tests/
  FunBooksAndVideos.UnitTests       # xUnit + Moq + Shouldly
```

Dependencies point inward: `Api → Application → Domain`, `Infrastructure → Application`.

### Design patterns

| Pattern | Where | Why |
|---|---|---|
| **Observer (Publish/Subscribe)** | `IDomainEventDispatcher` publishes `PurchaseOrderCreated`; rules implement `IDomainEventHandler<PurchaseOrderCreated>` | Processor publishes a domain event and knows nothing about the rules; each handler self-selects and reacts independently |
| **Repository** | `I*Repository` in Application, Dapper implementations in Infrastructure | Persistence abstracted away from business logic |
| **Unit of Work** | `IUnitOfWork` / `DbSession` | Order persistence + rule side effects are atomic (one transaction) |
| **Factory methods** | `LineItem.ForProduct` / `LineItem.ForMembership` | Invalid line states are unrepresentable |

### Business rules

- **BR1** – `MembershipActivationRule`: memberships in the order are activated on the
  customer account immediately. Owning both individual clubs upgrades to Premium.
- **BR2** – `ShippingSlipRule`: a shipping slip is generated for physical products
  (books). Online videos are digital and are not shipped.

Rules subscribe to the `PurchaseOrderCreated` domain event, published by
`PurchaseOrderProcessor` inside its transaction — if any handler fails,
the whole order is rolled back.

**Adding a new rule:** implement `IDomainEventHandler<PurchaseOrderCreated>` and register it in
`Application/DependencyInjection.cs`. Nothing else changes.

## Start the project

```bash
# setup MS SQL Server
docker compose up -d

# run app (database, tables and seed data are created automatically at startup)
dotnet restore
dotnet build
dotnet run --project src/FunBooksAndVideos.Api
```

Swagger UI is available at `/swagger` in Development.

### Example: process a purchase order

```http
POST /api/purchase-orders
Content-Type: application/json

{
  "customerId": 1,
  "lineItems": [
    { "productId": 3 },
    { "productId": 1 },
    { "membershipType": "BookClub" }
  ]
}
```

Line 1 is the video "Comprehensive First Aid Training", line 2 the book
"The Girl on the train", line 3 a Book Club Membership — the example order
from the task description.

Response `201 Created`: the order with computed total. Side effects: the customer
gets the Book Club membership (BR1) and a shipping slip is generated containing
only the physical book (BR2).

Errors are returned as RFC 7807 `ProblemDetails` (`400` validation, `404` not found).

## Clean up

```bash
docker compose down -v
```
