using Godot;
using System;

public partial class LightCapture : MeshInstance3D
{
    [Export] MeshInstance3D SourceMeshInstance;

    public override void _Ready()
    {
        base._Ready();

        Mesh = SourceMeshInstance.Mesh;
        SetProcess(false);
    }
}
