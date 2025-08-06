using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class GuardStateBehavior : Resource, ITemporalControl
{
    protected GuardController owner;

    public virtual void Initialize(GuardController controller) 
    { 
        this.owner = controller;
    }

    public virtual void EnterState(int previousState) { }

    public virtual void ExitState(int nextState) { }

    public virtual void ProcessState(double delta) { }

    public virtual void PhysicsProcessState(double delta) { }

    public virtual string[] GetTemporalProperties()
    {
        return null;
    }

    public virtual Dictionary<string, Variant> SaveCustomTemporalState()
    {
        return null;
    }

    public virtual void RestoreCustomTemporalState(Dictionary<string, Variant> customData) { }
}
