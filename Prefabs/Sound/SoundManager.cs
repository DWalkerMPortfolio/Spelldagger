using Godot;
using System;

public partial class SoundManager : Node
{
    public static SoundManager Instance;

    [Export] PackedScene SoundPrefab;

    public override void _Ready()
    {
        base._Ready();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.Print("Duplicate sound manager: " + Name);
            QueueFree();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if (Instance == this)
            Instance = null;
    }

    public void CreateSound(Node source, Vector3 position, float radius, Sound.Messages message = Sound.Messages.None, Vector3? targetPosition = null, float duration = 1, float screenShakeAmplitude = 0, float screenShakeDuration = 0, float opacity = 1)
    {
        Sound instantiatedSound = (Sound)SoundPrefab.Instantiate();
        AddChild(instantiatedSound);
        instantiatedSound.GlobalPosition = position;
        instantiatedSound.Play(source, radius, message, targetPosition, duration, screenShakeAmplitude, screenShakeDuration, opacity);
    }
}
