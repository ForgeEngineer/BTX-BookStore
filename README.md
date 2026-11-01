# Book Store

## Overview

I built a small book store for this technical assessment. You search Open Library through the API, pick a book and add its genre, price and stock. The book is then stored in SQLite and its data is never fetched from Open Library again, though the browser still loads cover images from Open Library's URLs.

## Prerequisites

- .NET 10 SDK
- Node.js `^20.19.0`, `^22.12.0` or `>=24.0.0` (the range Angular 21 supports)
- npm

## Run the API

```
cd api/src/BookStore.Api
dotnet run
```

- It listens on http://localhost:5080.
- The SQLite file (`bookstore.db`) is created in that folder and seeded with 8 books on startup.
- No API keys are needed.

Config:

- Connection string: `ConnectionStrings:Default` in `api/src/BookStore.Api/appsettings.json`.
- Open Library base URL: not in `appsettings.json`. I set it in `api/src/BookStore.Infrastructure/DependencyInjection.cs`, along with the 10 second timeout.

## Run the web app

The API must be running first.

```
cd web
npm install
npx ng serve
```

- It runs on http://localhost:4200. The API's CORS policy only allows this origin, so keep the port.
- The API URL is in `web/src/app/infrastructure/api-url.ts`.

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
    BookStore.Infrastructure/  EF Core, seeding, Open Library client
    BookStore.Api/             endpoints, error handling, startup
  tests/
    BookStore.Tests/

web/src/app/
  domain/                      models, abstract repositories
  application/                 BookStore signal store
  infrastructure/              HTTP repositories, API URL
  ui/                          header, body, footer, book-dialog
```

## Hand-written query

The raw SQL genre count query is in `api/src/BookStore.Application/Features/Books/GetGenreCounts.cs`. It is exposed at `GET /api/books/genres`.

## Decisions and trade-offs

- **MediatR 12.5.0:** the last Apache-2.0 version, so there is no commercial license concern.
- **Vertical slices:** they live inside the Application layer, with one file per slice (request + handler).
- **No generic repository:** handlers use `IAppDbContext` directly instead of a generic repository over EF Core.
- **`EnsureCreated`:** I used it instead of migrations to keep setup simple.
- **Price as double:** it is stored as a double in SQLite so sorting by price works.
- **Open Library:** it sits behind `IBookCatalog` and is proxied by the API, with a timeout and a 503 when it is unavailable. Search results are never saved.
- **Shared state:** Header and Body are siblings, so they share state through a root-provided signal store (`BookStore`) instead of inputs/outputs.
- **Repositories:** they are abstract classes used as DI tokens. Only the infrastructure layer knows about `HttpClient` and the API URL.
- **Paging and sorting:** both are done server-side.

## Known limitations

- Two simultaneous adds of the same book can return 500 instead of 409. The unique index on the Open Library key catches it.
- ISBN is not unique in the database.
- Catalog results without an author cannot be added.

## What I would do next

- EF migrations
- Optimistic concurrency on updates
- A correlation ID for the Open Library call
- Tests for the dialog and HTTP repositories
- Keeping the genre filter in the URL
- Genre counts in the dropdown
