#if TOOLS
using Godot;
using System;

[Tool]
public partial class ExposedCanvasItemEditorSnap : EditorPlugin, ISerializationListener
{
	public static ExposedCanvasItemEditorSnap Instance { get; private set; }

    readonly Vector2I DEFAULT_GRID_SNAP = new Vector2I(64, 64);

    [Export] Button GridSnappingButton;
    [Export] SpinBox GridSnapBoxX;
    [Export] SpinBox GridSnapBoxY;

	public override void _EnterTree()
	{
        Instance = this;

        Control baseControl = EditorInterface.Singleton.GetBaseControl();
        Node canvasItemEditor = baseControl.FindChild("@CanvasItemEditor*", owned: false);
        GridSnappingButton = (Button)canvasItemEditor.GetChild(0).GetChild(0).GetChild(0).GetChild(12);
        Node snapDialog = canvasItemEditor.FindChild("@SnapDialog*", owned: false);
        GridSnapBoxX = (SpinBox)snapDialog.GetChild(3, true).GetChild(0).GetChild(4);
        GridSnapBoxY = (SpinBox)snapDialog.GetChild(3, true).GetChild(0).GetChild(5);
    }

	public override void _ExitTree()
	{
        Instance = null;
    }

    public void OnBeforeSerialize()
    {
        Instance = null;
    }

    public void OnAfterDeserialize()
    {
        Instance = this;
    }

    public bool IsSnappingEnabled()
    {
        return GridSnappingButton.ButtonPressed;
    }

    public Vector2I GetGridSize()
    {
        Vector2I gridSnap = new Vector2I((int)GridSnapBoxX.Value, (int)GridSnapBoxY.Value);
        if (gridSnap == Vector2I.One)
            gridSnap = DEFAULT_GRID_SNAP;
        return gridSnap;
    }

    public Vector2 SnapPositionToGrid(Vector2 position)
    {
        if (!IsSnappingEnabled())
            return position;

        Vector2I gridSize = GetGridSize();
        return (position / gridSize).Round() * gridSize;
    }
}
#endif
