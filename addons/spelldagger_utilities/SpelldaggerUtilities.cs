#if TOOLS
using Godot;
using System;

[Tool]
public partial class SpelldaggerUtilities : EditorPlugin
{
	SpelldaggerUtilityReferences references;
	GenerateMeshProps generateMeshProps;

	public override void _EnterTree()
	{
		// Initialize references scene
		references = GD.Load<PackedScene>("addons/spelldagger_utilities/References.tscn").Instantiate<SpelldaggerUtilityReferences>();

		// Add generate static body props option
		generateMeshProps = new GenerateMeshProps();
		generateMeshProps.references = references;
		AddContextMenuPlugin(EditorContextMenuPlugin.ContextMenuSlot.Filesystem, generateMeshProps);
	}

	public override void _ExitTree()
	{
		RemoveContextMenuPlugin(generateMeshProps);
	}
}
#endif
