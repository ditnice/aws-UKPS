namespace UKPS.Api.Persistence.StateMachines;

internal class StateTransitionException : InvalidOperationException
{
    public StateTransitionException() { }

    public StateTransitionException(string? message)
        : base(message) { }

    public StateTransitionException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
