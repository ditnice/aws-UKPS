using System.Runtime.InteropServices;

namespace UKPS.Api.Persistence.Entities.Identity;

internal abstract class StateMachine<TState, TCommand>
    where TState : notnull
    where TCommand : notnull
{
    public TState State { get; private set; }
    private List<Transition> _permittedTransitions = [];

    protected StateMachine(TState initialState)
    {
        State = initialState;
    }

    internal void DefineTransition(TState state, TCommand command, TState next)
    {
        _permittedTransitions = _permittedTransitions
            .Where(x => !(x.InitialState.Equals(state) && x.NextState.Equals(next)))
            .Append(new(state, command, next))
            .ToList();
    }

    internal void ForState(
        TState state,
        Action<StateMachineTransitionBuilder<TCommand, TState>> configure
    )
    {
        configure(new StateMachineTransitionBuilder<TCommand, TState>(state, DefineTransition));
    }

    internal void SendCommand(TCommand command)
    {
        var transitionResult = TrySendCommand(command);
        if (!transitionResult.Success)
        {
            throw new StateTransitionException(
                $"There is no state transition define from [{State}] with the [{command}] command."
            );
        }
    }

    internal StateMachineTransitionResult<TState> TrySendCommand(TCommand command)
    {
        var foundTransition = _permittedTransitions
            .Select(x =>
                x.InitialState.Equals(State) && x.Command.Equals(command) ? x : (Transition?)null
            )
            .FirstOrDefault(x => x.HasValue);
        if (foundTransition.HasValue)
        {
            State = foundTransition.Value.NextState;
            return CreateTransitionResult(true);
        }
        return CreateTransitionResult(false);
    }

    private TState[] GetPermittedNextState()
    {
        return _permittedTransitions
            .Where(x => x.InitialState.Equals(State))
            .Select(x => x.NextState)
            .ToArray();
    }

    private StateMachineTransitionResult<TState> CreateTransitionResult(bool result)
    {
        return new()
        {
            CurrentState = State,
            PermittedNextState = GetPermittedNextState(),
            Success = result,
        };
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct Transition(
        TState InitialState,
        TCommand Command,
        TState NextState
    );
}
