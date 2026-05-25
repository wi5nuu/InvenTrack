namespace InvenTrack.Domain.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Access denied.") : base(message)
    {
    }
}
