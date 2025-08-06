using Godot;
using System;

[Tool]
public partial class Line : MeshInstance3D
{
    [Export] Node3D Origin;
    [Export] Node3D Target;

    [ExportToolButton("Update")]
    Callable UpdateCallable => Callable.From(Update);

    void Update()
    {
        if (Origin != null && Target != null)
        {
            LookAtFromPosition(Origin.GlobalPosition, Target.GlobalPosition, Vector3.Up);
            Scale = Scale with { Z = Origin.GlobalPosition.DistanceTo(Target.GlobalPosition) };
        }
    }
}
