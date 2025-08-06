using Godot;
using System;

#if TOOLS
[Tool]
public partial class ExtractMeshes : EditorScenePostImport
{
	const string MESH_SUBFOLDER_SUFFIX = "Meshes";
	const string FILE_TYPE = "glb";

    public override GodotObject _PostImport(Node scene)
    {
        // Get paths
        string filepath = GetSourceFile();
        string directoryPath = filepath.GetBaseDir();
        string fileExtension = filepath.GetExtension();
        string filename = filepath.GetFile();

        // Skip if not a .glb file
        if (fileExtension != FILE_TYPE)
            return scene;

        // Check if a meshes directory exists, create it if not
        string meshDirectoryPath = directoryPath + "/" + filename + "-" + MESH_SUBFOLDER_SUFFIX;
        if (!DirAccess.DirExistsAbsolute(meshDirectoryPath))
            DirAccess.MakeDirAbsolute(meshDirectoryPath);

        // Extract meshes
        ExtractMeshesRecursively(scene, meshDirectoryPath);

        // Rescan filesystem
        EditorInterface.Singleton.GetResourceFilesystem().Scan();
        return scene;
    }

    void ExtractMeshesRecursively(Node node, string meshDirectory)
    {
        // Extract mesh
        if (node is MeshInstance3D)
        {
            string resourcePath = meshDirectory + "/" + node.Name + ".res";
            Mesh mesh = ((MeshInstance3D)node).Mesh;

            // Copy materials to new mesh
            if (ResourceLoader.Exists(resourcePath))
            {
                Mesh previousMesh = (Mesh)ResourceLoader.Load(resourcePath);
                for (int i=0; i<Mathf.Min(previousMesh.GetSurfaceCount(), mesh.GetSurfaceCount()); i++)
                {
                    mesh.SurfaceSetMaterial(i, previousMesh.SurfaceGetMaterial(i));
                }
            }
                
            ResourceSaver.Save(mesh, resourcePath);
            mesh.TakeOverPath(resourcePath);
        }

        // Recurse
        foreach (Node childNode in node.GetChildren())
        {
            ExtractMeshesRecursively(childNode, meshDirectory);
        }
    }
}
#endif
