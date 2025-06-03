using System;
using Animancer;
using RootMotion.FinalIK;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class GoalKeeperAI : MonoBehaviour
{
    [Header("Animancer & Animations")]
    [SerializeField] private AnimancerComponent AnimancerComponent;
    [SerializeField] private AnimationClip[] Jumps;
    [SerializeField] private AnimationClip Idle;
    [SerializeField] private AnimationClip RightWalk, LeftWalk;
    [Header("Limb GameObjects & IK")]
    [SerializeField] private GameObject RightFoot; 
    [SerializeField] private GameObject LeftFoot;
    [SerializeField] private GameObject RightHand;
    [SerializeField] private GameObject LeftHand;
    [SerializeField] private FullBodyBipedIK FullBodyBipedIK;

    
    private Rigidbody ballRigidbody;

    [Header("Movement & Hit Parameters")]
    [SerializeField] private float moveDuration = 1f;           
    [SerializeField] private float hitTweenDuration = 1f;        
    [SerializeField] private float hitThreshold = 0.5f;          
    [SerializeField] private float forceMultiplier = 500f;      

    private bool isJumping = false;
    private bool isHitting = false;
    private Vector3 initialPosition;
    private Tween moveTween;
    private Tween hitTween;
    
    private void Start()
    {
        initialPosition = transform.position;
        AnimancerComponent.Play(Idle);
    }

    private void OnEnable()
    {
        PlayerInteractionController.BallKeeperJump += JumpToCatch;
    }

    private void OnDisable()
    {
        PlayerInteractionController.BallKeeperJump -= JumpToCatch;
    }
    
    public void JumpToCatch()
    {
        if (isJumping) return;

        isJumping = true;
        var rnd = Random.Range(0, Jumps.Length);
        var anim = AnimancerComponent.Play(Jumps[rnd], 0.3f);
        anim.Speed = 1.5f;
        anim.Events(this).OnEnd = () =>
        {
            AnimancerComponent.Play(Idle, 0.7f);
            isJumping = false;
        };
    }

    /// <summary>
    /// Waits for a trigger then activates this 
    ///  if the ball is in the treshold tries to kick 
    /// </summary>
    public void TryToMoveTowardsBall(BallInteract ballInteract)
    {
        ballRigidbody = ballInteract.ballRigidbody;
        
        if (isJumping) return; // Atlama modunda değilse çalışsın

        moveTween?.Kill();
        // Yalnızca yatay eksende (X) hareket – Y/Z sabit tutuluyor
        Vector3 targetPosition = new Vector3(ballInteract.transform.position.x, initialPosition.y, initialPosition.z);
        
        if(targetPosition.x > transform.position.x)
        {
            AnimancerComponent.Play(RightWalk, 0.3f);
        }
        else
        {
            AnimancerComponent.Play(LeftWalk, 0.3f);
        }
        moveTween = transform.DOMoveX(targetPosition.x, moveDuration).SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (!isHitting)
                {
                    ballRigidbody.linearVelocity = Vector3.zero;
                    ballRigidbody.angularVelocity = Vector3.zero;
                    // Check the distance between limbs 
                    float distanceRF = Vector3.Distance(ballRigidbody.position, RightFoot.transform.position);
                    float distanceLF = Vector3.Distance(ballRigidbody.position, LeftFoot.transform.position);
                    float distanceRH = Vector3.Distance(ballRigidbody.position, RightHand.transform.position);
                    float distanceLH = Vector3.Distance(ballRigidbody.position, LeftHand.transform.position);
                    float minDistance = distanceRF;
                    if (distanceLF < minDistance) minDistance = distanceLF;
                    if (distanceRH < minDistance) minDistance = distanceRH;
                    if (distanceLH < minDistance) minDistance = distanceLH;

                    if (minDistance < hitThreshold)
                    {
                        PerformHit();
                    }
                }
            }).SetSpeedBased();
    }

    /// <summary>
    /// find closest limb, increase weight with dotween.
    /// if it is close to the limb,
    /// give force to the ball opposite of the direction.
    /// </summary>
    private void PerformHit()
    {
        isHitting = true;
        
        float distanceRF = Vector3.Distance(ballRigidbody.position, RightFoot.transform.position);
        float distanceLF = Vector3.Distance(ballRigidbody.position, LeftFoot.transform.position);
        float distanceRH = Vector3.Distance(ballRigidbody.position, RightHand.transform.position);
        float distanceLH = Vector3.Distance(ballRigidbody.position, LeftHand.transform.position);

        GameObject selectedPart = RightFoot;
        IKEffector selectedEffector = FullBodyBipedIK.solver.rightFootEffector;
        float minDistance = distanceRF;

        if (distanceLF < minDistance)
        {
            minDistance = distanceLF;
            selectedPart = LeftFoot;
            selectedEffector = FullBodyBipedIK.solver.leftFootEffector;
        }
        if (distanceRH < minDistance)
        {
            minDistance = distanceRH;
            selectedPart = RightHand;
            selectedEffector = FullBodyBipedIK.solver.rightHandEffector;
        }
        if (distanceLH < minDistance)
        {
            minDistance = distanceLH;
            selectedPart = LeftHand;
            selectedEffector = FullBodyBipedIK.solver.leftHandEffector;
        }

        selectedEffector.target = ballRigidbody.transform; 
       
        hitTween = DOTween.To(() => selectedEffector.positionWeight,
                               x => selectedEffector.positionWeight = x,
                               0.8f,
                               hitTweenDuration).OnComplete(() =>
            {
                ballRigidbody.AddForce(transform.forward * forceMultiplier+transform.up*forceMultiplier/2, ForceMode.Impulse);
                ResetIKEffectorWeights();
                selectedEffector.target = null; 
                isHitting = false;
            });
    }
    
    private void ResetIKEffectorWeights()
    {
        float resetDuration = 0.5f; 

        DOTween.To(() => FullBodyBipedIK.solver.rightFootEffector.positionWeight,
            x => FullBodyBipedIK.solver.rightFootEffector.positionWeight = x,
            0f,
            resetDuration);

        DOTween.To(() => FullBodyBipedIK.solver.leftFootEffector.positionWeight,
            x => FullBodyBipedIK.solver.leftFootEffector.positionWeight = x,
            0f,
            resetDuration);

        DOTween.To(() => FullBodyBipedIK.solver.rightHandEffector.positionWeight,
            x => FullBodyBipedIK.solver.rightHandEffector.positionWeight = x,
            0f,
            resetDuration);

        DOTween.To(() => FullBodyBipedIK.solver.leftHandEffector.positionWeight,
            x => FullBodyBipedIK.solver.leftHandEffector.positionWeight = x,
            0f,
            resetDuration);
    }

    /// <summary>
    /// if the ball is away return at the Initial pose 
    /// </summary>
    public void ReturnToInitialPosition()
    { 
        if(isJumping) return;
        ballRigidbody = null;
        moveTween?.Kill();
        hitTween?.Kill();
        ResetIKEffectorWeights();
        isHitting = false;
        if(transform.position.x > initialPosition.x)
        {
            AnimancerComponent.Play(LeftWalk, 0.3f);
        }
        else
        {
            AnimancerComponent.Play(RightWalk, 0.3f);
        }
        transform.DOMove(initialPosition, moveDuration).SetEase(Ease.Linear).SetSpeedBased().OnComplete(()=> AnimancerComponent.Play(Idle, 0.7f));
    }
   
}
