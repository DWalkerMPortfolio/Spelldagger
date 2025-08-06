using Godot;
using System;

public interface ILevelEditorElement
{
    public void SetFloor(LevelEditorFloor floor);

    public void SkipInitialization() { }

    public bool AddToLevel(LevelEditor.ElementTypes type)
    {
        Node2D selfNode = this as Node2D;
        if (selfNode == null)
            return false;

        if (LevelEditor.SelectedLevelEditor == null)
            return false;

        if (selfNode.GetParent() is Control)
            return false;

        LevelEditor.SelectedLevelEditor.AddElement(selfNode, type);
        return true;
    }

}