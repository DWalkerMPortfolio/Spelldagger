using Godot;
using System;

public partial class DebugTeleporter : Area3D
{
    [Export] Node2D Destination;

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        BodyEntered -= OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body == PlayerController.Instance)
        {
            body.GlobalPosition = new Vector3(Destination.GlobalPosition.X / Globals.PixelsPerUnit, body.GlobalPosition.Y, Destination.GlobalPosition.Y / Globals.PixelsPerUnit);
        }
    }
}
