using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class LevelEditor : Node2D, ISerializationListener
{
    #region Variables
    public enum ElementTypes { Floor, Wall, Guard, Prop, Pickup }
    public static LevelEditor SelectedLevelEditor { get; private set; }
    static HashSet<LevelEditor> ActiveLevelEditors = new HashSet<LevelEditor>();

    public static readonly string CREATE_WALL_ACTION = "Level Editor Create Wall";
    public static readonly string CREATE_FLOOR_ACTION = "Level Editor Create Floor";
    public static readonly string CREATE_GUARD_ACTION = "Level Editor Create Guard";
    public static readonly string CREATE_PICKUP_ACTION = "Level Editor Create Pickup";
    public static readonly string CREATE_ELEMENT_ACTION_PREFIX = "Level Editor Create ";
    
    const string GO_UP_FLOOR_ACTION = "Level Editor Go Up One Floor";
    const string GO_DOWN_FLOOR_ACTION = "Level Editor Go Down One Floor";
    const string BRING_UP_FLOOR_ACTION = "Level Editor Bring Elements Up One Floor";
    const string BRING_DOWN_FLOOR_ACTION = "Level Editor Bring Elements Down One Floor";
    const string CHANGE_ELEMENTS_FLOOR_ACTION = "Level Editor Change Elements Floor";

    static readonly Dictionary<ElementTypes, Key> ElementHotkeys = new Dictionary<ElementTypes, Key>() {
        { ElementTypes.Floor, Key.F},
        { ElementTypes.Wall, Key.W },
        { ElementTypes.Pickup, Key.I }
    };

#if TOOLS
    static Dictionary<string, ToolScriptHotkeys.HotkeyPressed> RegisteredHotkeys = new Dictionary<string, ToolScriptHotkeys.HotkeyPressed>();
#endif

    [Export] public Node2D LevelFloorRoot { get; private set; }
    
    [ExportGroup("Internal")]
    [Export] public Godot.Collections.Dictionary<ElementTypes, PackedScene> ElementPrefabs { get; private set; } = new Godot.Collections.Dictionary<ElementTypes, PackedScene>();
    [Export] ThreeDUnderlay ThreeDUnderlay;
    [Export] PackedScene LevelFloorPrefab;
    [Export] public NavigationRegion3D NavigationRegion { get; private set; }
    [Export(PropertyHint.Layers3DPhysics)] uint NavigationCollisionLayers;

    [ExportGroup("Dynamic")]
    [Export] Transform2D LastTransform;
    #endregion

    #region Godot Functions
    public override void _EnterTree()
    {
        base._EnterTree();

        if (Engine.IsEditorHint())
        {
#if TOOLS
            GetParent()?.SetEditableInstance(this, true);
            ActiveLevelEditors.Add(this);
            SetSelectedLevelEditor();
            CallDeferred(MethodName.RegisterHotkeys);
            EditorInterface.Singleton.GetSelection().SelectionChanged += OnSelectionChanged;
            NavigationRegion.NavigationMesh = new NavigationMesh();
            NavigationRegion.NavigationMesh.GeometryParsedGeometryType = NavigationMesh.ParsedGeometryType.StaticColliders;
            NavigationRegion.NavigationMesh.GeometryCollisionMask = NavigationCollisionLayers;
#endif
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if (Engine.IsEditorHint())
        {
#if TOOLS
            ActiveLevelEditors.Remove(this);
            ClearSelectedLevelEditor();
            EditorInterface.Singleton.GetSelection().SelectionChanged -= OnSelectionChanged;
#endif
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (LastTransform != GlobalTransform)
            UpdateTransform(GlobalTransform);
    }

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (!IsPartOfEditedScene())
            return;

        if (what == NotificationEditorPreSave)
        {
            NavigationRegion.CallDeferred(NavigationRegion3D.MethodName.BakeNavigationMesh, false);
        }
    }
    #endregion

    #region Interface Functions
    public void OnAfterDeserialize()
    {
#if TOOLS
        SetSelectedLevelEditor();
        ActiveLevelEditors.Add(this);

        CallDeferred(MethodName.RegisterHotkeys);
        EditorInterface.Singleton.GetSelection().SelectionChanged += OnSelectionChanged;
#endif
    }

    public void OnBeforeSerialize()
    {
#if TOOLS
        EditorInterface.Singleton.GetSelection().SelectionChanged -= OnSelectionChanged;
#endif
    }
    #endregion

    #region Public Functions
    public void AddElement(Node2D elementNode, ElementTypes type)
    {
        GetLevelFloor(ThreeDUnderlay.Instance.Floor).AddElement(elementNode, (int)type);
    }
    #endregion

    #region Static Functions
#if TOOLS
    static void RegisterHotkeys()
    {
        UnregisterHotkeys();

        // Create element hotkeys
        foreach (ElementTypes type in ElementHotkeys.Keys)
        {
            string action = CREATE_ELEMENT_ACTION_PREFIX + type.ToString();
            RegisterHotkey(action, ElementHotkeys[type], Key.Ctrl, () => { OnHotkeyCreateElement(type); });
        }

        RegisterHotkey(GO_UP_FLOOR_ACTION, Key.Bracketright, Key.None, OnHotkeyGoUpFloor);
        RegisterHotkey(GO_DOWN_FLOOR_ACTION, Key.Bracketleft, Key.None, OnHotKeyGoDownFloor);
        RegisterHotkey(BRING_UP_FLOOR_ACTION, Key.Bracketright, Key.Shift, OnHotkeyBringUpFloor);
        RegisterHotkey(BRING_DOWN_FLOOR_ACTION, Key.Bracketleft, Key.Shift, OnHotkeyBringDownFloor);
    }

    static void UnregisterHotkeys()
    {
        foreach (string action in RegisteredHotkeys.Keys)
        {
            ToolScriptHotkeys.Instance.UnregisterHotkey(action, RegisteredHotkeys[action]);
        }
        RegisteredHotkeys.Clear();
    }

    static void RegisterHotkey(string actionName, Key key, Key modifier, ToolScriptHotkeys.HotkeyPressed pressed)
    {
        InputEventKey inputEvent = new InputEventKey();
        inputEvent.Keycode = key;
        inputEvent.CtrlPressed = modifier == Key.Ctrl;
        inputEvent.AltPressed = modifier == Key.Alt;
        inputEvent.ShiftPressed = modifier == Key.Shift;
        ToolScriptHotkeys.Instance.RegisterHotkey(actionName, inputEvent, pressed);
        RegisteredHotkeys.Add(actionName, pressed);
    }

    static bool HotkeyValid()
    {
        string mainScreen = ToolScriptHotkeys.Instance.CurrentMainScreen;
        return mainScreen == "2D";
    }

    static bool SelectedLevelEditorValid()
    {
        return SelectedLevelEditor != null && SelectedLevelEditor.IsPartOfEditedScene();
    }
#endif
    #endregion

    #region Hotkeys
#if TOOLS
    static void OnHotkeyGoUpFloor()
    {
        if (HotkeyValid())
        {
            int currentFloor = ThreeDUnderlay.Instance.Floor;
            foreach (LevelEditor levelEditor in ActiveLevelEditors)
            {
                levelEditor.SwitchFloor(currentFloor + 1);
            }
        }
    }

    static void OnHotKeyGoDownFloor()
    {
        if (HotkeyValid())
        {
            int currentFloor = ThreeDUnderlay.Instance.Floor;
            foreach (LevelEditor levelEditor in ActiveLevelEditors)
            {
                levelEditor.SwitchFloor(currentFloor - 1);
            }
        }
    }

    static void OnHotkeyBringUpFloor()
    {
        if (HotkeyValid())
        {
            int currentFloor = ThreeDUnderlay.Instance.Floor;
            foreach (LevelEditor levelEditor in ActiveLevelEditors)
            {
                levelEditor.MoveSelectionToFloor(currentFloor + 1);
            }

            foreach (LevelEditor levelEditor in ActiveLevelEditors)
            {
                levelEditor.SwitchFloor(currentFloor + 1);
            }
        }
    }

    static void OnHotkeyBringDownFloor()
    {
        if (HotkeyValid())
        {
            int currentFloor = ThreeDUnderlay.Instance.Floor;
            foreach (LevelEditor levelEditor in ActiveLevelEditors)
            {
                levelEditor.MoveSelectionToFloor(currentFloor - 1);
            }

            foreach (LevelEditor levelEditor in ActiveLevelEditors)
            {
                levelEditor.SwitchFloor(currentFloor - 1);
            }
        }
    }

    static void OnHotkeyCreateElement(ElementTypes type)
    {
        if (HotkeyValid() && SelectedLevelEditorValid())
        {
            SelectedLevelEditor.GetLevelFloor(ThreeDUnderlay.Instance.Floor);
            SelectedLevelEditor.CreateElement((int)type);
        }
    }
#endif
    #endregion

    #region Public Functions
    public void UpdateTransform(Transform2D transform)
    {
        GlobalTransform = transform;

        LevelFloorRoot.GlobalTransform = transform;

        NavigationRegion.GlobalPosition = new Vector3(transform.Origin.X, 0, transform.Origin.Y) / Globals.PixelsPerUnit;
        NavigationRegion.GlobalRotation = new Vector3(0, -transform.Rotation, 0);

        LastTransform = transform;
    }
    #endregion

    #region Private Functions
    private void OnSelectionChanged()
    {
#if TOOLS
        SetSelectedLevelEditor();
#endif
    }

#if TOOLS
    void SetSelectedLevelEditor()
    {
        if (!IsPartOfEditedScene())
            return;

        if (SelectedLevelEditor == null)
        {
            SelectedLevelEditor = this;
            //GD.Print("Selected level editor: " + SelectedLevelEditor.Name);
            return;
        }
        
        Godot.Collections.Array<Node> selectedNodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        if (selectedNodes.Count == 0)
            return;

        if (selectedNodes[0] == this)
        {
            SelectedLevelEditor = this;
            //GD.Print("Selected level editor: " + SelectedLevelEditor.Name);
            return;
        }

        if (IsAncestorOf(selectedNodes[0]) && !SelectedLevelEditor.IsAncestorOf(selectedNodes[0]))
        {
            SelectedLevelEditor = this;
            //GD.Print("Selected level editor: " + SelectedLevelEditor.Name);
            return;
        }
    }

    void ClearSelectedLevelEditor()
    {
        if (SelectedLevelEditor == this)
        {
            //GD.Print("Clearing level editor instance");
            SelectedLevelEditor = null;
        }
    }
#endif

    LevelEditorFloor GetLevelFloor(int floor, bool create = true)
    {
        // Check if floor exists
        int targetIndex = -1;
        for (int i=0; i<LevelFloorRoot.GetChildCount(); i++)
        {
            LevelEditorFloor levelFloor = LevelFloorRoot.GetChild(i) as LevelEditorFloor;
            if (levelFloor == null)
                continue;

            if (levelFloor.Floor == floor)
                return levelFloor;
            else if (levelFloor.Floor > floor)
            {
                targetIndex = i;
                break;
            }
        }

        if (create)
        {
            // Make floor if it doesn't exist
            LevelEditorFloor createdFloor = (LevelEditorFloor)LevelFloorPrefab.Instantiate();
            LevelFloorRoot.AddChild(createdFloor);
            createdFloor.Owner = Owner;
            createdFloor.Name = "Floor" + floor;
            createdFloor.Initialize(this, floor);
            LevelFloorRoot.MoveChild(createdFloor, targetIndex);

            return createdFloor;
        }

        return null;
    }

    void SwitchFloor(int floor)
    {
        ThreeDUnderlay?.SetHeight((floor + 1) * Globals.FloorHeight, floor);

        foreach (LevelEditorFloor levelFloor in LevelFloorRoot.GetChildren())
        {
            if (levelFloor.Floor == floor)
            {
                levelFloor.SetMeta("_edit_lock_", default);
                levelFloor.SetMeta("_edit_group_", default);
                levelFloor.SetDisplayFolded(false);
            }
            else
            {
                levelFloor.SetMeta("_edit_lock_", true);
                levelFloor.SetMeta("_edit_group_", true);
                levelFloor.SetDisplayFolded(true);
            }
        }
    }

    void MoveSelectionToFloor(int floor)
    {
#if TOOLS
        Godot.Collections.Array<Node> selectedNodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();

        EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
        undoRedo.CreateAction(CHANGE_ELEMENTS_FLOOR_ACTION);
        undoRedo.AddDoMethod(this, MethodName.MoveNodesToFloor, new Variant[] { selectedNodes, floor });
        undoRedo.AddUndoMethod(this, MethodName.MoveNodesToFloor, new Variant[] { selectedNodes, ThreeDUnderlay.Instance.Floor });
        undoRedo.CommitAction(); // Automatically performs do method
#endif
    }

    void MoveNodesToFloor(Godot.Collections.Array<Node> nodes, int floor)
    {
#if TOOLS
        LevelEditorFloor currentFloor = GetLevelFloor(ThreeDUnderlay.Instance.Floor, false);
        LevelEditorFloor targetFloor = GetLevelFloor(floor);
        
        foreach (Node node in nodes)
        {
            if (!IsAncestorOf(node))
                continue;

            ILevelEditorElement levelElement = node as ILevelEditorElement;
            if (levelElement == null)
                continue;

            if (currentFloor != null && currentFloor.IsAncestorOf(node))
            {
                Node nodeParent = node.GetParent();
                nodeParent.RemoveChild(node);
                Node newParent = targetFloor.FindChild(nodeParent.Name, false);
                if (newParent == null)
                    newParent = targetFloor.OtherRoot;
                newParent.AddChild(node);
                node.Owner = Owner;
            }

            levelElement.SetFloor(targetFloor);
            EditorInterface.Singleton.GetSelection().AddNode((Node)levelElement);
        }
#endif
    }

    void CreateElement(int type, bool redo = false, Vector2 redoPosition = default)
    {
#if TOOLS
        // Get prefab
        ElementTypes typeEnum = (ElementTypes)type;
        if (!ElementPrefabs.ContainsKey(typeEnum))
            return;
        PackedScene prefab = ElementPrefabs[typeEnum];

        // Instantiate node
        Node2D instantiatedNode = (Node2D)prefab.Instantiate();
        
        if (redo)
        {
            ILevelEditorElement instantiatedElement = instantiatedNode as ILevelEditorElement;
            if (instantiatedElement != null)
                instantiatedElement.SkipInitialization();
        }
        
        AddChild(instantiatedNode, true);
        instantiatedNode.Owner = Owner;

        // Position node
        Vector2 instantiatedNodePosition = redoPosition;
        if (!redo)
        {
            Vector2 mousePosition = EditorInterface.Singleton.GetEditorViewport2D().GetMousePosition();
            instantiatedNodePosition = ExposedCanvasItemEditorSnap.Instance.SnapPositionToGrid(mousePosition);
        }
        instantiatedNode.GlobalPosition = instantiatedNodePosition;

        // Create undo/redo
        if (!redo)
        {
            Node2D undoInstantiatedNode = instantiatedNode; // Has to be a new reference variable (so reference doesn't get overwritten if function is called again?)
            EditorUndoRedoManager undoRedo = EditorInterface.Singleton.GetEditorUndoRedo();
            undoRedo.CreateAction("Create Node");
            undoRedo.AddDoMethod(this, MethodName.CreateElement, new Variant[] { type, true, instantiatedNodePosition});
            undoRedo.AddUndoMethod(this, MethodName.FreeElement, new Variant[] { undoInstantiatedNode });
            undoRedo.CommitAction(false);
        }
#endif
    }

    void FreeElement(Node2D node)
    {
        node?.QueueFree();
    }
#endregion
}

