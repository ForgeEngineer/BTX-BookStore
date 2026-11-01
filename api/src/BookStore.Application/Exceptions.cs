namespace BookStore.Application;

public sealed class NotFoundException(string message) : Exception(message);

public sealed class ConflictException(string message) : Exception(message);

public sealed class CatalogUnavailableException(string message, Exception innerException)
    : Exception(message, innerException);
