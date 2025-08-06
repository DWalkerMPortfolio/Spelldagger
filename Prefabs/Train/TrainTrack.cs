#if TOOLS
using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class TrainTrack : LevelEditorElementPath2D
{
    [ExportGroup("Internal")]
    [Export] Path3D Path3D;
    [Export] MultiMeshInstance3D MultiMesh;
    [Export] Mesh PlankMesh;
    [Export] float PlankInterval;

    public override void _Ready()
    {
        base._Ready();

        if (Engine.IsEditorHint())
        {
            if (Curve == null)
                Curve = new Curve2D();

            GetParent()?.SetEditableInstance(this, true);
            SetDisplayFolded(true);

            if (Owner != this)
            {
                if (MultiMesh.Multimesh == null)
                {
                    MultiMesh.Multimesh = new MultiMesh();
                    MultiMesh.Multimesh.TransformFormat = Godot.MultiMesh.TransformFormatEnum.Transform3D;
                    MultiMesh.Multimesh.Mesh = PlankMesh;
                }

                if (Path3D.Curve == null)
                {
                    Path3D.Curve = new Curve3D();
                    Curve = new Curve2D();
                }
            }
        }
    }

    public override void _Draw()
    {
        base._Draw();

        if (Engine.IsEditorHint())
            GenerateTrack();
    }

    private void GenerateTrack()
    {
        // Update 3D path
        //GD.Print("Updating Path3D");
        Path3D.Curve.ClearPoints();
        for (int i = 0; i < Curve.PointCount; i++)
        {
            Vector3 pointPosition = Globals.TwoDTo3D(Curve.GetPointPosition(i));
            Vector3 pointIn = Globals.TwoDTo3D(Curve.GetPointIn(i));
            Vector3 pointOut = Globals.TwoDTo3D(Curve.GetPointOut(i));
            Path3D.Curve.AddPoint(pointPosition, pointIn, pointOut);
        }

        // Spawn planks between rails
        // From: www.youtube.com/watch?v=Gfpnxg-jne4
        float pathLength = Path3D.Curve.GetBakedLength();
        float plankIntervalClamped = Mathf.Max(PlankInterval, 0.1f);
        float offset = plankIntervalClamped / 2;
        int count = Mathf.FloorToInt(pathLength / plankIntervalClamped);
        MultiMesh.Multimesh.InstanceCount = count;
        for (int i = 0; i < count; i++)
        {
            float curveDistance = offset + plankIntervalClamped * i;
            Transform3D plankTransform = Path3D.Curve.SampleBakedWithRotation(curveDistance);
            MultiMesh.Multimesh.SetInstanceTransform(i, plankTransform);
        }
    }
}
#endif