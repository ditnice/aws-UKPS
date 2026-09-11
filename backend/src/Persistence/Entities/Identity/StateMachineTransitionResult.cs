namespace UKPS.Api.Persistence.Entities.Identity;

/// <summary>
/// Represents the result of attempting to transition a state machine to a new state.
/// </summary>
/// <typeparam name="TState">
/// The type used to represent a state in the state machine.
/// </typeparam>
public sealed record StateMachineTransitionResult<TState>
    where TState : notnull
{
    /// <summary>
    /// Gets a value indicating whether the state transition was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Gets the current state of the state machine after the transition attempt.
    /// </summary>
    public required TState CurrentState { get; init; }

    /// <summary>
    /// Gets the state of the state machine before the transition attempt.
    /// </summary>
    public required TState PreviousState { get; init; }

    /// <summary>
    /// Gets the states that can be transitioned to from the current state.
    /// </summary>
    public required IReadOnlyCollection<TState> PermittedNextState { get; init; }

    /// <summary>
    /// Gets a value indicating whether the state machine changed state as a result of the transition attempt.
    /// </summary>
    public bool HasChanged => !CurrentState.Equals(PreviousState);
}
