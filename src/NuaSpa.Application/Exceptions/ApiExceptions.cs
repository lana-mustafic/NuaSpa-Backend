namespace NuaSpa.Application.Exceptions;

/// <summary>Resurs nije pronađen (HTTP 404).</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>Poslovno pravilo nije ispunjeno (HTTP 400).</summary>
public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}

/// <summary>Konflikt stanja (HTTP 409).</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>Pristup je odbijen (HTTP 403).</summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

/// <summary>Neautorizovan zahtjev (HTTP 401).</summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
