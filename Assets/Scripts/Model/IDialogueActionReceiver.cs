public interface IDialogueActionReceiver {
    string DialogueActionId { get; }
    void ReceiveDialogueAction();
}
