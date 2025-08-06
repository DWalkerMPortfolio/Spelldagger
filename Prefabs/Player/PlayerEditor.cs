using Godot;
using System;

[Tool]
public partial class PlayerEditor : Node2D, ILevelEditorElement
{
    [Export] Follow2DParent Root;

    public override void _Ready()
    {
        base._Ready();

        if (Engine.IsEditorHint())
        {
            GetParent()?.SetEditableInstance(this, true);
            SetDisplayFolded(true);
        }
    }

    public void SetFloor(LevelEditorFloor floor)
    {
        Root.SetFloor(floor.Floor);
    }
}