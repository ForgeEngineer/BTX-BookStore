FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY src/BookStore.Domain/BookStore.Domain.csproj src/BookStore.Domain/
COPY src/BookStore.Application/BookStore.Application.csproj src/BookStore.Application/
COPY src/BookStore.Infrastructure/BookStore.Infrastructure.csproj src/BookStore.Infrastructure/
COPY src/BookStore.Api/BookStore.Api.csproj src/BookStore.Api/
RUN dotnet restore src/BookStore.Api/BookStore.Api.csproj
COPY src/ src/
RUN dotnet publish src/BookStore.Api/BookStore.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "BookStore.Api.dll"]
