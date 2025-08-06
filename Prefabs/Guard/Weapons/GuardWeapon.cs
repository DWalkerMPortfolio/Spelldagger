using Godot;
using System;
using System.Collections.Generic;

public partial class GuardWeapon : Node3D
{
    protected GuardController owner;
    protected bool active = false;

    public virtual void Initialize(GuardController owner)
    {
        this.owner = owner;
    }

    public virtual void EnteredAlert(int previousState)
    {
        active = true;
    }

    public virtual void ExitedAlert(int nextState)
    {
        active = false;
    }
}
