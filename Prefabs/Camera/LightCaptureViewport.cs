using Godot;
using System;

public partial class LightCaptureViewport : ViewportMatchSize
{
    const string LIGHT_CAPTURE_PARAMETER = "light_capture";

    public override void _Ready()
    {
        base._Ready();

        RenderingServer.Singleton.GlobalShaderParameterSet(LIGHT_CAPTURE_PARAMETER, GetTexture());
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        RenderingServer.Singleton.GlobalShaderParameterSet(LIGHT_CAPTURE_PARAMETER, default);
    }
}
