using System.Collections.Generic;
using Lutra.Utility;

namespace Lutra.Components;

/// <summary>
/// State machine using an enum of states.
/// Each state can have an entry, exit, and update function (all optionally null).
/// The exit function is passed the next state, so it can handle transitions between states.
/// </summary>
/// <typeparam name="TState">An enum of states.</typeparam>
public class StateMachine<TState> : Component
where TState : Enum
{
    private readonly Dictionary<TState, Action> stateEntryFuncs = [];
    private readonly Dictionary<TState, Action<TState>> stateExitFuncs = [];
    private readonly Dictionary<TState, Action> stateUpdateFuncs = [];

    private Func<TState, TState, bool> canSwitchStateFunc;

    private TState currentState;
    private bool running = false;

    public TState CurrentState => currentState;
    public bool Running => running;

    public override void Update()
    {
        if (running && stateUpdateFuncs.TryGetValue(currentState, out Action update))
        {
            update.Invoke();
        }
    }

    public void RegisterState(TState state, Action entryFunc, Action<TState> exitFunc, Action updateFunc)
    {
        if (entryFunc != null)
        {
            stateEntryFuncs.Add(state, entryFunc);
        }

        if (exitFunc != null)
        {
            stateExitFuncs.Add(state, exitFunc);
        }

        if (updateFunc != null)
        {
            stateUpdateFuncs.Add(state, updateFunc);
        }
    }

    public void RegisterCanSwitchStateFunc(Func<TState, TState, bool> onCanSwitch)
    {
        canSwitchStateFunc = onCanSwitch;
    }

    public void Start(TState startState, bool skipEntry = false)
    {
        if (!running)
        {
            if (!skipEntry && stateEntryFuncs.TryGetValue(startState, out Action entry))
            {
                entry.Invoke();
            }

            currentState = startState;
            running = true;
        }
    }

    public void Stop(bool skipExit = false)
    {
        if (running)
        {
            if (!skipExit && stateExitFuncs.TryGetValue(currentState, out Action<TState> exit))
            {
                exit.Invoke(currentState);
            }

            running = false;
        }
    }

    public void SwitchState(TState nextState, bool force = false)
    {
        if (!running)
        {
            Util.Log("Tried to switch state in a non-running StateMachine!");
            return;
        }

        if (canSwitchStateFunc == null || force || canSwitchStateFunc(currentState, nextState))
        {
            SwitchStateInternal(nextState);
        }
    }

    private void SwitchStateInternal(TState nextState)
    {
        if (stateExitFuncs.TryGetValue(currentState, out Action<TState> exit))
        {
            exit.Invoke(nextState);
        }

        if (stateEntryFuncs.TryGetValue(nextState, out Action entry))
        {
            entry.Invoke();
        }

        currentState = nextState;
    }
}
