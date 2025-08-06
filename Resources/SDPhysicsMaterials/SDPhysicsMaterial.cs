using Godot;
using System;

public partial class SDPhysicsMaterial : PhysicsMaterial
{
	[Export] public bool DaggersStick = true; // Whether thrown daggers stick in this material
	[Export] public float DaggerHitSoundRadius; // The radius of the sound created when daggers hit this material

	SDPhysicsMaterial() { }
}
