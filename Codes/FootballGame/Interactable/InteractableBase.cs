using UnityEngine;


namespace Game.FirstRunner.Scripts.Envoirement
{
    public abstract class InteractableBase : MonoBehaviour
    {
        public abstract void Interact();
        public abstract bool TryToInteract(PlayerInteractBase playerInteractor); 
        public abstract void InteractEnded();
        public abstract Rigidbody GetRigidbody();
        public abstract void SetInteractor(PlayerInteractor playerInteractor); 
    }
}
