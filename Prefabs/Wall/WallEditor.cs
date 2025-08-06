using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class WallEditor : LevelEditorElementLine2D, ISerializationListener
{
    #region Variables
    public struct SnapResult
    {
        public bool Valid;
        public Vector2 Position;
        public float Angle;
        public int Segment;
        public float SegmentProportion;
    }

    const string CREATE_DOOR_ACTION = "Wall Editor Create Door";
    const string SPLIT_WALL_ACTION = "Wall Editor Split Wall";
    const string FLIP_WALL_ACTION = "Wall Editor Flip Wall";

    // If changing these exposed properties, remember to add them to CopyProperties()
    [Export] ArrayMesh WallSegmentMesh;
    [Export] PhysicsMaterial PhysicsMaterial;
    [Export] bool OccludeVision = true;

	[ExportGroup("Internal")]
	[Export] StaticBody3D StaticBody;
	[Export] MeshInstance3D MeshInstance;
	[Export] MeshInstance3D GuardVisionDisplay;
	[Export] MeshInstance3D LightCapture;
	[Export] CollisionShape3D CollisionShape;
	[Export] MeshInstance3D VisionOccluder;
    [Export] MeshInstance3D PlayerVisionDisplay;
	[Export] CollisionShape3D OccluderCollisionShape;
    [Export] CsgCombiner3D CsgTree;
	[Export] CsgCombiner3D AdditiveCsgTree;
    [Export] CsgCombiner3D SubtractiveCsgTree;
    [Export] PackedScene DoorPrefab;
    [Export] Node2D DoorRoot;
    [Export] float BoundsPadding = 0.01f;
	[Export] Vector2 OccluderHeightRange;

    [ExportGroup("Dynamic")]
    [Export] LevelEditorFloor Floor;
	[Export] Vector2[] PreviousPoints;
    [Export] Godot.Collections.Array<Vector2> RedoSplitWallPositions = new Godot.Collections.Array<Vector2>();
    #endregion

    #region Godot Functions
    public override void _Ready()
	{
        if (Engine.IsEditorHint())
        {
            PreviousPoints = Points;
            Root.SetDisplayFolded(true);
        }

        base._Ready();
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        if (Engine.IsEditorHint())
        {
#if TOOLS
            CallDeferred(MethodName.RegisterHotkeys);
#endif
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

#if TOOLS
        UnregisterHotkeys();
#endif
    }

    // Gets called whenever the wall's points are updated
    public override void _Draw()
    {
#if TOOLS
        if (!PreviousPoints.SequenceEqual<Vector2>(Points))
		{
            if (PreviousPoints.Length == Points.Length)
            {
                foreach (DoorEditor door in DoorRoot.GetChildren())
                    door.StayOnWall();
            }
            else
            {
                foreach (DoorEditor door in DoorRoot.GetChildren())
                    door.SnapToWall();
            }

			PreviousPoints = Points;
			Generate();
		}
#endif
    }

    public override void _Notification(int what)
    {
        base._Notification(what);

#if TOOLS
        if (!IsPartOfEditedScene())
            return;

        if (what == NotificationEditorPreSave)
        {
			Generate();
			SaveMesh();

            StaticBody.PhysicsMaterialOverride = PhysicsMaterial;
        }
#endif
    }
    #endregion

    #region Interface Functions
    public override void SetFloor(LevelEditorFloor floor)
    {
        base.SetFloor(floor);
        
        Floor = floor;

        foreach (DoorEditor door in DoorRoot.GetChildren())
        {
            door.SetFloor(floor);
            door.Owner = Owner;
        }
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
#if TOOLS
        CallDeferred(MethodName.RegisterHotkeys);
#endif
    }
    #endregion

    #region Hotkey Functions
#if TOOLS
    void RegisterHotkeys()
    {
        // Create door
        InputEventKey createDoorEvent = new InputEventKey();
        createDoorEvent.Keycode = Key.R;
        createDoorEvent.CtrlPressed = true;
        ToolScriptHotkeys.Instance.RegisterHotkey(CREATE_DOOR_ACTION, createDoorEvent, OnHotkeyCreateDoor);

        // Split wall
        InputEventKey splitWallEvent = new InputEventKey();
        splitWallEvent.Keycode = Key.X;
        ToolScriptHotkeys.Instance.RegisterHotkey(SPLIT_WALL_ACTION, splitWallEvent, OnHotkeySplitWall);

        // Flip wall
        InputEventKey flipWallEvent = new InputEventKey();
        flipWallEvent.Keycode = Key.C;
        ToolScriptHotkeys.Instance.RegisterHotkey(FLIP_WALL_ACTION, flipWallEvent, OnHotkeyFlipWall);
    }

    void UnregisterHotkeys()
    {
        ToolScriptHotkeys.Instance.UnregisterHotkey(CREATE_DOOR_ACTION, OnHotkeyCreateDoor);
        ToolScriptHotkeys.Instance.UnregisterHotkey(SPLIT_WALL_ACTION, OnHotkeySplitWall);
    }

    void OnHotkeyCreateDoor()
    {
        if (!HotkeyValid())
            return;

        Vector2 doorPosition = EditorInterface.Singleton.GetEditorViewport2D().GetMousePosition();
        doorPosition = ExposedCanvasItemEditorSnap.Instance.SnapPositionToGrid(doorPosition);
        DoorEditor instantiatedDoor = CreateDoor(doorPosition);

        EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
        undoRedo.CreateAction(CREATE_DOOR_ACTION);
        undoRedo.AddDoMethod(this, MethodName.CreateDoor, doorPosition);
        undoRedo.AddUndoMethod(this, MethodName.FreeNode, instantiatedDoor);
        undoRedo.CommitAction(false);
    }

    void OnHotkeySplitWall()
    {
        if (!HotkeyValid())
            return;

        Vector2 splitPosition = EditorInterface.Singleton.GetEditorViewport2D().GetMousePosition();
        splitPosition = ExposedCanvasItemEditorSnap.Instance.SnapPositionToGrid(splitPosition);
        WallEditor splitWall = SplitWall(splitPosition);

        EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
        undoRedo.CreateAction(SPLIT_WALL_ACTION);
        undoRedo.AddDoMethod(this, MethodName.RedoSplitWall);
        undoRedo.AddUndoMethod(splitWall, MethodName.UndoSplitWall, this);
        undoRedo.AddUndoMethod(this, MethodName.UndidSplitWall, splitPosition);
        undoRedo.CommitAction(false);
    }

    void OnHotkeyFlipWall()
    {
        if (!HotkeyValid())
            return;

        EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
        undoRedo.CreateAction(FLIP_WALL_ACTION);
        undoRedo.AddDoMethod(this, MethodName.FlipWall);
        undoRedo.AddUndoMethod(this, MethodName.FlipWall);
        undoRedo.CommitAction(true);
    }

    bool HotkeyValid()
    {
        string mainScreen = ToolScriptHotkeys.Instance.CurrentMainScreen;
        Godot.Collections.Array<Node> selectedNodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        return IsPartOfEditedScene() && mainScreen == "2D" && selectedNodes.Count == 1 && (selectedNodes[0] == this || IsAncestorOf(selectedNodes[0]));
    }
#endif
#endregion

    #region Public Functions
    public void AddSubtractiveShape(WallSlicer subtractionShape)
    {
#if TOOLS
        subtractionShape.GetParent()?.RemoveChild(subtractionShape);
        SubtractiveCsgTree.AddChild(subtractionShape);
        subtractionShape.Owner = EditorInterface.Singleton.GetEditedSceneRoot();
#endif
    }

    /// <summary>
    /// Returns the closest position on this wall, with a rotation to face 90 degrees away from the wall towards the original position
    /// </summary>
    /// <param name="position">The global position to snap</param>
    /// <returns>A struct containing the result of the snap</returns>
    public SnapResult SnapPositionToWall(Vector2 position)
    {
        Vector2 localPosition = ToLocal(position);
        float closestDistanceSquared = Mathf.Inf;
        Vector2 closestPosition = localPosition;
        int closestIndex = 0;
        int closestIndexEnd = 1;

        int endIndex = Points.Length - 1;
        if (Closed)
            endIndex = Points.Length;

        for (int i = 0; i < endIndex; i++)
        {
            int segmentStartIndex = i;
            int segmentEndIndex = (i + 1) % Points.Length;

            Vector2 potentialSnapPosition = Geometry2D.GetClosestPointToSegment(localPosition, Points[segmentStartIndex], Points[segmentEndIndex]);
            float potentialSnapDistanceSquared = localPosition.DistanceSquaredTo(potentialSnapPosition);
            if (potentialSnapDistanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = potentialSnapDistanceSquared;
                closestPosition = potentialSnapPosition;
                closestIndex = segmentStartIndex;
                closestIndexEnd = segmentEndIndex;
            }
        }

        SnapResult result = new SnapResult();
        result.Valid = true;
        result.Position = ToGlobal(closestPosition);
        Vector2 normalDirection = ToGlobal(Points[closestIndex]).DirectionTo(ToGlobal(Points[closestIndexEnd])).Rotated(Mathf.Pi / 2);
        result.Angle = normalDirection.Angle();
        if (Mathf.Sign(normalDirection.Dot(position - result.Position)) < 0)
            result.Angle += Mathf.Pi;
        result.Segment = closestIndex;
        result.SegmentProportion = ToGlobal(Points[closestIndex]).DistanceTo(ToGlobal(closestPosition)) / ToGlobal(Points[closestIndex]).DistanceTo(ToGlobal(Points[closestIndexEnd]));
        return result;
    }
    #endregion

    #region Private Functions
    DoorEditor CreateDoor(Vector2 position)
    {
        DoorEditor door = (DoorEditor)DoorPrefab.Instantiate();
        DoorRoot.AddChild(door, true);
        door.GlobalPosition = position;
        door.Owner = Owner;
        door.Initialize(this);
        door.SetFloor(Floor);
#if TOOLS
        EditorInterface.Singleton.EditNode(door);
#endif
        return door;
    }

    void FreeNode(Node node)
    {
        node.QueueFree();
    }

    WallEditor SplitWall(Vector2 position)
    {
        Vector2 localPosition = ToLocal(position);
        float closestDistanceSquared = Mathf.Inf;
        Vector2 splitPosition = position;
        int splitIndex = 0;
        for (int i = 0; i < Points.Length - 1; i++)
        {
            Vector2 potentialSplitPosition = Geometry2D.GetClosestPointToSegment(localPosition, Points[i], Points[i + 1]);
            float potentialSplitDistanceSquared = localPosition.DistanceSquaredTo(potentialSplitPosition);
            if (potentialSplitDistanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = potentialSplitDistanceSquared;
                splitPosition = potentialSplitPosition;
                splitIndex = i;
            }
        }
        Vector2 closestPositionGlobal = ToGlobal(splitPosition);
        float splitProportion = Points[splitIndex].DistanceTo(splitPosition) / Points[splitIndex].DistanceTo(Points[splitIndex + 1]);


        // Create new wall
        WallEditor splitWall = ResourceLoader.Load<PackedScene>(SceneFilePath).Instantiate<WallEditor>();
        ILevelEditorElement splitWallElement = (ILevelEditorElement)splitWall;
        splitWallElement.SkipInitialization();
        GetParent().AddChild(splitWall, true);
        splitWall.Owner = Owner;

#if TOOLS
        splitWall.GlobalPosition = ExposedCanvasItemEditorSnap.Instance.SnapPositionToGrid(closestPositionGlobal);
#endif

        Vector2 splitWallPointsOffset = GlobalPosition - splitWall.GlobalPosition;
        int splitWallPointsLength = Points.Length - splitIndex;
        if (Closed)
            splitWallPointsLength += 1;
        Vector2[] splitWallPoints = new Vector2[splitWallPointsLength];
        splitWallPoints[0] = splitPosition + splitWallPointsOffset;
        for (int i=1; i<Points.Length - splitIndex; i++)
        {
           splitWallPoints[i] = Points[splitIndex + i] + splitWallPointsOffset;
        }
        if (Closed)
            splitWallPoints[splitWallPointsLength - 1] = Points[0] + splitWallPointsOffset;

        splitWall.CopyProperties(splitWallPoints, WallSegmentMesh, PhysicsMaterial, OccludeVision);

        // Copy over doors
        foreach (DoorEditor door in DoorRoot.GetChildren())
        {
            if (door.AttachedSegment > splitIndex || (door.AttachedSegment == splitIndex && door.AttachedSegmentProportion > splitProportion))
                splitWall.TransferDoor(door);
            else
                door.SnapToWall();
        }

        // Remove split points
        Vector2[] newPoints = Points[0..(splitIndex + 2)];
        newPoints[splitIndex + 1] = splitPosition;
        SetPoints(newPoints);

        Closed = false;
        splitWall.Closed = false;

        return splitWall;
    }

    void UndoSplitWall(WallEditor originalWall)
    {
        Vector2 pointsOffset = GlobalPosition - originalWall.GlobalPosition;
        originalWall.RemovePoint(originalWall.Points.Length - 1); // Remove the point created by the split
        for (int i = 1; i<Points.Length; i++)
        {
            originalWall.AddPoint(Points[i] + pointsOffset);
        }

        foreach (DoorEditor door in DoorRoot.GetChildren())
        {
            originalWall.TransferDoor(door);
        }

        QueueFree();
    }

    void UndidSplitWall(Vector2 position)
    {
        RedoSplitWallPositions.Add(position);
    }

    void RedoSplitWall()
    {
        SplitWall(RedoSplitWallPositions[RedoSplitWallPositions.Count - 1]);
        RedoSplitWallPositions.RemoveAt(RedoSplitWallPositions.Count - 1);
    }

    void FlipWall()
    {
        Points = Points.Reverse<Vector2>().ToArray<Vector2>();

        foreach (DoorEditor door in DoorRoot.GetChildren())
        {
            door.SnapToWall();
        }
    }

    void CopyProperties(Vector2[] points, ArrayMesh wallSegmentMesh, PhysicsMaterial physicsMaterial, bool occludeVision)
    {
        SetPoints(points);
        WallSegmentMesh = wallSegmentMesh;
        PhysicsMaterial = physicsMaterial;
        OccludeVision = occludeVision;
    }

    void TransferDoor(DoorEditor door)
    {
        //GD.Print("Transfering door to: " + Name);
        Vector2 doorGlobalPosition = door.GlobalPosition;
        door.GetParent()?.RemoveChild(door);
        DoorRoot.AddChild(door);
        door.GlobalPosition = doorGlobalPosition;
        door.Owner = Owner;
        door.Initialize(this);
    }

#if TOOLS
    void Generate()
	{
        // Clear existing additive CSG meshes
        foreach (Node3D child in AdditiveCsgTree.GetChildren())
        {
            AdditiveCsgTree.RemoveChild(child);
            child.Free();
        }

        if (WallSegmentMesh == null || Points.Length == 0 || !IsPartOfEditedScene())
            return;

        if (EditorInterface.Singleton.GetEditedSceneRoot() != this) // Don't build on save in wall prefab scene
        {
            BuildMesh();
            BuildOccluder();
        }
    }

    void BuildMesh()
    {
        Aabb wallMeshAabb = WallSegmentMesh.GetAabb();

        // Iterate over each wall segment
        int endIndex = Points.Length - 1;
        if (Closed)
            endIndex = Points.Length;

        for (int i = 0; i < endIndex; i++)
        {
            int previousSegmentStartIndex = (i - 1) % Points.Length;
            if (previousSegmentStartIndex < 0)
                previousSegmentStartIndex += Points.Length;
            int segmentStartIndex = i;
            int segmentEndIndex = (i + 1) % Points.Length;
            int nextSegmentEndIndex = (i + 2) % Points.Length;

            Vector3 point1 = new Vector3(Points[segmentStartIndex].X / Globals.PixelsPerUnit, 0, Points[segmentStartIndex].Y / Globals.PixelsPerUnit); // Local space 3D coordinates
            Vector3 point2 = new Vector3(Points[segmentEndIndex].X / Globals.PixelsPerUnit, 0, Points[segmentEndIndex].Y / Globals.PixelsPerUnit);
            float segmentLength = point1.DistanceTo(point2);
            if (segmentLength == 0)
                continue;
            Vector3 segmentDirection = point1.DirectionTo(point2);

            CsgCombiner3D segmentRoot = new CsgCombiner3D();
            AdditiveCsgTree.AddChild(segmentRoot);
            segmentRoot.Owner = EditorInterface.Singleton.GetEditedSceneRoot();

            // Generate additive CSG meshes along the segment
            for (float x = 0; x < segmentLength + wallMeshAabb.Size.Z / 2; x += wallMeshAabb.Size.Z)
            {
                CsgMesh3D mesh = new CsgMesh3D();
                segmentRoot.AddChild(mesh);
                mesh.Owner = EditorInterface.Singleton.GetEditedSceneRoot();
                
                mesh.Mesh = WallSegmentMesh;
                mesh.Position = point1 + segmentDirection * x;
                mesh.Rotation = mesh.Rotation with { Y = Vector3.Forward.SignedAngleTo(segmentDirection, Vector3.Up) };
            }

            // Generate segment bounds
            CsgPolygon3D segmentBounds = new CsgPolygon3D();
            segmentRoot.AddChild(segmentBounds);
            segmentRoot.MoveChild(segmentBounds, -1);
            segmentBounds.Owner = EditorInterface.Singleton.GetEditedSceneRoot();

            segmentBounds.Operation = CsgShape3D.OperationEnum.Intersection;
            segmentBounds.Position -= Vector3.Up * BoundsPadding;
            segmentBounds.Depth = wallMeshAabb.End.Y + BoundsPadding * 2;
            segmentBounds.RotationDegrees = new Vector3(90, 0, 0);
            segmentBounds.Material = WallSegmentMesh.SurfaceGetMaterial(0);

            Vector2[] segmentBoundsPolygon = new Vector2[4];

            Vector2 currentSegmentTangent = Points[segmentStartIndex].DirectionTo(Points[segmentEndIndex]);
            Vector2 currentSegmentNormal = currentSegmentTangent.Rotated(Mathf.Pi / 2);
            Vector2 currentSegmentSide1From = Points[segmentStartIndex] / Globals.PixelsPerUnit + currentSegmentNormal * (wallMeshAabb.Position.X - BoundsPadding);
            Vector2 currentSegmentSide2From = Points[segmentStartIndex] / Globals.PixelsPerUnit + currentSegmentNormal * (wallMeshAabb.End.X + BoundsPadding);
            if (segmentStartIndex==0 && !Closed)
            {
                segmentBoundsPolygon[0] = currentSegmentSide1From;
                segmentBoundsPolygon[3] = currentSegmentSide2From;
            }
            else
            {
                Vector2 previousSegmentTangent = Points[previousSegmentStartIndex].DirectionTo(Points[segmentStartIndex]);
                Vector2 previousSegmentNormal = previousSegmentTangent.Rotated(Mathf.Pi / 2);
                Vector2 previousSegmentSide1From = Points[previousSegmentStartIndex] / Globals.PixelsPerUnit + previousSegmentNormal * (wallMeshAabb.Position.X - BoundsPadding);
                Vector2 previousSegmentSide2From = Points[previousSegmentStartIndex] / Globals.PixelsPerUnit + previousSegmentNormal * (wallMeshAabb.End.X + BoundsPadding);

                Variant side1Intersection = Geometry2D.LineIntersectsLine(currentSegmentSide1From, currentSegmentTangent, previousSegmentSide1From, previousSegmentTangent);
                if (side1Intersection.VariantType == Variant.Type.Vector2)
                    segmentBoundsPolygon[0] = (Vector2)side1Intersection;
                else
                    segmentBoundsPolygon[0] = currentSegmentSide1From;

                Variant side2Intersection = Geometry2D.LineIntersectsLine(currentSegmentSide2From, currentSegmentTangent, previousSegmentSide2From, previousSegmentTangent);
                if (side2Intersection.VariantType == Variant.Type.Vector2)
                    segmentBoundsPolygon[3] = (Vector2)side2Intersection;
                else
                    segmentBoundsPolygon[3] = currentSegmentSide2From;
            }

            if (segmentStartIndex == Points.Length - 2 && !Closed)
            {
                segmentBoundsPolygon[1] = Points[segmentEndIndex] / Globals.PixelsPerUnit + currentSegmentNormal * (wallMeshAabb.Position.X - BoundsPadding);
                segmentBoundsPolygon[2] = Points[segmentEndIndex] / Globals.PixelsPerUnit + currentSegmentNormal * (wallMeshAabb.End.X + BoundsPadding);
            }
            else
            {
                Vector2 nextSegmentTangent = Points[segmentEndIndex].DirectionTo(Points[nextSegmentEndIndex]);
                Vector2 nextSegmentNormal = nextSegmentTangent.Rotated(Mathf.Pi / 2);
                Vector2 nextSegmentSide1From = Points[segmentEndIndex] / Globals.PixelsPerUnit + nextSegmentNormal * (wallMeshAabb.Position.X - BoundsPadding);
                Vector2 nextSegmentSide2From = Points[segmentEndIndex] / Globals.PixelsPerUnit + nextSegmentNormal * (wallMeshAabb.End.X + BoundsPadding);

                Variant side1Intersection = Geometry2D.LineIntersectsLine(currentSegmentSide1From, currentSegmentTangent, nextSegmentSide1From, nextSegmentTangent);
                if (side1Intersection.VariantType == Variant.Type.Vector2)
                    segmentBoundsPolygon[1] = (Vector2)side1Intersection;
                else
                    segmentBoundsPolygon[1] = nextSegmentSide1From;

                Variant side2Intersection = Geometry2D.LineIntersectsLine(currentSegmentSide2From, currentSegmentTangent, nextSegmentSide2From, nextSegmentTangent);
                if (side2Intersection.VariantType == Variant.Type.Vector2)
                    segmentBoundsPolygon[2] = (Vector2)side2Intersection;
                else
                    segmentBoundsPolygon[2] = nextSegmentSide2From;
            }

            segmentBounds.Polygon = segmentBoundsPolygon;
        }
    }

	void BuildOccluder()
	{
        // Skip if this wall doesn't occlude vision
        if (!OccludeVision)
        {
            VisionOccluder.Mesh = new PlaceholderMesh();
            PlayerVisionDisplay.Mesh = new PlaceholderMesh();
            OccluderCollisionShape.Shape = null;
            return;
        }

        // Build occluder
        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool.SetSmoothGroup(UInt32.MaxValue);

        int endIndex = Points.Length - 1;
        if (Closed)
            endIndex = Points.Length;

        for (int i = 0; i < endIndex; i++)
        {
            int segmentStartIndex = i;
            int segmentEndIndex = (i + 1) % Points.Length;

            // Get all subtractive geometry on this segment
            List<Vector2> segmentSlices = new List<Vector2>(); // X = start of slice, Y = end of slice
            foreach (Node subtractiveChild in SubtractiveCsgTree.GetChildren())
            {
                WallSlicer wallSlicer = subtractiveChild as WallSlicer;
                if (wallSlicer == null || !wallSlicer.SliceOccluder)
                    continue;
                Vector2 wallSlicerPosition2D = new Vector2(wallSlicer.Position.X, wallSlicer.Position.Z) * Globals.PixelsPerUnit;
                if (Mathf.IsZeroApprox(wallSlicerPosition2D.DistanceSquaredTo(Geometry2D.GetClosestPointToSegment(wallSlicerPosition2D, Points[segmentStartIndex], Points[segmentEndIndex])))) // Check if wall slicer is on this segment
                {
                    float wallSlicerDistance = Points[segmentStartIndex].DistanceTo(wallSlicerPosition2D);
                    segmentSlices.Add(new Vector2(wallSlicerDistance - wallSlicer.Size.Z * Globals.PixelsPerUnit / 2, wallSlicerDistance + wallSlicer.Size.Z * Globals.PixelsPerUnit / 2));
                }
            }
            segmentSlices.OrderBy(v => v.X);

            // Generate all pieces of this segment
            if (segmentSlices.Count == 0)
            {
                AddOccluderSegment(surfaceTool, Points[segmentStartIndex], Points[segmentEndIndex]);
            }
            else
            {
                Vector2 segmentDirection = Points[segmentStartIndex].DirectionTo(Points[segmentEndIndex]);
                float segmentLength = Points[segmentStartIndex].DistanceTo(Points[segmentEndIndex]);
                if (segmentSlices[0].X > 0)
                    AddOccluderSegment(surfaceTool, Points[segmentStartIndex], Points[segmentStartIndex] + segmentDirection * segmentSlices[0].X); // Piece up to the start of the first slice
                float currentSliceEndDistanceAlongSegment = segmentSlices[0].Y;
                for (int j=1; j<segmentSlices.Count; j++)
                {
                    Vector2 segmentSlice = segmentSlices[j];
                    if (currentSliceEndDistanceAlongSegment >= segmentSlice.X) // Check if this segment slice starts before the previous one ended
                    {
                        currentSliceEndDistanceAlongSegment = Mathf.Max(currentSliceEndDistanceAlongSegment, segmentSlice.Y);
                        continue;
                    }
                    AddOccluderSegment(surfaceTool, Points[segmentStartIndex] + segmentDirection * currentSliceEndDistanceAlongSegment, Points[segmentStartIndex] + segmentDirection * segmentSlice.X);
                    currentSliceEndDistanceAlongSegment = segmentSlice.Y;
                }
                if (currentSliceEndDistanceAlongSegment < segmentLength)
                    AddOccluderSegment(surfaceTool, Points[segmentStartIndex] + segmentDirection * currentSliceEndDistanceAlongSegment, Points[segmentEndIndex]);
            }
        }
        surfaceTool.Index();
        surfaceTool.GenerateNormals();
        VisionOccluder.Mesh = surfaceTool.Commit();
        PlayerVisionDisplay.Mesh = VisionOccluder.Mesh;
        ConcavePolygonShape3D occluderCollisionShapeShape = VisionOccluder.Mesh.CreateTrimeshShape();
        occluderCollisionShapeShape.BackfaceCollision = true;
        OccluderCollisionShape.Shape = occluderCollisionShapeShape;
    }

    void AddOccluderSegment(SurfaceTool surfaceTool, Vector2 startPosition, Vector2 endPosition)
    {
        Vector3 point1 = new Vector3(startPosition.X / Globals.PixelsPerUnit, OccluderHeightRange.X, startPosition.Y / Globals.PixelsPerUnit);
        Vector3 point2 = new Vector3(endPosition.X / Globals.PixelsPerUnit, OccluderHeightRange.X, endPosition.Y / Globals.PixelsPerUnit);
        Vector3 point3 = point2 with { Y = OccluderHeightRange.Y };
        Vector3 point4 = point1 with { Y = OccluderHeightRange.Y };
        AddQuad(surfaceTool, point1, point2, point3, point4);
    }

    void AddQuad(SurfaceTool surfaceTool, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
    {
        surfaceTool.SetUV(new Vector2(0, 1));
        surfaceTool.AddVertex(point1);
        surfaceTool.SetUV(new Vector2(0, 1));
        surfaceTool.AddVertex(point2);
        surfaceTool.SetUV(Vector2.Zero);
        surfaceTool.AddVertex(point3);

        surfaceTool.SetUV(new Vector2(0, 1));
        surfaceTool.AddVertex(point1);
        surfaceTool.SetUV(Vector2.Zero);
        surfaceTool.AddVertex(point3);
        surfaceTool.SetUV(Vector2.Zero);
        surfaceTool.AddVertex(point4);
    }

    void SaveMesh()
	{
		Godot.Collections.Array csgGeneratedMesh = CsgTree.GetMeshes();
		if (csgGeneratedMesh.Count > 0)
        {
			MeshInstance.Mesh = (Mesh)csgGeneratedMesh[1];
            CollisionShape.Shape = MeshInstance.Mesh.CreateTrimeshShape();
        }
        else
        {
            MeshInstance.Mesh = new PlaceholderMesh();
            CollisionShape.Shape = null;
        }

        GuardVisionDisplay.Mesh = MeshInstance.Mesh;
        LightCapture.Mesh = MeshInstance.Mesh;
    }
#endif
#endregion
}