using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    private IInteractor playerInteractor;

    private void Awake()
    {
        playerInteractor = GetComponentInParent<IInteractor>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckInteract(other.gameObject);
    }
    
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