using Godot;
using System;

[Tool]
public partial class HideInGame : Node2D
{
	[Export] bool HideInEditor;

	public override void _Ready()
	{
		Visible = Engine.IsEditorHint() != HideInEditor;
	}
}
