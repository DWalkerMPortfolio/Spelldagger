using Godot;
using System;

[Tool]
public partial class LevelEditorFloor : Node2D
{
    #region Variables
    [ExportGroup("Internal")]
    [Export] Godot.Collections.Dictionary<LevelEditor.ElementTypes, Node2D> ElementRoots = new Godot.Collections.Dictionary<LevelEditor.ElementTypes, Node2D>();
    [Export] public Node2D OtherRoot { get; private set; }

    [ExportGroup("Dynamic")]
    [Export] public LevelEditor LevelEditor { get; private set; }
    [Export] public int Floor { get; private set; }
    [Export] float Height;
    #endregion

    #region Godot Functions
    public override void _Ready()
    {
        base._Ready();

        if (Engine.IsEditorHint())
            GetParent()?.SetEditableInstance(this, true);
        else
            SetScript(default);
    }
    #endregion

    #region Public Functions
    public void Initialize(LevelEditor levelEditor, int floor)
    {
        LevelEditor = levelEditor;
        Floor = floor;
        Height = floor * Globals.FloorHeight;
    }

    public void AddElement(Node2D elementNode, int type, bool redo = false)
    {
        LevelEditor.ElementTypes typeEnum = (LevelEditor.ElementTypes)type;

        ILevelEditorElement element = elementNode as ILevelEditorElement;
        if (element == null)
            return;

        Node elementParent = elementNode.GetParent();
        if (elementParent != null)
            elementParent.RemoveChild(elementNode);

        if (ElementRoots.ContainsKey(typeEnum))
            ElementRoots[typeEnum].AddChild(elementNode, true);
        else
            OtherRoot.AddChild(elementNode, true);
        elementNode.Owner = Owner;
        element.SetFloor(this);

        if (!redo)
        {
#if TOOLS
            EditorInterface.Singleton.EditNode(elementNode);
            CallDeferred(MethodName.CreateAddElementUndoRedo, new Variant[] { elementNode, type, elementParent });
#endif
        }
    }

    public WallEditor.SnapResult SnapPositionToWall(Vector2 position, float maxDistance = Mathf.Inf)
    {
        float maxDistanceSquared = maxDistance * maxDistance;
        WallEditor.SnapResult result = default;
        float closestDistanceSquared = Mathf.Inf;
        
        Node2D wallRoot = ElementRoots[LevelEditor.ElementTypes.Wall];
        if (wallRoot == null)
            return result;

        foreach (WallEditor wall in wallRoot.GetChildren())
        {
            WallEditor.SnapResult testResult = wall.SnapPositionToWall(position);
            float testDistanceSquared = position.DistanceSquaredTo(testResult.Position);
            if (testDistanceSquared < closestDistanceSquared && testDistanceSquared < maxDistanceSquared)
            {
                closestDistanceSquared = testDistanceSquared;
                result = testResult;
            }
        }

        return result;
    }
    #endregion

    #region Private Functions
#if TOOLS
    void CreateAddElementUndoRedo(Node2D elementNode, int type, Node elementOriginalParent)
    {
        EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();

        undoRedo.CreateAction("Create Node", mergeMode: UndoRedo.MergeMode.All);
        undoRedo.AddDoMethod(this, MethodName.RedoAddElement, new Variant[] { elementNode, type });
        undoRedo.CommitAction(false);

        undoRedo.CreateAction("Add Prop", customContext: this); // Needs to be a separate action to allow parents to update between steps
        undoRedo.AddUndoMethod(this, MethodName.UndoAddElement, new Variant[] { elementNode, elementOriginalParent });
        undoRedo.CommitAction(false);
    }

    void UndoAddElement(Node2D elementNode, Node elementOriginalParent)
    {
        elementNode.GetParent()?.RemoveChild(elementNode);
        elementOriginalParent.AddChild(elementNode);

        EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
        undoRedo.GetHistoryUndoRedo(undoRedo.GetObjectHistoryId(this)).CallDeferred(UndoRedo.MethodName.Undo); // After the element's parent has updated, undo again to actually delete it
    }

    void RedoAddElement(Node2D elementNode, int type)
    {
        CallDeferred(MethodName.AddElement, new Variant[] { elementNode, type, true });

        EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
        undoRedo.GetHistoryUndoRedo(undoRedo.GetObjectHistoryId(this)).Redo(); // Redo again to get history back in sync
    }
#endif
    #endregion
}