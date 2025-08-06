using Godot;
using System;

public partial class Damageable : Area3D, IDamageable
{
    public delegate void DamagedDelegate(IDamageable.Teams team, Node3D source);
    public DamagedDelegate Damaged;

    public void TakeDamage(IDamageable.Teams team, Node3D source)
    {
        Damaged.Invoke(team, source);
    }
}
