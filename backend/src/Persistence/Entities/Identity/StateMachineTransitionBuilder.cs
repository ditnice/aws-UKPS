namespace UKPS.Api.Persistence.Entities.Identity;

internal class StateMachineTransitionBuilder<TCommand, TState>
    where TState : notnull
    where TCommand : notnull
{
    private readonly TState _state;
    private readonly Action<TState, TCommand, TState> _defineTransition;

    public StateMachineTransitionBuilder(
        TState state,
        Action<TState, TCommand, TState> defineTransition
    )
    {
        _state = state;
        _defineTransition = defineTransition;
    }

    internal void On(TCommand command, TState nextState)
    {
        _defineTransition(_state, command, nextState);
    }

    internal void Ignore(TCommand command)
    {
        _defineTransition(_state, command, _state);
    }
}
