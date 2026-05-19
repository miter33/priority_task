# Hotel Visitation Management System

Full-stack solution to the interview brief: customers, hotels, visitations, and a loyalty analytic.

- **Backend** — .NET 8 Web API in a Clean Architecture layout with a hand-rolled CQRS pipeline.
- **Frontend** — React 18 + React Router, Jest + React Testing Library.

---

## Architecture

```
backend/
├── InterviewApi.Domain/          # Entities (Customer, Hotel, Visitation) — no deps
├── InterviewApi.Application/     # The "service layer" — see note below
│   ├── Common/Cqrs/              # IRequest, IRequestHandler, ISender
│   ├── Common/Dtos/              # Public DTOs returned across layers
│   ├── Common/Interfaces/        # Repository contracts
│   ├── Common/Exceptions/        # Domain-level exceptions (NotFound, Validation, …)
│   ├── Customers/                # Commands + Queries + Handlers per feature
│   ├── Hotels/
│   └── Visitations/
│       └── Loyalty/LoyaltyAnalyzer.cs  # Pure business rule
├── InterviewApi.Infrastructure/  # JSON repositories + reflection-based ISender
└── InterviewApi/                 # Composition root — Program.cs + controllers + middleware
```

### Why CQRS handlers ARE the service layer

The original assignment asked for `ICustomerService`, `IVisitationService`, `ILoyaltyService` — three classes that own
the business logic so controllers stay thin. In this codebase that role is played by **CQRS handlers** instead of
three coarse-grained service classes:

| Conceptual service     | Concrete handlers / classes                                                                 |
| ---------------------- | ------------------------------------------------------------------------------------------- |
| `ICustomerService`     | `ListCustomersQueryHandler`, `GetCustomerByIdQueryHandler`, `CreateCustomerCommandHandler`  |
| `IVisitationService`   | `SearchVisitationsQueryHandler`, `CreateVisitationCommandHandler`                           |
| `ILoyaltyService`      | `LoyaltyAnalyzer` (pure static rule, consumed by `SearchVisitationsQueryHandler`)           |

This satisfies the same invariants the assignment cares about:

- ✅ **Controllers are thin** — every action body is just `await _sender.Send(query)` plus an HTTP shape.
- ✅ **No LINQ over JSON in controllers** — that lives in the JSON repositories under `Infrastructure/Persistence`.
- ✅ **No inline validation** — validation lives in command handlers and throws `ValidationException`, which the
  exception middleware translates to a 400.
- ✅ **DI in `Program.cs`** — `AddApplication()` and `AddInfrastructure()` extension methods register everything.

The advantage over three big services: each handler is one feature, so a new endpoint adds one file rather than
growing a 500-line service class. The trade-off: more files. We picked CQRS because the project has multiple
distinct read shapes (loyalty search vs. list customers) that don't share much code.

---

## Endpoints

All endpoints are documented with XML doc comments + `ProducesResponseType` and surface through Swagger.
Run the API and visit **http://localhost:5000/swagger** for the rendered spec.

| Verb     | Path                                                                       | Status codes        |
| -------- | -------------------------------------------------------------------------- | ------------------- |
| GET      | `/api/customer`                                                            | 200                 |
| GET      | `/api/customer/{id}`                                                       | 200, 404            |
| POST     | `/api/customer`                                                            | 201, 400            |
| GET      | `/api/hotel`                                                               | 200                 |
| GET      | `/api/hotel/{id}`                                                          | 200, 404            |
| GET      | `/api/visitation?month=&year=&hotelIds=&onlyLoyal=`                        | 200, 400            |
| POST     | `/api/visitation`                                                          | 201, 400            |
| GET      | `/api/assignment`                                                          | 200                 |

`/api/visitation` supports multi-valued `hotelIds`: `?hotelIds=1&hotelIds=2`.

### Loyalty rule

A `(customer, hotel, weekday)` triple is **loyal** for a given month when the customer visits that hotel on
every occurrence of that weekday (e.g. all four Sundays of January, or all five Sundays of March).

---

## Running

### Backend
```bash
cd backend/InterviewApi
dotnet run --launch-profile http
# http://localhost:5000 + /swagger
```

### Frontend
```bash
cd frontend
npm install
npm start
# http://localhost:3000
```

### Tests
```bash
dotnet test backend/InterviewApi.Tests/InterviewApi.Tests.csproj
cd frontend && npm test
```

---

## Repository layout

```
.
├── backend/
│   ├── InterviewApi.Domain/
│   ├── InterviewApi.Application/
│   ├── InterviewApi.Infrastructure/
│   ├── InterviewApi/                 (API host)
│   └── InterviewApi.Tests/           (xUnit)
├── frontend/
│   └── src/
│       ├── components/<Name>/        (each: <Name>.js + .css + index.js + tests)
│       ├── pages/<Name>/             (same layout)
│       ├── api.js                    (single fetch client)
│       └── setupTests.js
├── priority-interview.sln
└── README.md
```
