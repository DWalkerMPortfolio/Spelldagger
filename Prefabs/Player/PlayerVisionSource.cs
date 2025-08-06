using Godot;
using System;

public partial class PlayerVisionSource : Node3D
{
    const string PLAYER_VISION_POSITION_PARAMETER = "player_vision_position";

    public override void _Process(double delta)
    {
        base._Process(delta);

        RenderingServer.GlobalShaderParameterSet(PLAYER_VISION_POSITION_PARAMETER, GlobalPosition);
    }
}
