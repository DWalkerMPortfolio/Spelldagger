using Godot;
using System;

public interface IDamageable
{
    public enum Teams { Environment, Player, Guards }

    public void TakeDamage(Teams team, Node3D source);

    public void DamageSourceRemoved(Teams team, Node3D source)
    {

    }
}
