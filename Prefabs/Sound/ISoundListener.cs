using Godot;
using System;

interface ISoundListener
{
    /// <summary>
    /// Called when overlapping a sound as the sound is created
    /// </summary>
    /// <param name="source">The node responsible for creating the sound</param>
    /// <param name="position">The target position of the sound (not necessarily the center of the sound's radius)</param>
    /// <param name="message">The message carried by the sound</param>
    public void OnHeardSound(Node source, Vector3 position, Sound.Messages message);
}