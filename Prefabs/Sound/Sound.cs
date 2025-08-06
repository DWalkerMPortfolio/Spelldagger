using Godot;
using Godot.Collections;
using System;

public partial class Sound : Node3D
{
    public enum Messages { None, Investigate, Alert, AllClear}

    [Export] MeshInstance3D Mesh;
    [Export] Area3D Area;
    [Export] float ScaleInDuration;
    [Export] Dictionary<Messages, Color> MessageColors;

    Messages message;
    Node source;
    Vector3 targetPosition;

    public override void _Ready()
    {
        base._Ready();

        CallDeferred(MethodName.OverlappingBodies);
    }

    async void OverlappingBodies()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        foreach (Node3D body in Area.GetOverlappingBodies())
        {
            ISoundListener soundListener = body as ISoundListener;
            if (soundListener != null && soundListener != source)
            {
                soundListener.OnHeardSound(source, targetPosition, message);
            }
        }

        foreach (Area3D area in Area.GetOverlappingAreas())
        {
            ISoundListener soundListener = area as ISoundListener;
            if (soundListener != null && soundListener != source)
            {
                soundListener.OnHeardSound(source, targetPosition, message);
            }
        }
    }

    public void Play(Node source, float radius, Messages message, Vector3? targetPosition, float duration, float screenShakeAmplitude, float screenShakeDuration, float opacity)
    {
        Mesh.Scale = new Vector3(radius * 2, Mesh.Scale.Y, radius * 2);
        this.message = message;
        this.source = source;
        if (targetPosition != null)
            this.targetPosition = targetPosition.Value;
        else
            this.targetPosition = GlobalPosition;
        Color color = MessageColors[message];
        color.A *= opacity;
        Mesh.SetInstanceShaderParameter("color", color);

        // TODO: Reimplement screenshake

        Tween tween = CreateTween();
        tween.TweenMethod(Callable.From((float value) => { Mesh.SetInstanceShaderParameter("max_distance", value); }), radius / 2, radius, ScaleInDuration * duration)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        tween.Parallel().TweenMethod(Callable.From((float value) => { Mesh.SetInstanceShaderParameter("alpha", value); }), 0.0f, 1.0f, ScaleInDuration * duration)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        tween.TweenMethod(Callable.From((float value) => { Mesh.SetInstanceShaderParameter("alpha", value); }), 1.0f, 0.0f, duration - (ScaleInDuration * duration));
        tween.TweenCallback(Callable.From(QueueFree));
    }
}
