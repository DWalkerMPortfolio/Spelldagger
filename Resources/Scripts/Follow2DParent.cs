using Godot;
using System;

[Tool]
public partial class Follow2DParent : Node3D
{
	[Export] Node2D ParentOverride;
	[Export] Vector3 Offset;
	[Export] public bool SnapToGround;
	[Export(PropertyHint.Layers3DPhysics)] uint GroundLayers = 129;
	[Export] public NodePath[] ExcludedCollisionObjects;
	[Export] bool ActiveInGame = true;
	[Export] bool YScaleMatchX = true;

	[ExportGroup("Dynamic")]
	[Export] public int Floor { get; private set; }
	[Export] Transform2D ParentGlobalTransformPrevious;
	[Export] Node2D twoDParent;
	[Export] Godot.Collections.Array<Rid> ExcludedRids;

	public override void _Ready()
	{
        twoDParent = null;
        CallDeferred(MethodName.Initialize);

		if (!Engine.IsEditorHint())
		{
			if (!ActiveInGame)
				SetProcess(false);
		}
		else
		{
			if (ActiveInGame)
			{
				ProcessMode = ProcessModeEnum.Always;
				foreach (Node child in GetChildren())
				{
					if (child.ProcessMode == ProcessModeEnum.Inherit)
						child.ProcessMode = ProcessModeEnum.Pausable;
				}
			}
		}
	}

	public override void _Process(double delta)
	{
		// Stop processing if not in edited scene
		if (Engine.IsEditorHint() && !IsPartOfEditedScene())
			return;

		// Stop processing if no 2D parent is found
		if (twoDParent == null || !twoDParent.IsInsideTree())
			return;
		
		if (twoDParent.GlobalTransform != ParentGlobalTransformPrevious)
		{
			UpdateTransform();
			ParentGlobalTransformPrevious = twoDParent.GlobalTransform;
		}
	}

	public void SetFloor(int newFloor)
	{
		Floor = newFloor;
		UpdateTransform();
	}

	public void Initialize()
	{
		// Find 2D parent
		if (ParentOverride == null)
		{
			Node Parent = GetParent();
			if (Parent != null && Parent is Node2D)
			{
				twoDParent = (Node2D)Parent;
			}
			else
			{
				GD.PushWarning("No 2D parent found for node: " + Name);
			}
		}
		else
			twoDParent = ParentOverride;

		// Get excluded RIDs
        ExcludedRids = new Godot.Collections.Array<Rid>();
		if (ExcludedCollisionObjects != null)
		{
			foreach (NodePath nodePath in ExcludedCollisionObjects)
			{
				ExcludedRids.Add(((CollisionObject3D)GetNode(nodePath)).GetRid());
			}
		}
    }

	void UpdateTransform()
	{
        if (twoDParent == null || !twoDParent.IsInsideTree())
            return;

        // Position
        Vector3 parentPosition_3D = new Vector3(twoDParent.GlobalPosition.X, 0, twoDParent.GlobalPosition.Y) / Globals.PixelsPerUnit;
        Vector3 currentOffset = Vector3.Up * Floor * Globals.FloorHeight + Offset;
        GlobalPosition = parentPosition_3D + currentOffset;

        // Snap to ground
        if (SnapToGround)
        {
            PhysicsDirectSpaceState3D directSpaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = new PhysicsRayQueryParameters3D();
            query.From = parentPosition_3D with { Y = ((Floor + 1) * Globals.FloorHeight) - 1 };
            query.To = parentPosition_3D with { Y = Floor * Globals.FloorHeight };
            query.CollisionMask = GroundLayers;
            query.Exclude = ExcludedRids;
            Godot.Collections.Dictionary queryResults = directSpaceState.IntersectRay(query);
            if (queryResults.Count > 0)
            {
                GD.Print(queryResults["position"]);
                GlobalPosition = (Vector3)queryResults["position"] + Offset;
            }
        }

        // Rotation
        GlobalRotation = GlobalRotation with { Y = -twoDParent.GlobalRotation };

		// Scale
		float yScale = 1;
		if (YScaleMatchX)
			yScale = twoDParent.Scale.X;
        Scale = new Vector3(twoDParent.Scale.X, yScale, twoDParent.Scale.Y);
    }
}
