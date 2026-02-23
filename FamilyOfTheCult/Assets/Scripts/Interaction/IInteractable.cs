using UnityEngine;

public interface IInteractable
{
    public void Interact();

    public void HoldInteract();

    public void HighlightItem();

    public void UnHighlightItem();

    public void CannotInteract();
}
