using Godot;
using System;

public partial class HazardVolume : Area3D
{
    [Export] public bool Active = true;
    [Export] IDamageable.Teams Team;

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
        if (Active)
            CallDeferred(MethodName.OverlappingBodies); // Handle bodies already inside the area when this hazard spawned
    }

    async void OverlappingBodies()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        foreach (Node3D body in GetOverlappingBodies())
        {
            OnBodyEntered(body);
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if (!Active)
            return;

        IDamageable damageableBody = body as IDamageable;
        if (damageableBody != null)
        {
            damageableBody.TakeDamage(Team, this);
        }
    }

    private void OnAreaEntered(Area3D area)
    {
        if (!Active)
            return;

        IDamageable damageableArea = area as IDamageable;
        if (damageableArea != null)
        {
            damageableArea.TakeDamage(Team, this);
        }
    }

    private void OnBodyExited(Node3D body)
    {
        IDamageable damageableBody = body as IDamageable;
        if (damageableBody != null)
        {
            damageableBody.DamageSourceRemoved(Team, this);
        }
    }

    private void OnAreaExited(Area3D area)
    {
        IDamageable damageableArea = area as IDamageable;
        if (damageableArea != null)
        {
            damageableArea.DamageSourceRemoved(Team, this);
        }
    }
}
