using Godot;
using System;
using System.Linq;

[Tool]
public partial class HoleEditor : Polygon2D
{
    [ExportGroup("Dynamic")]
    [Export] FloorEditor FloorEditor;
    [Export] Vector2[] PreviousPolygon;
    [Export] Transform2D PreviousTransform;
    [Export] CsgPolygon3D FloorCsgRoot;
    [Export] CsgPolygon3D FloorCutter;

    public override void _Draw()
    {
        base._Draw();
        
        if (!Polygon.SequenceEqual<Vector2>(PreviousPolygon) || Transform != PreviousTransform)
        {
            PreviousPolygon = Polygon;
            PreviousTransform = Transform;
            UpdateFloorCutter();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if (!IsPartOfEditedScene())
            return;

        FloorCutter?.Free();
        FloorCutter = null;
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        // Switching scenes removes the floor cutter so recreate it here
        if (!Engine.IsEditorHint() || !IsPartOfEditedScene())
            return;

        if (FloorCsgRoot != null)
            CreateFloorCutter();
    }

    public void Initialize(FloorEditor floorEditor, CsgPolygon3D csgRoot)
    {
        FloorEditor = floorEditor;
        FloorCsgRoot = csgRoot;
        PreviousPolygon = Polygon;
        PreviousTransform = Transform;
        CreateFloorCutter();
    }

    void CreateFloorCutter()
    {
        FloorCutter?.QueueFree();
        FloorCutter = new CsgPolygon3D();
        FloorCutter.Operation = CsgShape3D.OperationEnum.Subtraction;
        FloorCutter.Depth = FloorCsgRoot.Depth + 1;
        FloorCsgRoot.AddChild(FloorCutter);
        FloorCutter.Owner = Owner;

        UpdateFloorCutter();
    }

    void UpdateFloorCutter()
    {
        if (FloorCutter == null)
            return;

        FloorCutter.GlobalPosition = new Vector3(GlobalPosition.X / Globals.PixelsPerUnit, FloorCsgRoot.GlobalPosition.Y - 0.5f, GlobalPosition.Y / Globals.PixelsPerUnit);
        FloorCutter.Rotation = Vector3.Zero with { Z = Rotation }; // Remember floor cutter root is rotated already
        FloorCutter.Scale = new Vector3(Scale.X, Scale.Y, 1);

        Vector2[] newFloorCutterPolygon = new Vector2[Polygon.Length];
        for (int i=0; i<Polygon.Length; i++)
        {
            newFloorCutterPolygon[i] = Polygon[i] / Globals.PixelsPerUnit;
        }
        FloorCutter.Polygon = newFloorCutterPolygon;
    }
}
