using Godot;
using System;

[Tool]
public partial class WallSlicer : CsgBox3D
{
	[Export] public bool SliceOccluder;
}
