using Godot;
using System;

public partial class ViewportMatchSize : SubViewport
{
    public override void _Ready()
    {
        base._Ready();

        GetWindow().SizeChanged += OnWindowSizeChanged;
        OnWindowSizeChanged();
    }

    private void OnWindowSizeChanged()
    {
        Size = GetWindow().Size;
    }
}
