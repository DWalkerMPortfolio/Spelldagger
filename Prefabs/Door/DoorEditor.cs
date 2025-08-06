using Godot;
using System;

[Tool]
public partial class DoorEditor : Node2D, ILevelEditorElement
{
    [Export] KeyItem[] Keys;
    [Export] bool SliceOccluder = true;
    [Export] bool Open
    {
        get { return _open; }
        set { _open = value; if (Door != null) { Door.openBackwards = OpenBackwards; Door.SetOpen(value); } }
    }
    bool _open;
    [Export] public bool OpenBackwards { get; private set; }
    [Export] Mesh Mesh
    {
        get { return _mesh; }
        set { _mesh = value; if (Door != null) Door.SetMesh(value); }
    }
    Mesh _mesh;

    [ExportGroup("Internal")]
    [Export] Door Door;
    [Export] Follow2DParent Root;
    [Export] CollisionShape3D WallCutterZone;

    [ExportGroup("Dynamic")]
    [Export] public int AttachedSegment { get; private set; }
    [Export] public float AttachedSegmentProportion { get; private set; } // Percentage of the way along the attached segment this door is
    [Export] Vector2 PreviousPosition = Vector2.Inf;
    [Export] WallEditor Wall;
    [Export] WallSlicer WallCutter;

    public override void _Ready()
    {
        base._Ready();

        if (!Engine.IsEditorHint())
        {
            Door.Keys = Keys;
            SetScript(default);
        }
        else
        {
            GetParent()?.SetEditableInstance(this, true);
            SetDisplayFolded(true);
        }
    }

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationEditorPreSave)
        {
            Door.Keys = Keys;
            Door.openBackwards = OpenBackwards;
        }
    }

    public void Initialize(WallEditor wall)
    {
        Wall = wall;
        CreateWallCutter();
        PreviousPosition = Vector2.Inf;
        SnapToWall();
    }

    public void SetFloor(LevelEditorFloor floor)
    {
        Root.SetFloor(floor.Floor);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!IsPartOfEditedScene())
            return;

        if (GlobalPosition != PreviousPosition)
        {
            if (Wall != null)
                SnapToWall();

            UpdateWallCutter();

            PreviousPosition = GlobalPosition;
        }
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();

        if (!IsPartOfEditedScene())
            return;

        WallCutter?.Free();
        WallCutter = null;
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        // Switching scenes removes the wall cutter so recreate it here
        if (!Engine.IsEditorHint() || !IsPartOfEditedScene())
            return;

        if (Wall != null)
            CreateWallCutter();
    }

    public void StayOnWall()
    {
        int segmentEndIndex = (AttachedSegment + 1) % Wall.Points.Length;
        Position = Wall.Points[AttachedSegment].Lerp(Wall.Points[segmentEndIndex], AttachedSegmentProportion);
        LookAt(GlobalPosition + Wall.Points[AttachedSegment].DirectionTo(Wall.Points[segmentEndIndex]).Rotated(Mathf.Pi / 2));
    }

    public void SnapToWall()
    {
        WallEditor.SnapResult snapResult = Wall.SnapPositionToWall(GlobalPosition);
        GlobalPosition = snapResult.Position;
        Rotation = snapResult.Angle;
        AttachedSegment = snapResult.Segment;
        AttachedSegmentProportion = snapResult.SegmentProportion;
    }

    void CreateWallCutter()
    {
        WallCutter?.QueueFree();
        WallCutter = new WallSlicer();
        WallCutter.Size = ((BoxShape3D)WallCutterZone.Shape).Size;
        WallCutter.SliceOccluder = SliceOccluder;
        Wall.AddSubtractiveShape(WallCutter);

        UpdateWallCutter();
    }

    async void UpdateWallCutter()
    {
        if (WallCutter == null)
            return;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // Give time for transforms to update (especially Follow2DParent)
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        WallCutter.GlobalPosition = WallCutterZone.GlobalPosition;
        WallCutter.GlobalRotation = WallCutterZone.GlobalRotation;
        WallCutter.Scale = WallCutterZone.Scale;
    }
}