#if TOOLS
using Godot;
using System;

[Tool]
public partial class SpelldaggerUtilityReferences : Node
{
    [Export] public PackedScene PropBase { get; private set; }
    [Export] public PackedScene GenerateMeshPropsPopup { get; private set; }
}
#endif