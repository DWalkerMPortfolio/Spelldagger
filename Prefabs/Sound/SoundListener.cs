using Godot;
using System;

public partial class SoundListener : Area3D, ISoundListener
{
    public delegate void HeardSoundDelegate(Node source, Vector3 position, Sound.Messages message);
    public HeardSoundDelegate OnHeardSound;

    void ISoundListener.OnHeardSound(Node source, Vector3 position, Sound.Messages message)
    {
        OnHeardSound.Invoke(source, position, message);
    }
}
