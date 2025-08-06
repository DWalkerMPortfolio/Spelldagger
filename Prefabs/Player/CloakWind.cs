using Godot;
using System;

public partial class CloakWind : Area3D
{
    [Export] float GravityStrength;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        GravityDirection = -GlobalBasis.Z * GravityStrength;
    }
}
