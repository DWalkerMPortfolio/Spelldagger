using Godot;
using System;

public partial class DestroyTimer : Timer
{
	[Export] Node DestroyTarget;

	public override void _Ready()
	{
		Timeout += Destroy;
	}

	void Destroy()
	{
		DestroyTarget.QueueFree();
	}
}
