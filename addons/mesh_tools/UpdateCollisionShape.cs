#if TOOLS
using Godot;
using System;

[Tool]
public partial class UpdateCollisionShape : EditorContextMenuPlugin
{
    const string UPDATE_COLLISION_SHAPE = "Update Sibling Collision Shape";

    MeshInstance3D targetMeshInstance;

    public override void _PopupMenu(string[] paths)
    {
        base._PopupMenu(paths);

        if (paths.Length == 1)
        {
            targetMeshInstance = EditorInterface.Singleton.GetEditedSceneRoot().GetNode(paths[0]) as MeshInstance3D;
            if (targetMeshInstance != null)
            {
                AddContextMenuItem(UPDATE_COLLISION_SHAPE, Callable.From((string[] paths) => { UpdateShape(paths); }));
            }
        }
    }

    void UpdateShape(string[] paths)
    {
        bool updatedShape = false;
        foreach (Node siblingNode in targetMeshInstance.GetParent().GetChildren())
        {
            CollisionShape3D collisionShape = siblingNode as CollisionShape3D;

            if (collisionShape == null)
            {
                // Check children of any sibling static bodies as well
                StaticBody3D staticBody = siblingNode as StaticBody3D;
                if (staticBody != null)
                {
                    foreach (Node staticBodyChild in staticBody.GetChildren())
                    {
                        collisionShape = staticBodyChild as CollisionShape3D;
                        if (collisionShape != null)
                            break;
                    }
                }
            }

            if (collisionShape != null)
            {
                collisionShape.Shape = targetMeshInstance.Mesh.CreateConvexShape();
                updatedShape = true;
                break;
            }
        }

        if (updatedShape)
            GD.Print("Updated collision shape");
        else
            GD.PushError("No collision shape found");
    }
}
#endif