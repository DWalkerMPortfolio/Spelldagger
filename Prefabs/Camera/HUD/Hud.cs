using Godot;
using System;

public partial class Hud : Control
{
    #region Variables
    public static Hud Instance { get; private set; }

    const string READING_DOCUMENT_LOCK = "ReadingDocumentLock";
    const string CLOSE_DOCUMENT_ACTION = "interact";

    [Export] Control DocumentDisplayRoot;

    bool readingDocument;
    Control currentDocument;
    Action currentDocumentCallback;
    Tween documentFadeTween;
    #endregion

    #region Godot Functions
    public override void _EnterTree()
    {
        base._EnterTree();

        if (Instance == null)
            Instance = this;
        else
        {
            GD.PushWarning("Duplicate HUD in scene: " + Name);
            QueueFree();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        CallDeferred(MethodName.UnregisterSingleton);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event.IsActionPressed(CLOSE_DOCUMENT_ACTION))
        {
            if (readingDocument)
                CloseDocument();
        }
    }
    #endregion

    #region Public Functions
    public void DisplayDocument(DocumentItem documentItem, Action onDoneReading = null)
    {
        InputManager.Instance.AddInputLock(READING_DOCUMENT_LOCK);

        if (DocumentDisplayRoot.GetChildCount() > 0)
            DocumentDisplayRoot.GetChild(0).QueueFree(); // Remove previous document if one exists
        
        DocumentTemplate documentTemplate = (DocumentTemplate)documentItem.Template.Instantiate();
        DocumentDisplayRoot.AddChild(documentTemplate);
        documentTemplate.Position = Vector2.Zero;
        documentTemplate.DisplayDocument(documentItem);

        documentFadeTween?.Kill();
        DocumentDisplayRoot.Modulate = Colors.Transparent;
        documentFadeTween = DocumentDisplayRoot.CreateTween();
        documentFadeTween.TweenProperty(DocumentDisplayRoot, (string)Control.PropertyName.Modulate, Colors.White, 0.5f);

        currentDocument = documentTemplate;
        currentDocumentCallback = onDoneReading;
        readingDocument = true;
    }
    #endregion

    #region Private Functions
    void UnregisterSingleton()
    {
        if (Instance == this)
            Instance = null;
    }

    void CloseDocument()
    {
        InputManager.Instance.RemoveInputLock(READING_DOCUMENT_LOCK);

        documentFadeTween?.Kill();
        documentFadeTween = DocumentDisplayRoot.CreateTween();
        documentFadeTween.TweenProperty(DocumentDisplayRoot, (string)Control.PropertyName.Modulate, Colors.Transparent, 0.5f);

        readingDocument = false;
        currentDocumentCallback?.Invoke();
    }
    #endregion
}
