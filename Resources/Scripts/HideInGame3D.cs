using Godot;
using System;

[Tool]
public partial class HideInGame3D : Node3D
{
	[Export] bool HideInEditor;

	public override void _Ready()
	{
		Visible = Engine.IsEditorHint() != HideInEditor;
	}
}
