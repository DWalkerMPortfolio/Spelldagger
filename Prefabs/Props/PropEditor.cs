using Godot;
using System;

[Tool]
public partial class PropEditor : LevelEditorElementNode2D, ILevelEditorElement
{
    [Export] protected bool SnapToGround 
    { 
        get { if (Root != null) return Root.SnapToGround; else return false; }
        set { if (Root != null) Root.SnapToGround = value; }
    }
    [Export] protected bool SnapToWall;
    [Export] float SnapToWallMaxDistance = 3;

    [ExportGroup("Internal")]
    [Export] PackedScene LightCaptureMeshInstance;
    [Export] PackedScene ProximityFadeMeshInstance;

    [ExportGroup("Dynamic")]
    [Export] Vector2 PreviousPosition;
    [Export] LevelEditorFloor Floor;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!IsPartOfEditedScene() || !Initialized)
            return;

        if (SnapToWall && GlobalPosition != PreviousPosition)
        {
            WallEditor.SnapResult snapResult = Floor.SnapPositionToWall(GlobalPosition, SnapToWallMaxDistance * Globals.PixelsPerUnit);
            if (snapResult.Valid)
            {
                GlobalPosition = snapResult.Position;
                Rotation = snapResult.Angle;
            }

            PreviousPosition = GlobalPosition;
        }
    }

    public override void SetFloor(LevelEditorFloor floor)
    {
        base.SetFloor(floor);

        Root?.SetFloor(floor.Floor);
    }

    public void Generate(string name, Mesh mesh, bool snapToGround, bool snapToWall, bool addCollision, bool lightCapture)
    {
        Name = name;
        SnapToGround = snapToGround;
        SnapToWall = snapToWall;

        // Mesh
        MeshInstance3D meshInstance = null;
        if (lightCapture)
            meshInstance = LightCaptureMeshInstance.Instantiate<MeshInstance3D>();
        else
            meshInstance = ProximityFadeMeshInstance.Instantiate<MeshInstance3D>();

        Root.AddChild(meshInstance);
        meshInstance.Owner = this;
        meshInstance.Mesh = mesh;

        // Collision
        if (addCollision)
        {
            StaticBody3D staticBody = new StaticBody3D();
            Root.AddChild(staticBody);
            staticBody.Owner = this;
            staticBody.Name = nameof(StaticBody3D);
            Root.ExcludedCollisionObjects = new NodePath[] { Root.GetPathTo(staticBody) };

            CollisionShape3D collisionShape = new CollisionShape3D();
            staticBody.AddChild(collisionShape);
            collisionShape.Owner = this;
            collisionShape.Name = nameof(CollisionShape3D);
            collisionShape.Shape = mesh.CreateConvexShape();
        }
        
        Root.Initialize();
    }
}