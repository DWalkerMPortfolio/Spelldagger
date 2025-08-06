using Godot;
using System;

public partial class RoguelikeTeleporter : Area3D
{
    [Export] RoguelikeController Controller;

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
            Controller.Teleport();
        }
    }
}
