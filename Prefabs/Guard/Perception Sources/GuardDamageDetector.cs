using Godot;
using System;

public partial class GuardDamageDetector : GuardPerception
{
    [ExportGroup("Internal")]
    [Export] Damageable Damageable;

    public override void _Ready()
    {
        base._Ready();

        Damageable.Damaged += OnDamaged;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        Damageable.Damaged -= OnDamaged;
    }

    void OnDamaged(IDamageable.Teams team, Node3D source)
    {
        if (team == IDamageable.Teams.Player)
            owner.InvestigatePosition(GlobalPosition, false);
    }
}
