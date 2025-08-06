using Godot;
using System;
using System.Collections.Generic;

public partial class GuardWeakPoint : Area3D, IDamageable
{
    GuardController owner;
    List<Node3D> damageSources = new List<Node3D>();

    public void Initialize(GuardController owner)
    {
        this.owner = owner;
    }

    public void TakeDamage(IDamageable.Teams team, Node3D source)
    {
        if (team == IDamageable.Teams.Guards)
            return;
        
        if (damageSources.Count == 0)
            owner.OnWeakpointDamaged(this);
        damageSources.Add(source);
    }

    public void DamageSourceRemoved(IDamageable.Teams team, Node3D source)
    {
        damageSources.Remove(source);
        if (damageSources.Count == 0)
            owner.OnWeakpointDamageSourceRemoved(this);
    }
}
