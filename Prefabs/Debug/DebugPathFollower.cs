using Godot;
using System;

public partial class DebugPathFollower : PathFollow2D
{
    [Export] float Speed;

    public override void _Process(double delta)
    {
        base._Process(delta);

        Progress += Speed * (float)delta * Globals.PixelsPerUnit;
    }
}
