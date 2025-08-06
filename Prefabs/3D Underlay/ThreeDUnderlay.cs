using Godot;
using System;

[Tool]
public partial class ThreeDUnderlay : Node, ISerializationListener
{
	public static ThreeDUnderlay Instance;

	enum LightModes { Visible, Hidden, OnlyIn2D }

    [Export] bool Visible;
	[Export] LightModes Light;

	[ExportCategory("Internal")]
    [Export] SubViewport CaptureViewport;
	[Export] Camera3D CaptureCamera;
	[Export] Sprite2D DisplaySprite;
	[Export] MeshInstance3D DepthDarkener;
	[Export] Control HeightLabelRoot;
	[Export] Label HeightLabel;
	[Export] DirectionalLight3D DirectionalLight;

	[ExportGroup("Dynamic")]
	[Export] float CameraHeight;
	[Export] public int Floor { get; private set; }

    public override void _EnterTree()
    {
		// Destroy if not in editor
		if (!Engine.IsEditorHint())
			QueueFree();

		if (IsPartOfEditedScene())
			Instance = this;
	}

    public override void _ExitTree()
    {
		base._ExitTree();

		if (Instance == this)
			Instance = null;
    }

    public override void _Process(double delta)
	{
#if TOOLS
		if (Engine.IsEditorHint())
		{
			// Set visibility
			DisplaySprite.Visible = Visible;
			DepthDarkener.Visible = Visible && ToolScriptHotkeys.Instance.CurrentMainScreen == "2D";

			// Calculate 2D viewport position
			SubViewport editorViewport2D = EditorInterface.Singleton.GetEditorViewport2D();
			Transform2D global2DCanvasTransform = editorViewport2D.GlobalCanvasTransform;
			Vector2 global2DCanvasTransformScale = global2DCanvasTransform.Scale;
			Vector2 camera2DPosition = -global2DCanvasTransform.Origin / global2DCanvasTransformScale;
			Vector2 camera2DCenterPosition = camera2DPosition + editorViewport2D.Size / global2DCanvasTransformScale / 2;

			// Update display sprite
			DisplaySprite.Scale = new Vector2(1 / global2DCanvasTransformScale.X, 1/global2DCanvasTransformScale.Y);
			DisplaySprite.Position = camera2DCenterPosition;

			// Update height label position
			HeightLabelRoot.Position = DisplaySprite.Position - DisplaySprite.Texture.GetSize() * DisplaySprite.Scale / 2;
			HeightLabelRoot.Size = DisplaySprite.Texture.GetSize();
			HeightLabelRoot.Scale = DisplaySprite.Scale;

			// Update capture viewport
			CaptureViewport.Size = editorViewport2D.Size;

			// Update capture camera
			CaptureCamera.Size = DisplaySprite.Scale.Y * CaptureViewport.Size.Y / Globals.PixelsPerUnit;
			Vector2 camera2DCenterPosition3D = camera2DCenterPosition / Globals.PixelsPerUnit;
			CaptureCamera.GlobalPosition = new Vector3(camera2DCenterPosition3D.X, CaptureCamera.GlobalPosition.Y, camera2DCenterPosition3D.Y);
			CaptureCamera.RotationDegrees = new Vector3(-90, 0, 0);

			// Update light
			switch (Light)
			{
				case LightModes.Visible:
					DirectionalLight.Visible = true;
					break;
				case LightModes.Hidden:
					DirectionalLight.Visible = false;
					break;
				case LightModes.OnlyIn2D:
					DirectionalLight.Visible = ToolScriptHotkeys.Instance.CurrentMainScreen == "2D";
					break;
            }
		}
#endif
	}

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
		if (IsPartOfEditedScene())
			Instance = this;
    }

    public void SetHeight(float cameraHeight, int floor)
    {
		CaptureCamera.GlobalPosition = CaptureCamera.GlobalPosition with { Y = cameraHeight };
		DepthDarkener.GlobalPosition = CaptureCamera.GlobalPosition - Vector3.Up * (Globals.FloorHeight + 0.1f);
		Floor = floor;
		HeightLabel.Text = "Floor: " + floor;
    }
}
