using Godot;
using System;

public partial class DocumentTemplate : Control
{
    [Export] Label TextLabel;

    public void DisplayDocument(DocumentItem document)
    {
        TextLabel.Text = document.Text;
    }
}
