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

## Deploy to AWS

### Architecture

```
browser -> CloudFront -> S3                     Angular app (private bucket, Origin Access Control)
browser -> CloudFront /api/* -> ALB -> ECS Fargate (API) -> RDS PostgreSQL
                                       ECS Fargate (API) -> Open Library
```

- One CDK stack (`BookStoreStack`, C#) in `infra/`, deployed to us-east-1.
- VPC with 2 AZs, public and isolated subnets, no NAT gateway.
- The API task runs in a public subnet with a public IP so it can reach Open Library and pull its image. It only accepts traffic from the ALB, and the ALB only accepts traffic from CloudFront.
- RDS PostgreSQL 17 (db.t4g.micro, 20 GB, single AZ) runs in the isolated subnets and only accepts traffic from the API task. Its credentials are generated in Secrets Manager, and the password reaches the task as an ECS secret.
- A CloudFront Function rewrites non-file paths to `/index.html` for SPA routing, so API 404s are not touched.
- A reviewer IAM user `btx-reviewer` gets read-only console access until 2026-11-25.

### Prerequisites

- AWS CLI with a `bookstore` profile for an account bootstrapped with CDK in us-east-1
- Docker running (the API image is built locally and pushed by CDK)
- .NET 10 SDK and Node.js

### Deploy

```
./infra/deploy.ps1
```

It builds the Angular app for production and runs `cdk deploy --profile bookstore`. When it finishes, it prints these outputs:

- `CloudFrontUrl`: the app
- `ConsoleSignInUrl`: the console sign-in page for the reviewer
- `ReviewerPasswordSecretName`: the secret that holds the reviewer's first password

Read the reviewer password:

```
aws secretsmanager get-secret-value --secret-id <ReviewerPasswordSecretName> --query SecretString --output text --profile bookstore --region us-east-1
```

The reviewer signs in as `btx-reviewer` and must change the password on first sign-in.

### Destroy

```
./infra/destroy.ps1
```

It runs `cdk destroy --profile bookstore --force` and removes every resource in the stack, including the database (no final snapshot), the bucket and its objects, and the API logs. The CDK helper Lambdas (S3 auto-delete, default security group cleanup) can leave small `/aws/lambda/BookStoreStack-*` log groups behind. They cost almost nothing and can be deleted in the console. The CDK bootstrap bucket and ECR repository also keep the uploaded assets.

### Cost

- There is no NAT gateway, which would be the biggest fixed cost.
- The ALB, the RDS instance, the Fargate task and the public IPv4 addresses bill by the hour whether or not anyone uses the app.
- Destroy the stack after the demo.

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

infra/
  src/BookStore.Infra/         CDK app and BookStoreStack
  deploy.ps1, destroy.ps1

.github/workflows/ci.yml       CI pipeline
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
- **AWS with CDK in C#:** the infrastructure is in the same language as the API. It is sized for the lowest cost that still looks like a real setup, and everything is removed by `cdk destroy`.
- **CI:** GitHub Actions runs on every push and pull request. It builds and tests the API, builds and tests the web app, builds both Docker images and runs `cdk synth`. It needs no AWS credentials and does not deploy.
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
