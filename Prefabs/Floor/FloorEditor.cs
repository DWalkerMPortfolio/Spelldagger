using Godot;
using System;
using System.Linq;

[Tool]
public partial class FloorEditor : LevelEditorElementPolygon2D, ISerializationListener
{
	const string CREATE_HOLE_ACTION = "Floor Editor Create Hole";

	[Export] Material FloorMaterial;
	[Export] Vector2 UVScale = Vector2.One;

	[ExportGroup("Internal")]
	[Export] CsgPolygon3D CsgRoot;
	[Export] MeshInstance3D MeshInstance;
	[Export] MeshInstance3D LightCapture;
	[Export] MeshInstance3D GuardVisionDisplay;
	[Export] MeshInstance3D ShadowCaster;
	[Export] CollisionShape3D CollisionShape;
	[Export] PackedScene HolePrefab;
	[Export] Node2D HoleRoot;

	[ExportGroup("Dynamic")]
	[Export] Vector2[] PreviousPolygon;

    public override void _Ready()
    {
		if (Engine.IsEditorHint())
			PreviousPolygon = Polygon;
        
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

    public override void _Draw()
    {
        base._Draw();

		if (!Engine.IsEditorHint())
			return;

		if (!Polygon.SequenceEqual<Vector2>(PreviousPolygon))
		{
			BuildMesh();
			PreviousPolygon = Polygon;
		}
    }

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (!IsPartOfEditedScene())
            return;

		if (what == NotificationEditorPreSave)
			SaveMesh();
    }

    public void OnAfterDeserialize()
    {
#if TOOLS
		CallDeferred(MethodName.RegisterHotkeys);
#endif
	}

	public void OnBeforeSerialize() { }

    public override void SetFloor(LevelEditorFloor floor)
	{
		base.SetFloor(floor);

        foreach (Node node in HoleRoot.GetChildren())
		{
			node.Owner = Owner;
		}
	}

	public void BuildMesh()
	{
		Vector2[] newCsgPolygon = new Vector2[Polygon.Length];
		for (int i=0; i<Polygon.Length; i++)
		{
			newCsgPolygon[i] = Polygon[i] / Globals.PixelsPerUnit;
		}
		CsgRoot.Polygon = newCsgPolygon;

		CsgRoot.Material = FloorMaterial;
    }

	void SaveMesh()
	{
        Godot.Collections.Array csgGeneratedMesh = CsgRoot.GetMeshes();
		if (csgGeneratedMesh.Count > 0)
		{
			MeshInstance.Mesh = (Mesh)csgGeneratedMesh[1];

			MeshInstance.MaterialOverride = FloorMaterial;

			CollisionShape.Shape = MeshInstance.Mesh.CreateTrimeshShape();
		}
		else
		{
			MeshInstance.Mesh = new PlaceholderMesh();
			CollisionShape.Shape = null;
		}

        GuardVisionDisplay.Mesh = MeshInstance.Mesh;
        LightCapture.Mesh = MeshInstance.Mesh;
		ShadowCaster.Mesh = MeshInstance.Mesh;
    }

#if TOOLS
	void RegisterHotkeys()
	{
		InputEventKey createHoleEvent = new InputEventKey();
		createHoleEvent.Keycode = Key.H;
		createHoleEvent.CtrlPressed = true;
		ToolScriptHotkeys.Instance.RegisterHotkey(CREATE_HOLE_ACTION, createHoleEvent, OnCreateHoleHotkeyPressed);
	}

	void UnregisterHotkeys()
	{
		ToolScriptHotkeys.Instance.UnregisterHotkey(CREATE_HOLE_ACTION, OnCreateHoleHotkeyPressed);
	}

	void OnCreateHoleHotkeyPressed()
	{
		Godot.Collections.Array<Node> selectedNodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
		if (ToolScriptHotkeys.Instance.CurrentMainScreen == "2D" && IsPartOfEditedScene() && selectedNodes.Count == 1 && (selectedNodes[0] == this || IsAncestorOf(selectedNodes[0])))
		{
			Vector2 mousePosition = EditorInterface.Singleton.GetEditorViewport2D().GetMousePosition();
			Vector2 mousePositionSnapped = ExposedCanvasItemEditorSnap.Instance.SnapPositionToGrid(mousePosition);
            HoleEditor hole = CreateHole(ToLocal(mousePositionSnapped));

            EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
			undoRedo.CreateAction(CREATE_HOLE_ACTION);
			undoRedo.AddDoMethod(this, MethodName.CreateHole, ToLocal(mousePosition));
			undoRedo.AddUndoMethod(hole, Node.MethodName.Free);
			undoRedo.CommitAction(false);
		}
	}
#endif

	HoleEditor CreateHole(Vector2 position)
	{
		HoleEditor hole = (HoleEditor)HolePrefab.Instantiate();
		HoleRoot.AddChild(hole, true);
		hole.Owner = Owner;
		hole.Position = position;
		hole.Initialize(this, CsgRoot);

#if TOOLS
		EditorInterface.Singleton.EditNode(hole);
#endif

		return hole;
	}

	Vector2[] MergePolygonHole(Vector2[] outerPolygon, Vector2[] holePolygon)
	{
		float closestDistanceSquared = Mathf.Inf;
		int closestOuterIndex = 0;
		int closestHoleIndex = 0;
		for (int i=0; i<outerPolygon.Length; i++)
		{
			for (int j=0; j<holePolygon.Length; j++)
			{
				float distanceSquared = outerPolygon[i].DistanceSquaredTo(holePolygon[j]);
                if (distanceSquared < closestDistanceSquared)
                {
					closestDistanceSquared = distanceSquared;
					closestOuterIndex = i;
					closestHoleIndex = j;
                }
            }
		}

		Vector2[] result = new Vector2[outerPolygon.Length + holePolygon.Length + 2];
		int resultIndex = 0;
		// Follow outer polygon to the closest index
		for (int i=0; i<=closestOuterIndex; i++, resultIndex++)
		{
			result[resultIndex] = outerPolygon[i];
		}
		// Follow inner polygon from the closest index (includes closest index again at the end) (don't need to reverse direction because hole is already stored counterclockwise)
		for (int i=0; i<=holePolygon.Length; i++, resultIndex++)
		{
			int holeIndex = (closestHoleIndex + i) % holePolygon.Length;
			result[resultIndex] = holePolygon[holeIndex];
		}
		// Follow outer polygon forward from closest index (including closest index again)
		for (int i=closestOuterIndex; i<outerPolygon.Length; i++, resultIndex++)
		{
			result[resultIndex] = outerPolygon[i];
		}

		return result;
	}
}
