using Godot;
using System;

public partial class DebugMover : Node
{
    [Export] NodePath[] ThreeDTargetPaths;
    [Export] NodePath[] TwoDTargetPaths;
    [Export] float Speed;

    Node3D[] threeDTargets;
    Node2D[] twoDTargets;

    public override void _Ready()
    {
        base._Ready();

        threeDTargets = Globals.ConvertNodePathArray<Node3D>(this, ThreeDTargetPaths);
        twoDTargets = Globals.ConvertNodePathArray<Node2D>(this, TwoDTargetPaths);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        foreach (Node3D node3D in threeDTargets)
            node3D.GlobalPosition += Vector3.Right * Speed * (float)delta;

        foreach (Node2D node2D in twoDTargets)
            node2D.GlobalPosition += Vector2.Right * Speed * Globals.PixelsPerUnit * (float)delta;
    }
}
