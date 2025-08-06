using Godot;
using System;

public partial class PlayerVisionDisplay : MeshInstance3D
{
    const string TRANSPARENCY_PARAMETER = "transparency";
    const string FADE_BACKWARDS_PARAMETER = "transparency_fade_backwards";

    [Export] Follow2DParent Root;
    [Export] bool VisibleInGame = true;

    int currentCameraFloor;
    Tween changeFloorTween;

    public override void _Ready()
    {
        base._Ready();

        if (VisibleInGame)
        {
            currentCameraFloor = MainCamera.Instance.CurrentFloor;
            SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, currentCameraFloor == Root.Floor ? 0.0 : 1.0);
            MainCamera.Instance.FloorChanged += OnCameraFloorChanged;
        }
        else
        {
            Visible = false;
            SetProcess(false);
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if (VisibleInGame)
            MainCamera.Instance.FloorChanged -= OnCameraFloorChanged;
    }

    void OnCameraFloorChanged(int newFloor)
    {
        bool goingDown = newFloor < currentCameraFloor;

        if (Root.Floor == currentCameraFloor || Root.Floor == newFloor)
        {
            changeFloorTween?.Kill();
            changeFloorTween = CreateTween();
        }

        if (goingDown)
        {
            if (Root.Floor == newFloor)
            {
                if (!TemporalController.RestoringSnapshots)
                {
                    SetInstanceShaderParameter(FADE_BACKWARDS_PARAMETER, true);
                    SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, 1.0);
                    changeFloorTween.TweenMethod(Callable.From((float t) => { SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, t); }), 1.0, 0.0, Globals.FloorFadeDuration).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
                }
                else
                    SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, 0.0);
            }
            else if (Root.Floor == currentCameraFloor)
            {
                if (!TemporalController.RestoringSnapshots)
                {
                    SetInstanceShaderParameter(FADE_BACKWARDS_PARAMETER, false);
                    SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, 0.0);
                    changeFloorTween.TweenMethod(Callable.From((float t) => { SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, t); }), 0.0, 1.0, Globals.FloorFadeDuration).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
                }
                else
                    SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, 1.0);
            }
        }
        else
        {
            if (Root.Floor == newFloor)
            {
                if (!TemporalController.RestoringSnapshots)
                {
                    SetInstanceShaderParameter(FADE_BACKWARDS_PARAMETER, false);
                    SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, 1.0);
                    changeFloorTween.TweenMethod(Callable.From((float t) => { SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, t); }), 1.0, 0.0, Globals.FloorFadeDuration).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
                }
                else
                    SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, 0.0);
            }
            else if (Root.Floor == currentCameraFloor)
            {
                if (!TemporalController.RestoringSnapshots)
                {
                    SetInstanceShaderParameter(FADE_BACKWARDS_PARAMETER, true);
                    SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, 0.0);
                    changeFloorTween.TweenMethod(Callable.From((float t) => { SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, t); }), 0.0, 1.0, Globals.FloorFadeDuration).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
                }
                else
                    SetInstanceShaderParameter(TRANSPARENCY_PARAMETER, 1.0);
            }
        }

        currentCameraFloor = newFloor;
    }
}
