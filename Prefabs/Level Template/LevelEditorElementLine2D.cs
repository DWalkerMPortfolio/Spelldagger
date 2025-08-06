using Godot;
using System;

[Tool]
public partial class LevelEditorElementLine2D : Line2D, ILevelEditorElement
{
    [ExportGroup("Internal")]
    [Export] protected Follow2DParent Root;
    [Export] bool DestroyInGame = true;
    [Export] LevelEditor.ElementTypes Type = LevelEditor.ElementTypes.Prop;

    [ExportGroup("Dynamic")]
    [Export] protected bool Initialized;

    public override void _Ready()
    {
        base._Ready();

        if (!Engine.IsEditorHint())
        {
            if (DestroyInGame)
                SetScript(default);
            return;
        }

#if TOOLS
        if (EditorInterface.Singleton.GetEditedSceneRoot() == this)
            EditorInterface.Singleton.EditNode(Root);
        else
        {
            GetParent()?.SetEditableInstance(this, true);
            SetDisplayFolded(true);
        }
#endif

        if (!Initialized)
        {
            Initialized = ((ILevelEditorElement)this).AddToLevel(Type);
            Root.Initialize();
        }
    }

    public void SkipInitialization()
    {
        Initialized = true;
    }

    public virtual void SetFloor(LevelEditorFloor floor)
    {
        Root.SetFloor(floor.Floor);
    }
}
