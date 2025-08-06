#if TOOLS
using Godot;
using System;

[Tool]
public partial class MeshTools : EditorPlugin
{
	UpdateCollisionShape updateCollisionShape;

	public override void _EnterTree()
	{
		updateCollisionShape = new UpdateCollisionShape();
		AddContextMenuPlugin(EditorContextMenuPlugin.ContextMenuSlot.SceneTree, updateCollisionShape);
	}

	public override void _ExitTree()
	{
		RemoveContextMenuPlugin(updateCollisionShape);
	}
}
#endif
