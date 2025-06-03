using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.FirstRunner.Scripts.Envoirement;
using RootMotion.FinalIK;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerInteractor : PlayerInteractBase
{
    [SerializeField] private PlayerControllerBase PlayerControllerBase;
    [SerializeField] private Transform LeftFoot, RightFoot, Head;
    [SerializeField] private LookAtController AimIK;
    [SerializeField] private FullBodyBipedIK BipedIK;
    [SerializeField] private GameObject Target;
    [SerializeField] private Transform[] interactableTransforms;
    [SerializeField] private FBBIKHeadEffector HeadController;
    [SerializeField] public GameObject CharacterBall;
    private bool active;
    private bool stillKicking;
    private Tween ActiveTween;
    private Transform transformOfInteractable;
    private Transform Ball;
    private bool disableStarted;
    private bool ballNear;
    Transform target;
    private Transform TargetToShoot;
    private bool kickedWithAnimation;
    public bool characterWaitingForPass;

    private void Start()
    {
        UIControllerBase.Instance.OnActionButtonEvent += OnPressedKick;
        DelayHelper.WaitForAFrame(()=>transformOfInteractable = GameController.Instance.BallInteract.transform); 
    }

    private void OnDisable()
    {
        UIControllerBase.Instance.OnActionButtonEvent -= OnPressedKick;
    }

    public override bool TryToInteract(InteractableBase interactableBase, Transform transformOfInteractable)
    {
        if (GameController.Instance.CurrentPlayerController.CurrentPlayerState != PlayerControllerBase.PlayerState.Pass)
            GameController.Instance.CurrentPlayerController.PlayerInteractor.characterWaitingForPass = false;
        this.transformOfInteractable = transformOfInteractable;
        return interactableBase.TryToInteract(this);
    }

    public override void InteractEnded(InteractableBase interactableBase)
    {
        interactableBase.InteractEnded();
        disableStarted = true;

        if (Ball != null && Vector3.Distance(interactableBase.transform.position, Ball.position) < 1f)
            return;
        DisableBallInteract();
    }

    private void DisableBallInteract()
    {
        ActiveTween.Kill();
        BipedIK.solver.leftFootEffector.target = null;
        BipedIK.solver.rightFootEffector.target = null;
        GameController.Instance.BallInteract.ballRigidbody.useGravity = true;
        stillKicking = false;
        active = false;
        KickPositioning = false;
        Ball = null;
        ResetEffectorWeight(HeadController.positionWeight, value => HeadController.positionWeight = value);
        ResetEffectorWeight(BipedIK.solver.rightFootEffector.positionWeight,
            value => BipedIK.solver.rightFootEffector.positionWeight = value);
        ResetEffectorWeight(BipedIK.solver.leftFootEffector.positionWeight,
            value => BipedIK.solver.leftFootEffector.positionWeight = value);
    }

    private void ResetEffectorWeight(float startValue, TweenCallback<float> onUpdate)
    {
        DOVirtual.Float(startValue, 0, .2f, onUpdate)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(() => PlayerControllerBase.IsPlayerInteractingWithBall = false);
    }

    public override void InteractStarted(InteractableBase interactableBase)
    {
        KickPositioning = false;
        disableStarted = false;
        if (interactableBase.GetRigidbody().linearVelocity.y > 0.1f)
            return;
        kickedWithAnimation = false;
        GameController.Instance.BallInteract.IsActive = true;
        SetTarget(interactableBase.transform);
        interactableBase.SetInteractor(this);
    }

    private void OnPressedKick()
    {
        if (!PlayerControllerBase.IsActiveCharacter)
            return;
        Ball = GameController.Instance.BallInteract.transform;
        if (!ballNear)
            return;
        KickPositioning = true;
    }

    public void OnBallAway(bool isNear)
    {
        ballNear = isNear;
        if (!isNear)
            KickPositioning = false;
        else if (GameController.Instance.CurrentPlayerController.CurrentPlayerState !=
                 PlayerControllerBase.PlayerState.Empty)
            OnPressedKick();
    }

    private Tween ActiveBallTween;

    public void SetTarget(Transform targetTransform)
    {
        if (GameController.Instance.CurrentPlayerController != PlayerControllerBase)
            return;
        ActiveBallTween.Kill();
        if (PlayerControllerBase.PlayerState.Empty ==
            GameController.Instance.CurrentPlayerController.CurrentPlayerState)
        {
            if (GameController.Instance.BallInteract.ballRigidbody.linearVelocity.y > -0.1f &&
                GameController.Instance.BallInteract.ballRigidbody.linearVelocity.y < 0.1f)
            {
                ActiveBallTween = targetTransform.DOMove(CharacterBall.transform.position, .1f).OnComplete(() =>
                {
                    CharacterBall.SetActive(true);
                    targetTransform.gameObject.SetActive(false);
                    GameController.Instance.CharacterWalkingWithABall = true;
                    GameController.Instance.CameraController.SetPlayerAsTarget();
                    GameController.Instance.CurrentPlayerController.PlayerLookAtInteractor.DeActivateLook();
                });

                return;
            }
        }

        if (GameController.Instance.CharacterWalkingWithABall)
        {
            targetTransform.transform.position = CharacterBall.transform.position;
            GameController.Instance.CameraController.SetBallAsTarget();
        }

        targetTransform.gameObject.SetActive(true);
        CharacterBall.SetActive(false);
        GameController.Instance.CharacterWalkingWithABall = false;
        if (!PlayerControllerBase.IsActiveCharacter)
            return;
        if (PlayerControllerBase.IsPlayerInteractingWithBall)
            return;
        if (stillKicking)
            return;
        if (IsBallMovingUpwards())
            return;
        ActiveTween.Kill();
        BipedIK.solver.leftFootEffector.target = null;
        BipedIK.solver.leftFootEffector.positionWeight = 0;
        BipedIK.solver.rightFootEffector.target = null;
        BipedIK.solver.rightFootEffector.positionWeight = 0;
        active = true;
        KickTarget(targetTransform);
    }

    private bool IsBallMovingUpwards()
    {
        Vector3 acceleration = GameController.Instance.BallInteract.ballRigidbody.linearVelocity.normalized;
        return Vector3.Dot(acceleration, Vector3.up) > 0.5f;
    }

    public void KickTarget(Transform ball)
    {
        if (!active || stillKicking || !PlayerControllerBase.IsActiveCharacter || KickPositioning)
            return;

        HandleBallState(GameController.Instance.CurrentPlayerController.CurrentPlayerState);

        Ball = ball;
        TurnTarget();
    }

    private Rigidbody ballRig;

    private void HandleBallState(PlayerControllerBase.PlayerState state)
    {
        if (PlayerControllerBase.PlayerState.Empty == state)
            return;
        ballRig = GameController.Instance.BallInteract.ballRigidbody;
        ballRig.linearVelocity = Vector3.zero;
        ballRig.angularVelocity = Vector3.zero;

        switch (state)
        {
            case PlayerControllerBase.PlayerState.Bounce:
                ballRig.useGravity = false;
                ballRig.transform.DOMoveY(transformOfInteractable.position.y, .2f)
                    .SetUpdate(UpdateType.Fixed)
                    .SetEase(Ease.Linear);
                break;

            case PlayerControllerBase.PlayerState.Shoot:
                ballRig.useGravity = false;
                GameController.Instance.BallInteract.IsActive = false;
                ballRig.transform.DOMoveY(transformOfInteractable.position.y, .2f)
                    .SetUpdate(UpdateType.Fixed)
                    .SetEase(Ease.Linear);
                break;
            case PlayerControllerBase.PlayerState.Pass:
                GameController.Instance.BallInteract.IsActive = false;
                break;

            case PlayerControllerBase.PlayerState.Special:
                ballRig.useGravity = false;
                GameController.Instance.BallInteract.IsActive = false;
                ballRig.transform.DOMoveY(transformOfInteractable.position.y, .2f)
                    .SetUpdate(UpdateType.Fixed)
                    .SetEase(Ease.Linear);
                break;
        }

        ballRig = null;
    }

    private Dictionary<string, float> distances;

    private void KickBallMovement(Transform targetTransform, bool rightFoot)
    {
        PlayerControllerBase.PlayerControll.StopMovement();
        PlayerControllerBase.IsPlayerInteractingWithBall = true;
        ActiveTween.Kill();
        stillKicking = true;

        Target.transform.position = GameController.Instance.BallInteract.transform.position;

        distances = new Dictionary<string, float>
        {
            { "RightFoot", Vector3.Distance(targetTransform.position, RightFoot.position) },
            { "LeftFoot", Vector3.Distance(targetTransform.position, LeftFoot.position) },
            { "Head", Vector3.Distance(targetTransform.position, Head.position) }
        };

        string closestPart = distances.OrderBy(pair => pair.Value).First().Key;
        if (kickedWithAnimation)
        {
            if (rightFoot)
                ExecuteFootKick(Target.transform, BipedIK.solver.rightFootEffector, true);
            else
                ExecuteFootKick(Target.transform, BipedIK.solver.leftFootEffector, true);
        }
        else
        {
            switch (closestPart)
            {
                case "RightFoot":
                    ExecuteFootKick(Target.transform, BipedIK.solver.rightFootEffector, false);
                    break;
                case "LeftFoot":
                    ExecuteFootKick(Target.transform, BipedIK.solver.leftFootEffector, false);
                    break;
                case "Head":
                    ExecuteHeadKick();
                    break;
            }
        }
    }

    private void ExecuteFootKick(Transform target, IKEffector footEffector, bool isSpecial)
    {
        footEffector.target = target;
        ActiveTween = DOVirtual
            .Float(0, .7f, .2f, value => { footEffector.positionWeight = value; })
            .OnComplete(() =>
            {
                GameController.Instance.CurrentPlayerController.playerBallController.GiveForce(TargetToShoot);
                //if Pass
                ActiveTween = DOVirtual.Float(.7f, 0, .2f, value =>
                {
                    footEffector.target = null;
                    if (disableStarted)
                        DisableBallInteract();
                    PlayerControllerBase.IsPlayerInteractingWithBall = false;
                    footEffector.positionWeight = value;
                    stillKicking = false;
                    GameController.Instance.CurrentPlayerController.PlayerControll.IsActive = true;
                }).SetUpdate(UpdateType.Fixed);
            }).SetUpdate(UpdateType.Fixed);
    }

    private void ExecuteHeadKick()
    {
        HeadController.transform.position = transformOfInteractable.position;
        HeadController.transform.position =
            GameController.Instance.BallInteract.transform.position + Vector3.down * 0.3f;

        ActiveTween = DOVirtual.Float(0, .7f, .2f, value =>
        {
            AimIK.weight = value;
            HeadController.positionWeight = value;
        }).OnComplete(() =>
        {
            GameController.Instance.CurrentPlayerController.playerBallController.GiveForce(TargetToShoot);
            ActiveTween = DOVirtual.Float(.7f, 0, .2f, value =>
            {
                if (disableStarted)
                    DisableBallInteract();
                PlayerControllerBase.IsPlayerInteractingWithBall = false;
                AimIK.weight = value;
                HeadController.positionWeight = value;
                stillKicking = false;
                GameController.Instance.CurrentPlayerController.PlayerControll.IsActive = true;
            }).SetUpdate(UpdateType.Fixed).OnComplete(() => HeadController.transform.localPosition = Vector3.zero);
        }).SetUpdate(UpdateType.Fixed);
    }

    private void TurnTarget()
    {
        if (GameController.Instance.CurrentPlayerController.CurrentPlayerState ==
            PlayerControllerBase.PlayerState.Pass)
            target = SelectClosestOne();
        if (GameController.Instance.CurrentPlayerController.CurrentPlayerState ==
            PlayerControllerBase.PlayerState.Shoot)
        {
            target = GameController.Instance.Goal
                .GoalPoints[Random.Range(0, GameController.Instance.Goal.GoalPoints.Length)].transform;
            TargetToShoot = target;
        }

        if (GameController.Instance.CurrentPlayerController.CurrentPlayerState ==
            PlayerControllerBase.PlayerState.Special)
        {
            target = GameController.Instance.Goal
                .GoalPoints[Random.Range(0, GameController.Instance.Goal.GoalPoints.Length)].transform;
            if (Ball.transform.position.y < 1.5f)
                GameController.Instance.CurrentPlayerController.CurrentPlayerState =
                    PlayerControllerBase.PlayerState.Special2;
            TargetToShoot = target;
        }

        if (target != null)
        {
            GameController.Instance.CurrentPlayerController.PlayerControll.IsActive = false;
            GameController.Instance.CurrentPlayerController.PlayerControll.StopWalking();
            AimIK.weight = 0;
            KickPositioning = true;
        }
        else
            KickBallMovement(Ball, false);
    }

    private Transform closestPlayer;

    Transform SelectClosestOne()
    {
        closestPlayer = null;
        for (int i = 0; i < GameController.Instance.AllPlayerControllers.Count; i++)
        {
            if (GameController.Instance.AllPlayerControllers[i] == GameController.Instance.CurrentPlayerController)
                continue;
            Vector3 directionToPlayer = GameController.Instance.AllPlayerControllers[i].transform.position -
                                        transform.position;
            float angle = Vector3.Angle(transform.forward,
                directionToPlayer);

            if (70 <= angle || angle <= 130)
            {
                if (closestPlayer == null)
                {
                  //  Debug.Log(angle);
                    closestPlayer = GameController.Instance.AllPlayerControllers[i].transform;
                }
            }
        }

        TargetToShoot = closestPlayer;
        return closestPlayer;
    }

    private bool KickPositioning;

    private void FixedUpdate()
    {
        MoveSecondPlayerNearFirst();
        if (!ballNear)
            return;
        if (characterWaitingForPass && GameController.Instance.CurrentPlayerController.CurrentPlayerState ==
            PlayerControllerBase.PlayerState.Pass)
        {
            if(stillKicking|| GameController.Instance.CurrentPlayerController != PlayerControllerBase || !IsCloseEnoughWityY(GameController.Instance.BallInteract.transform.position, GameController.Instance.CurrentPlayerController.transform.position, 1.3f))
                return;
            kickedWithAnimation = false;
            TargetToShoot = GameController.Instance.currentSecondClosestOne.transform; 
            KickBallMovement(GameController.Instance.BallInteract.transform, false);
            return;
        }
        if (!KickPositioning)
        {
            PlayerControllerBase.PlayerControll.ExtraVelocityActiveState(false);
            return;
        }

        if (Ball == null)
            return;

        if (target == null)
        {
            MoveTowardsBall();
        }
        else
        {
            MoveTowardsTarget();
        }
    }

    private Vector3 directionToBall;

    private void MoveTowardsBall()
    {
        directionToBall = GetDirectionTo(Ball.transform.position);

        if (IsCloseEnough(GameController.Instance.CurrentPlayerController.transform.position, Ball.transform.position,
                .1f))
        {
            PlayerControllerBase.PlayerControll.ExtraVelocityActiveState(false);
            PlayerControllerBase.PlayerControll.StopMovement();
            LookAt(Ball.transform.position);
            KickPositioning = false;
            if (GameController.Instance.BallInteract.ballRigidbody.linearVelocity.y > -0.1f &&
                GameController.Instance.BallInteract.ballRigidbody.linearVelocity.y < 0.1f)
                SetTarget(GameController.Instance.BallInteract.transform);
        }
        else
        {
            PlayerControllerBase.PlayerControll.ExtraVelocityActiveState(true);
            if (PlayerControllerBase.PlayerControll == GameController.Instance.CurrentPlayerController.PlayerControll)
                PlayerControllerBase.PlayerControll.ApplyExtraVelocity(directionToBall * 2f,
                    1f + Ball.transform.GetComponent<Rigidbody>().linearVelocity.magnitude);
        }
    }

    private Vector3 directionToTarget;
    private Vector3 positionBehindBall;
    private Vector3 directionToMove;

    private void MoveTowardsTarget()
    {
        directionToTarget = GetDirectionTo(target.position, Ball.transform.position);
        positionBehindBall = GetPositionBehindBall(directionToTarget, 1f);
        directionToMove = GetDirectionTo(positionBehindBall);
        PlayerControllerBase.PlayerControll.ExtraVelocityActiveState(true);
        if (PlayerControllerBase.PlayerControll == GameController.Instance.CurrentPlayerController.PlayerControll)
            PlayerControllerBase.PlayerControll.ApplyExtraVelocity(directionToMove * 2f,
                1f + Ball.transform.GetComponent<Rigidbody>().linearVelocity.magnitude);
        LookAt(target.position);
        if (IsCloseEnough(GameController.Instance.CurrentPlayerController.transform.position, positionBehindBall, 0.3f))
        {
            PlayerControllerBase.PlayerControll.StopMovement();
            GameController.Instance.CurrentPlayerController.playerAnimationController.PlaySelectedShootAnimation(
                0.3f, (bool RightFoot, bool animated) =>
                {
                    kickedWithAnimation = animated;
                    KickBallMovement(Ball, RightFoot);
                });
            KickPositioning = false;
            target = null;
            PlayerControllerBase.PlayerControll.ExtraVelocityActiveState(false);
        }
    }

    private Vector3 directionFromFirstPlayer;
    private Vector3 pointNearFirstPlayer;
    private Vector3 directionToMoveSecondPlayer;

    private void MoveSecondPlayerNearFirst()
    {
        if (PlayerControllerBase != GameController.Instance.currentSecondClosestOne)
            return;
        if (GameController.Instance.currentClosestOne == null ||
            GameController.Instance.currentSecondClosestOne == null)
            return;
        // Calculate a point near the first player
        directionFromFirstPlayer = Random.insideUnitSphere; // Random direction
        directionFromFirstPlayer.y = 0; // Keep movement on the horizontal plane
        pointNearFirstPlayer = GameController.Instance.currentClosestOne.transform.position +
                               directionFromFirstPlayer.normalized * 2f; // 2 units away

        // Calculate the distance and determine speed based on proximity
        float distanceToTarget = Vector3.Distance(transform.position,
            pointNearFirstPlayer);
        float speedFactor =
            Mathf.Clamp(distanceToTarget / 5f, 0.1f,
                1f); // Speed decreases as distance decreases, clamped between 0.1 and 1
        directionToMoveSecondPlayer =
            (pointNearFirstPlayer - transform.position).normalized;
        transform.LookAt(GameController.Instance.currentClosestOne.transform.position);

        if (IsCloseEnough(transform.position, pointNearFirstPlayer, 8f))
            return;
        PlayerControllerBase.PlayerControll.ApplyExtraVelocity(directionToMoveSecondPlayer, speedFactor * 0.5f);
    }

    private Vector3 direction;

    private Vector3 GetDirectionTo(Vector3 targetPosition, Vector3? fromPosition = null)
    {
        direction = (targetPosition -
                     (fromPosition ?? GameController.Instance.CurrentPlayerController.transform.position));
        direction.y = 0;
        return direction.normalized;
    }

    private Vector3 GetpositionBehindBall;

    private Vector3 GetPositionBehindBall(Vector3 directionToTarget, float desiredDistance)
    {
        GetpositionBehindBall = Ball.transform.position - directionToTarget * desiredDistance;
        GetpositionBehindBall.y = GameController.Instance.CurrentPlayerController.transform.position.y;
        return GetpositionBehindBall;
    }

    private bool IsCloseEnough(Vector3 currentPosition, Vector3 targetPosition, float threshold)
    {
        return Vector3.Distance(new Vector3(currentPosition.x, 0, currentPosition.z),
            new Vector3(targetPosition.x, 0, targetPosition.z)) <= threshold;
    }
    private bool IsCloseEnoughWityY(Vector3 currentPosition, Vector3 targetPosition, float threshold)
    {
        return Vector3.Distance(currentPosition,
            targetPosition) <= threshold;
    }
    private Vector3 lookPos;

    private void LookAt(Vector3 targetPosition)
    {
        lookPos = targetPosition;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
    }
}