using Godot;
using System;

public partial class GuardPerception : Node3D
{
    protected GuardController owner;

    public virtual void Initialize(GuardController owner)
    {
        this.owner = owner;
    }

    public virtual void SetVisibility(bool value)
    {

    }

    /// <summary>
    /// Update the owning guard's awareness
    /// </summary>
    /// <returns>The delta to add to the owning guard's awareness</returns>
    public virtual float UpdateAwareness()
    {
        return 0;
    }

    /// <summary>
    /// Returns whether the player is in line of sight of this perception source
    /// </summary>
    /// <returns>Whether the player is in light of sight of this perception source</returns>
    public virtual bool IsPlayerInLineOfSight()
    {
        return false;
    }

    /// <summary>
    /// Called when the owning guard's awareness has updated
    /// </summary>
    public virtual void AwarenessUpdated()
    {

    }
}
