namespace ChoDoLa.Interaction
{
    /// <summary>
    /// Contract for 3D objects in the market scene that the player can look at and interact with.
    /// </summary>
    public interface IInteractable
    {
        string PromptText { get; }
        bool CanInteract { get; }
        void Interact();
    }
}
