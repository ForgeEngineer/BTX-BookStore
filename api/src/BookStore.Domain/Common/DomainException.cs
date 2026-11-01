namespace BookStore.Domain.Common;

/// <summary>
/// Thrown when a domain invariant is violated. Mapped to 400 Bad Request by the API.
/// </summary>
public sealed class DomainException(string message) : Exception(message);