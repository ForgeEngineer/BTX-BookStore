# Book Store

## Overview

I built a small book store for this technical assessment. You search Open Library through the API, pick a book and add its genre, price and stock. The book is then stored in PostgreSQL and its data is never fetched from Open Library again, though the browser still loads cover images from Open Library's URLs.

## Prerequisites

- Docker (with Docker Compose)
- .NET 10 SDK
- Node.js `^20.19.0`, `^22.12.0` or `>=24.0.0` (the range Angular 21 supports)
- npm

## Run with Docker

```
docker compose up --build
```

- App: http://localhost:8090
- API: http://localhost:5090 (health check at http://localhost:5090/health)
- Grafana: http://localhost:3100. Traces are in Tempo, logs in Loki and metrics in Mimir/Prometheus.
- OTLP: gRPC on localhost:14317 and HTTP on localhost:14318.

`docker compose down` stops everything. The database is kept in the `pgdata` volume.

## Run locally

Start only the database:

```
docker compose up -d db
```

Postgres is on localhost:5433.

Then run the API:

```
cd api/src/BookStore.Api
dotnet run
```

- It listens on http://localhost:5090.
- Migrations are applied on startup and the database is seeded with 8 books.
- No API keys are needed.

Then run the web app:

```
cd web
npm install
npx ng serve
```

- It runs on http://localhost:4300 and forwards `/api` to http://localhost:5090 (`web/proxy.conf.json`).

Config:

- Connection string: `ConnectionStrings:BookStore`, set in `api/src/BookStore.Api/appsettings.Development.json` for local runs and through the `ConnectionStrings__BookStore` environment variable in Docker.
- OpenTelemetry: set `OTEL_EXPORTER_OTLP_ENDPOINT` to export traces, metrics and logs over OTLP. Without it nothing is exported.
- Open Library base URL: not in `appsettings.json`. I set it in `api/src/BookStore.Infrastructure/DependencyInjection.cs`, along with the 10 second timeout.

## Migrations

```
cd api
dotnet tool restore
dotnet ef migrations add <Name> -p src/BookStore.Infrastructure -s src/BookStore.Api -o Migrations
```

## Tests

```
cd api
dotnet test
```

```
cd web
npx ng test
```

`npx ng test` stays in watch mode in a terminal. Add `--watch=false` for a single run.

## Project structure

```
api/
  src/
    BookStore.Domain/          Book entity, Genre, validation
    BookStore.Application/     IAppDbContext, IBookCatalog, DTOs
      Features/
        Books/                 one file per slice
        Catalog/
    BookStore.Infrastructure/  EF Core, migrations, seeding, Open Library client
    BookStore.Api/             FastEndpoints endpoints and validators, error handling, startup
  tests/
    BookStore.Tests/

web/src/app/
  domain/                      models, abstract repositories
  application/                 BookStore signal store
  infrastructure/              HTTP repositories, API URL
  ui/                          header, body, footer, book-dialog

docker-compose.yml             db, api, web, lgtm
```

## Hand-written query

The genre count query is the PostgreSQL function `get_genre_counts()`, created in the `AddGenreCountsFunction` migration in `api/src/BookStore.Infrastructure/Migrations`. The handler in `api/src/BookStore.Application/Features/Books/GetGenreCounts.cs` calls it with `Database.SqlQuery`. It is exposed at `GET /api/books/genres`.

## Decisions and trade-offs

- **MediatR 12.5.0:** the last Apache-2.0 version, so there is no commercial license concern.
- **Vertical slices:** they live inside the Application layer, with one file per slice (request + handler).
- **FastEndpoints:** I use it for the BFF API (one class per endpoint, REPR pattern). Endpoints stay thin and dispatch to MediatR handlers in the Application layer.
- **No generic repository:** handlers use `IAppDbContext` directly instead of a generic repository over EF Core.
- **PostgreSQL:** price is stored as `numeric(10,2)`, so it stays exact and sorts correctly. The genre counts come from a stored function.
- **EF migrations:** the schema and the stored function are created by migrations, applied on startup before seeding. `dotnet-ef` is a local tool in `api/dotnet-tools.json`.
- **Containers:** the API and the web app have multi-stage Dockerfiles. The API runs as a non-root user on port 8080. nginx serves the Angular app and proxies `/api` to the API, so the web app uses the relative path `/api` and CORS is not needed.
- **OpenTelemetry:** the API sends traces (ASP.NET Core, HttpClient, Npgsql), metrics (ASP.NET Core, HttpClient, runtime) and logs over OTLP to the Grafana LGTM stack. Serilog still writes to the console and forwards its logs to OpenTelemetry.
- **Tests on SQLite:** the handler tests use SQLite in memory, so they run without a database server.
- **Open Library:** it sits behind `IBookCatalog` and is proxied by the API, with a timeout and a 503 when it is unavailable. Search results are never saved.
- **Shared state:** Header and Body are siblings, so they share state through a root-provided signal store (`BookStore`) instead of inputs/outputs.
- **Repositories:** they are abstract classes used as DI tokens. Only the infrastructure layer knows about `HttpClient` and the API URL.
- **Paging and sorting:** both are done server-side.

## AI-assisted development

I built this with Claude Code as a pair programmer. I reviewed and committed every change myself.

## Known limitations

- Two simultaneous adds of the same book can return 500 instead of 409. The unique index on the Open Library key catches it.
- ISBN is not unique in the database.
- Catalog results without an author cannot be added.

## What I would do next

- Optimistic concurrency on updates
- A correlation ID for the Open Library call
- Tests for the dialog and HTTP repositories
- Keeping the genre filter in the URL
- Genre counts in the dropdown
