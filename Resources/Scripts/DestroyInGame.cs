using Godot;
using System;

public partial class DestroyInGame : Node
{
    public override void _EnterTree()
    {
        if (!Engine.IsEditorHint())
            QueueFree();
    }
}
