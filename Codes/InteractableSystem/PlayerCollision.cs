using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private IInteractor playerInteractor;

    private void Awake() => playerInteractor = GetComponent<IInteractor>();
    private void OnCollisionEnter(Collision other) => CheckInteract(other.gameObject);

    private void CheckInteract(GameObject other)
    {
        other.transform.TryGetComponent<IInteractable>(out var interactable);

        if (interactable != null)
        {
            if (playerInteractor.TryToInteract(interactable))
            {
                interactable.Interact();
            }
        }
    }

    private void OnDestroy()
    {
        playerInteractor = null;
    }
}