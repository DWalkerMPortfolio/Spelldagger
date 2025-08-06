#if TOOLS
using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class PointsEditorHotkeys : EditorPlugin, ISerializationListener
{
    const string BOX_SELECT_POINTS_ACTION = "Box Select Edit Polygon";

    enum BoxSelectStates { NoSelectBox, CreatingSelectBox, SelectBoxCreated, DraggingSelection}

    [Export] Godot.Collections.Array<BoxSelectButton> BoxSelectButtons;
    [Export] ColorRect SelectBox;
    [Export] Button SelectModeButton;

    Shortcut createPointsShortcut;
    Shortcut editPointsShortcut;
    Shortcut deletePointsShortcut;
    Shortcut bezierCurveShortcut;
    Vector2 boxSelectStartPosition;
    Vector2 selectBoxMaxPosition;
    Vector2 selectBoxMinPosition;
    Vector2 previousDragPosition;
    Polygon2D selectedPolygon;
    Line2D selectedLine;
    Path2D selectedPath;
    BoxSelectStates boxSelectState = BoxSelectStates.NoSelectBox; 
    List<int> selectedPoints = new List<int>();

    public override void _EnterTree()
    {
        base._EnterTree();
        
        BoxSelectButtons = new Godot.Collections.Array<BoxSelectButton>();

        createPointsShortcut = GD.Load<Shortcut>("addons/points_editor_hotkeys/CreatePointsShortcut.tres");
        editPointsShortcut = GD.Load<Shortcut>("addons/points_editor_hotkeys/EditPointsShortcut.tres");
        deletePointsShortcut = GD.Load<Shortcut>("addons/points_editor_hotkeys/DeletePointsShortcut.tres");
        bezierCurveShortcut = GD.Load<Shortcut>("addons/points_editor_hotkeys/BezierCurveShortcut.tres");
        SelectBox = GD.Load<PackedScene>("addons/points_editor_hotkeys/SelectBox.tscn").Instantiate<ColorRect>();

        Control baseControl = EditorInterface.Singleton.GetBaseControl();
        ConfigureButtons(baseControl.FindChild("@Polygon2DEditor*", owned: false));
        ConfigureButtons(baseControl.FindChild("@Line2DEditor*", owned: false));
        ConfigureCurveButtons(baseControl.FindChild("@Path2DEditor*", owned: false).GetChild(0));

        SelectModeButton = (Button)baseControl.FindChild("@CanvasItemEditor*", owned: false).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        SelectModeButton.Toggled += OnSelectModeButtonToggled;

        EditorInterface.Singleton.GetSelection().SelectionChanged += OnSelectionChanged;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        EditorInterface.Singleton.GetSelection().SelectionChanged -= OnSelectionChanged;
        SelectBox?.QueueFree();
        foreach (Button boxSelectButton in BoxSelectButtons)
        {
            boxSelectButton.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        SubViewport editorViewport2D = EditorInterface.Singleton.GetEditorViewport2D();
        Vector2 mousePosition = editorViewport2D.GetMousePosition();

        if (boxSelectState == BoxSelectStates.CreatingSelectBox)
        {
            selectBoxMinPosition = new Vector2(Mathf.Min(boxSelectStartPosition.X, mousePosition.X), Mathf.Min(boxSelectStartPosition.Y, mousePosition.Y));
            selectBoxMaxPosition = new Vector2(Mathf.Max(boxSelectStartPosition.X, mousePosition.X), Mathf.Max(boxSelectStartPosition.Y, mousePosition.Y));
        }

        if (boxSelectState == BoxSelectStates.DraggingSelection)
        {
            Vector2 mousePositionSnapped = ExposedCanvasItemEditorSnap.Instance.SnapPositionToGrid(mousePosition);
            Vector2 previousDragPositionSnapped = ExposedCanvasItemEditorSnap.Instance.SnapPositionToGrid(previousDragPosition);
            Vector2 dragOffset = mousePositionSnapped - previousDragPositionSnapped;
            previousDragPosition = mousePositionSnapped;

            EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
            undoRedo.CreateAction(BOX_SELECT_POINTS_ACTION, UndoRedo.MergeMode.Ends);

            if (selectedLine != null)
            {
                undoRedo.AddUndoProperty(selectedLine, Line2D.PropertyName.Points, selectedLine.Points);
                selectedLine.Points = OffsetSelectedPoints(selectedLine.Points, dragOffset);
                undoRedo.AddDoProperty(selectedLine, Line2D.PropertyName.Points, selectedLine.Points);
            }
            else if (selectedPolygon != null)
            {
                undoRedo.AddUndoProperty(selectedPolygon, Polygon2D.PropertyName.Polygon, selectedPolygon.Polygon);
                selectedPolygon.Polygon = OffsetSelectedPoints(selectedPolygon.Polygon, dragOffset);
                undoRedo.AddDoProperty(selectedPolygon, Polygon2D.PropertyName.Polygon, selectedPolygon.Polygon);
            }
            else if (selectedPath != null)
            {
                undoRedo.AddUndoProperty(selectedPath, Path2D.PropertyName.Curve, selectedPath.Curve);
                OffsetSelectedCurvePoints(selectedPath.Curve, dragOffset);
                undoRedo.AddDoProperty(selectedPath, Path2D.PropertyName.Curve, selectedPath.Curve);
            }

            undoRedo.AddUndoProperty(this, PropertyName.selectBoxMinPosition, selectBoxMinPosition);
            undoRedo.AddUndoProperty(this, PropertyName.selectBoxMaxPosition, selectBoxMaxPosition);
            selectBoxMinPosition += dragOffset;
            selectBoxMaxPosition += dragOffset;
            undoRedo.AddDoProperty(this, PropertyName.selectBoxMinPosition, selectBoxMinPosition);
            undoRedo.AddDoProperty(this, PropertyName.selectBoxMaxPosition, selectBoxMaxPosition);

            undoRedo.CommitAction(false);
        }

        if (boxSelectState != BoxSelectStates.NoSelectBox)
        {
            Vector2 viewportScale = editorViewport2D.GlobalCanvasTransform.Scale;
            SelectBox.Scale = new Vector2(1 / viewportScale.X, 1 / viewportScale.Y);
            SelectBox.GlobalPosition = selectBoxMinPosition;
            SelectBox.Size = (selectBoxMaxPosition - selectBoxMinPosition) / SelectBox.Scale;
        }
    }

    public override bool _Handles(GodotObject @object)
    {
        return @object is Node2D; // The closest common ancestor of Line2D and Polygon2D
    }

    public override bool _ForwardCanvasGuiInput(InputEvent @event)
    {
        InputEventMouseButton mouseButtonEvent = @event as InputEventMouseButton;
        if (mouseButtonEvent == null)
            return false;

        Godot.Collections.Array<Node> selectedNodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        if (selectedNodes.Count != 1 || !(selectedNodes[0] is Line2D || selectedNodes[0] is Polygon2D || selectedNodes[0] is Path2D))
            return false;

        if (!SelectModeButton.ButtonPressed)
            return false;
        
        bool boxSelectButtonSelected = false;
        foreach (BoxSelectButton boxSelectButton in BoxSelectButtons)
        {
            if (boxSelectButton.ButtonPressed && boxSelectButton.IsVisibleInTree())
            {
                boxSelectButtonSelected = true;
                break;
            }
        }
        if (!boxSelectButtonSelected)
            return false;

        if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
        {
            if (mouseButtonEvent.IsPressed())
            {
                Vector2 mousePosition = EditorInterface.Singleton.GetEditorViewport2D().GetMousePosition();
                if (boxSelectState == BoxSelectStates.SelectBoxCreated && IsPointInSelectBox(mousePosition))
                    StartDraggingSelectBox(mousePosition);
                else
                    StartCreatingSelectBox();
                return true;
            }
            else if (mouseButtonEvent.IsReleased())
            {
                if (boxSelectState == BoxSelectStates.DraggingSelection)
                    boxSelectState = BoxSelectStates.SelectBoxCreated;
            }
        }
        
        if (mouseButtonEvent.IsPressed() && mouseButtonEvent.ButtonIndex == MouseButton.Right)
        {
            if (boxSelectState != BoxSelectStates.NoSelectBox)
            {
                ClearSelectBox();
                return true;
            }
        }

        return false;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        InputEventMouseButton mouseButtonEvent = @event as InputEventMouseButton;
        if (mouseButtonEvent == null)
            return;

        if (boxSelectState == BoxSelectStates.CreatingSelectBox)
        {
            if (mouseButtonEvent.IsReleased() && mouseButtonEvent.ButtonIndex == MouseButton.Left)
                FinishCreatingSelectBox();
        }
    }

    public void OnBeforeSerialize()
    {
        ClearSelectBox();
    }

    public void OnAfterDeserialize() { }

    void OnSelectionChanged()
    {
        ClearSelectBox();
    }

    private void OnBoxSelectButtonToggled(bool toggledOn)
    {
        if (!toggledOn)
            ClearSelectBox();
    }

    private void OnSelectModeButtonToggled(bool toggledOn)
    {
        if (!toggledOn)
            ClearSelectBox();
    }

    void ConfigureButtons(Node parent)
    {
        ButtonGroup buttonGroup = new ButtonGroup();

        Button createPointsButton = (Button)parent.GetChild(0);
        createPointsButton.Shortcut = createPointsShortcut;
        createPointsButton.ButtonGroup = buttonGroup;

        Button editPointsButton = (Button)parent.GetChild(1);
        editPointsButton.Shortcut = editPointsShortcut;
        editPointsButton.ButtonGroup = buttonGroup;

        Button deletePointsButton = (Button)parent.GetChild(2);
        deletePointsButton.Shortcut = deletePointsShortcut;
        deletePointsButton.ButtonGroup = buttonGroup;

        BoxSelectButton boxSelectButton = GD.Load<PackedScene>("addons/points_editor_hotkeys/BoxSelectButton.tscn").Instantiate<BoxSelectButton>();
        boxSelectButton.ButtonGroup = buttonGroup;
        BoxSelectButtons.Add(boxSelectButton);
        parent.AddChild(boxSelectButton);
        boxSelectButton.Toggled += OnBoxSelectButtonToggled;
    }

    void ConfigureCurveButtons(Node parent)
    {
        ButtonGroup buttonGroup = new ButtonGroup();

        Button editPointsButton = (Button)parent.GetChild(0);
        editPointsButton.Shortcut = editPointsShortcut;
        editPointsButton.ButtonGroup = buttonGroup;

        Button bezierCurveButton = (Button)parent.GetChild(1);
        bezierCurveButton.Shortcut = bezierCurveShortcut;
        bezierCurveButton.ButtonGroup = buttonGroup;

        Button createPointsButton = (Button)parent.GetChild(2);
        createPointsButton.Shortcut = createPointsShortcut;
        createPointsButton.ButtonGroup = buttonGroup;

        Button deletePointsButton = (Button)parent.GetChild(3);
        deletePointsButton.Shortcut = deletePointsShortcut;
        deletePointsButton.ButtonGroup = buttonGroup;

        BoxSelectButton boxSelectButton = GD.Load<PackedScene>("addons/points_editor_hotkeys/BoxSelectButton.tscn").Instantiate<BoxSelectButton>();
        boxSelectButton.ButtonGroup = buttonGroup;
        BoxSelectButtons.Add(boxSelectButton);
        parent.AddChild(boxSelectButton);
        parent.MoveChild(boxSelectButton, 4);
        boxSelectButton.Toggled += OnBoxSelectButtonToggled;
    }

    void StartCreatingSelectBox()
    {
        Godot.Collections.Array<Node> selectedNodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        selectedPolygon = selectedNodes[0] as Polygon2D;
        selectedLine = selectedNodes[0] as Line2D;
        selectedPath = selectedNodes[0] as Path2D;

        SubViewport editorViewport2D = EditorInterface.Singleton.GetEditorViewport2D();
        SelectBox.GetParent()?.RemoveChild(SelectBox);
        editorViewport2D.AddChild(SelectBox);
        boxSelectStartPosition = editorViewport2D.GetMousePosition();
        SelectBox.GlobalPosition = boxSelectStartPosition;
        SelectBox.Size = Vector2.Zero;

        boxSelectState = BoxSelectStates.CreatingSelectBox;
    }

    void FinishCreatingSelectBox()
    {
        if (selectedPolygon != null)
            GetSelectedPoints(selectedPolygon, selectedPolygon.Polygon);
        else if (selectedLine != null)
            GetSelectedPoints(selectedLine, selectedLine.Points);
        else if (selectedPath != null)
            GetSelectedPoints(selectedPath, GetCurvePoints(selectedPath.Curve));

        boxSelectState = BoxSelectStates.SelectBoxCreated;
    }

    void StartDraggingSelectBox(Vector2 startPosition)
    {
        previousDragPosition = startPosition;
        boxSelectState = BoxSelectStates.DraggingSelection;
    }

    void ClearSelectBox()
    {
        SelectBox.GetParent()?.RemoveChild(SelectBox);
        boxSelectState = BoxSelectStates.NoSelectBox;
    }

    void GetSelectedPoints(Node2D node, Vector2[] points)
    {
        selectedPoints.Clear();
        for (int i=0; i<points.Length; i++)
        {
            Vector2 pointGlobal = node.ToGlobal(points[i]);
            if (IsPointInSelectBox(pointGlobal))
            {
                selectedPoints.Add(i);
            }
        }
    }

    Vector2[] GetCurvePoints(Curve2D curve)
    {
        Vector2[] result = new Vector2[curve.PointCount];
        for (int i = 0; i < curve.PointCount; i++)
            result[i] = curve.GetPointPosition(i);
        return result;
    }

    void OffsetSelectedCurvePoints(Curve2D curve, Vector2 offset)
    {
        Vector2[] curvePoints = GetCurvePoints(curve);
        Vector2[] offsetCurvePoints = OffsetSelectedPoints(curvePoints, offset);
        for (int i = 0; i < curve.PointCount; i++)
            curve.SetPointPosition(i, offsetCurvePoints[i]);
    }

    Vector2[] OffsetSelectedPoints(Vector2[] points, Vector2 offset)
    {
        Vector2[] result = new Vector2[points.Length];
        int selectedPointsIndex = 0;
        for (int i=0; i<points.Length; i++)
        {
            if (selectedPointsIndex < selectedPoints.Count && selectedPoints[selectedPointsIndex] == i)
            {
                result[i] = points[i] + offset;
                
                selectedPointsIndex++;
            }
            else
                result[i] = points[i];
        }
        return result;
    }

    bool IsPointInSelectBox(Vector2 point)
    {
        return point.X > selectBoxMinPosition.X && point.X < selectBoxMaxPosition.X && point.Y > selectBoxMinPosition.Y && point.Y < selectBoxMaxPosition.Y;
    }
}
#endif
