using Godot;
using System;
using System.Collections.Generic;

public partial class StateMachine : Node
{
    public delegate void EnterState(int previousState);
    public delegate void ExitState(int newState);
    public delegate void ProcessState(double delta);
    public delegate void PhysicsProcessState(double delta);
    public delegate void StateChangedDelegate(int newState, int previousState);

    public StateChangedDelegate StateChanged;

    [Export] bool Debug;

    public int CurrentState { get; private set; } = -1;
    public int PreviousState { get; private set; }

    Dictionary<int, EnterState> enterStateMethods = new Dictionary<int, EnterState>();
    Dictionary<int, ExitState> exitStateMethods = new Dictionary<int, ExitState>();
    Dictionary<int, ProcessState> processStateMethods = new Dictionary<int, ProcessState>();
    Dictionary<int, PhysicsProcessState> physicsProcessStateMethods = new Dictionary<int, PhysicsProcessState>();

    public override void _Process(double delta)
    {
        if (processStateMethods.ContainsKey(CurrentState))
            processStateMethods[CurrentState]?.Invoke(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (physicsProcessStateMethods.ContainsKey(CurrentState))
            physicsProcessStateMethods[CurrentState]?.Invoke(delta);
    }

    public void RegisterState(int state, EnterState enter = null, ExitState exit = null, ProcessState process = null, PhysicsProcessState physicsProcess = null)
    {
        if (enter != null)
            enterStateMethods[state] = enter;
        if (exit != null)
            exitStateMethods[state] = exit;
        if (process != null)
            processStateMethods[state] = process;
        if (physicsProcess != null)
            physicsProcessStateMethods[state] = physicsProcess;
    }

    public void SwitchState(int newState)
	{
        if (newState == CurrentState)
            return;

        PreviousState = CurrentState;
        CurrentState = newState;

        if (exitStateMethods.ContainsKey(PreviousState))
            exitStateMethods[PreviousState].Invoke(CurrentState);
        if (enterStateMethods.ContainsKey(CurrentState))
            enterStateMethods[CurrentState].Invoke(PreviousState);

        StateChanged?.Invoke(CurrentState, PreviousState);

        if (Debug)
            GD.Print(Name + " switching state to: " + newState);
	}
}
