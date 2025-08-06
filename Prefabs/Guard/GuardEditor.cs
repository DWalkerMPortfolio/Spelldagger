using Godot;
using System;

[Tool]
public partial class GuardEditor : LevelEditorElementLine2D
{
    [Export] public GuardStateBehavior IdleBehavior { get; private set; }
    [Export] public GuardStateBehavior InvestigatingBehavior { get; private set; }
    [Export] public GuardStateBehavior AlertedBehavior { get; private set; }
    [Export] public GuardStateBehavior DamagedBehavior { get; private set; }

    [ExportGroup("Dynamic")]
    [Export] public float Height { get; private set; }
    [Export] public LevelEditorFloor Floor { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        if (!Engine.IsEditorHint())
        {
            // Duplicate behavior scripts so they aren't shared with other guards
            IdleBehavior = (GuardStateBehavior)IdleBehavior?.Duplicate();
            InvestigatingBehavior = (GuardStateBehavior)InvestigatingBehavior?.Duplicate();
            AlertedBehavior = (GuardStateBehavior)AlertedBehavior?.Duplicate();
            DamagedBehavior = (GuardStateBehavior)DamagedBehavior?.Duplicate();
        }
    }

    public override void SetFloor(LevelEditorFloor floor)
    {
        base.SetFloor(floor);

        Height = floor.Floor * Globals.FloorHeight;
        Floor = floor;
    }
}
