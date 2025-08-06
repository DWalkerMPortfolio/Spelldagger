#if TOOLS
using Godot;
using System;

public partial class GenerateMeshProps : EditorContextMenuPlugin
{
    const string GENERATE_MESH_PROPS = "Generate Mesh Prop(s)";
    const string GENERATED_DIRECTORY_NAME = "Generated";

    public SpelldaggerUtilityReferences references;

    string[] paths;

    public override void _PopupMenu(string[] paths)
    {
        base._PopupMenu(paths);

        AddContextMenuItem(GENERATE_MESH_PROPS, Callable.From((string[] paths) => { Popup(paths); }));
    }

    void Popup(string[] paths)
    {
        this.paths = paths;

        GenerateMeshPropsPopup popup = (GenerateMeshPropsPopup)references.GenerateMeshPropsPopup.Instantiate();
        EditorInterface.Singleton.PopupDialogCentered(popup);
        popup.Initialize(this);
    }

    public void Generate(bool snapToFloor, bool snapToWall, bool addCollision, bool lightCapture)
    {
        EditorInterface.Singleton.OpenSceneFromPath(references.PropBase.ResourcePath, true);
        PropEditor propBase = (PropEditor)EditorInterface.Singleton.GetEditedSceneRoot();

        string generatedDirectoryPath = paths[0].GetBaseDir().GetBaseDir() + "/" + GENERATED_DIRECTORY_NAME;
        if (!DirAccess.DirExistsAbsolute(generatedDirectoryPath))
            DirAccess.MakeDirAbsolute(generatedDirectoryPath);

        foreach (string path in paths)
        {
            Mesh mesh = GD.Load<Resource>(path) as Mesh;
            if (mesh == null)
                continue; // Tried to generate a prop from a non-mesh resource, skipping

            string name = path.GetFile().Split('.')[0];
            propBase.Generate(name, mesh, snapToFloor, snapToWall, addCollision, lightCapture);
            EditorInterface.Singleton.SaveSceneAs(generatedDirectoryPath + "/" + name + ".tscn");
        }

        EditorInterface.Singleton.GetResourceFilesystem().Scan();
    }
}
#endif