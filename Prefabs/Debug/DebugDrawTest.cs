using Godot;
using System;

public partial class DebugDrawTest : Node3D
{
    public override void _Process(double delta)
    {
        base._Process(delta);

        DebugDraw3D.DrawSphere(GlobalPosition);
    }
}
