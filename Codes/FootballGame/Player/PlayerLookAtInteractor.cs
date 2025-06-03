using System;
using DG.Tweening;
using Game.FirstRunner.Scripts.Envoirement;
using RootMotion.FinalIK;
using UnityEngine;

public class PlayerLookAtInteractor : PlayerInteractBase
{
    [SerializeField] private PlayerInteractor _playerInteractor;
    [SerializeField] private LookAtController LookAtIK;
    [SerializeField] private PlayerControllerBase PlayerControllerBase;
    private bool interacted;
    private Transform target;

    public override bool TryToInteract(InteractableBase ınteractableBase, Transform target)
    {
        return ınteractableBase.TryToInteract(this);
    }

    public override void InteractEnded(InteractableBase interactableBase)
    {
        if (PlayerControllerBase != GameController.Instance.CurrentPlayerController)
            return;
      //  GameController.Instance.CurrentPlayerController.ResetPlayerState();
        UIControllerBase.Instance.ResetWithTimeOut();
        LookAtIK.weight = 0;
        interacted = false;
        _playerInteractor.OnBallAway(false);
    }

    public void SetTarget(Transform targetTransform)
    {
        if (!PlayerControllerBase.IsActiveCharacter)
            return;
        LookAtIK.SetTarget(targetTransform);
        target = targetTransform;
        interacted = true;
    }

    private void Update()
    {
        if (!PlayerControllerBase.IsActiveCharacter)
            return;
        if (!interacted)
            return;
        if (!GameController.Instance.CurrentPlayerController.PlayerControll.IsActive)
            return;
        var lookPos = target.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, rotation, Time.deltaTime * 1f);
    }

    public override void InteractStarted(InteractableBase interactableBase)
    {
        //  GameController.Instance.CurrentPlayerController.ResetPlayerState();
        if(_playerInteractor ==GameController.Instance.CurrentPlayerController.PlayerInteractor)
            UIControllerBase.Instance.ResetDelays();
        SetTarget(interactableBase.transform);
        _playerInteractor.OnBallAway(true);
    }

    public void DeActivateLook()
    {
        interacted = false; 
        LookAtIK.weight = 0;
    }
}