using Godot;
using System;

public partial class PlayerVisionLight : OmniLight3D
{
    [Export] Node3D PlayerBody;
    [Export] float Height;
    [Export] int FloorOffset;

    public override void _Process(double delta)
    {
        base._Process(delta);

        int floor = Mathf.FloorToInt(PlayerBody.GlobalPosition.Y / Globals.FloorHeight) + FloorOffset;
        GlobalPosition = PlayerBody.GlobalPosition with { Y = floor * Globals.FloorHeight + Height };
    }
}
