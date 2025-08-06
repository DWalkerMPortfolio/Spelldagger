using Godot;
using System;

public partial class Rotate : Node3D
{
    [Export] Vector3 Axis;
    [Export] float Speed;

    public override void _Process(double delta)
    {
        base._Process(delta);

        RotateObjectLocal(Axis, Mathf.DegToRad(Speed * (float)delta));
    }
}
