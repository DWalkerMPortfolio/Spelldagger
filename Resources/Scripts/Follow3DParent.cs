using Godot;
using System;

[Tool]
public partial class Follow3DParent : Node2D
{
    [Export] Node3D ParentOverride;
    [Export] Vector2 Offset;
    [Export] bool FollowRotation = true;
    [Export] bool FollowScale;

    Node3D threeDParent;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Find3DParent();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Stop processing if not in edited scene
        if (!IsPartOfEditedScene())
            return;

        // Stop processing if no 2D parent is found
        if (threeDParent == null || !threeDParent.IsInsideTree())
            return;

        // Position
        GlobalPosition = new Vector2(threeDParent.GlobalPosition.X, threeDParent.GlobalPosition.Z) * Globals.PixelsPerUnit + Offset;

        // Rotation
        if (FollowRotation)
            GlobalRotation = -threeDParent.GlobalRotation.Y;

        // Scale
        if (FollowScale)
            Scale = new Vector2(threeDParent.Scale.X, threeDParent.Scale.Z);
    }

    void Find3DParent()
    {
        if (ParentOverride == null)
        {
            Node Parent = GetParent();
            if (Parent != null && Parent is Node3D)
            {
                threeDParent = (Node3D)Parent;
            }
            else
            {
                GD.PushWarning("No 3D parent found for node: " + Name);
            }
        }
        else
            threeDParent = ParentOverride;
    }
}
