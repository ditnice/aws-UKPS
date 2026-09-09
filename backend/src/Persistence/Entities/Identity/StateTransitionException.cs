namespace UKPS.Api.Persistence.Entities.Identity;

internal class StateTransitionException : InvalidOperationException
{
    public StateTransitionException() { }

    public StateTransitionException(string? message)
        : base(message) { }

    public StateTransitionException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
