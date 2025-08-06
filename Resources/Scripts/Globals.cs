using Godot;
using System;

public partial class Globals : Resource
{
	public static int PixelsPerUnit { get; private set; } = 64;
	public static string GravitySetting { get; private set; } = "physics/3d/default_gravity";
	public static float FloorHeight { get; private set; } = 5;
	public static float FloorFadeDuration { get; private set; } = 0.5f;

	public static T[] ConvertNodePathArray<T>(Node owner, NodePath[] array) where T : Node
	{
		if (array == null)
			return new T[0];

		T[] result = new T[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			result[i] = owner.GetNode<T>(array[i]);
		}
		return result;
	}

	public static Vector3 TwoDTo3D(Vector2 twoD)
	{
		return new Vector3(twoD.X, 0, twoD.Y) / PixelsPerUnit;
	}

	public static Vector2 ThreeDTo2D(Vector3 threeD)
	{
		return new Vector2(threeD.X, threeD.Z) * PixelsPerUnit;
	}
}
