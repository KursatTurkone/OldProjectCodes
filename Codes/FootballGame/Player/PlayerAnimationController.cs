using System;
using System.Collections.Generic;
using Animancer;
using Animancer.Units;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private PlayerControllerBase PlayerControllerBase;
    [SerializeField] private Rigidbody Rigidbody;
    [SerializeField] private TransitionAssetBase _Transition;
    [SerializeField] private StringAsset _ParameterX;
    [SerializeField] private StringAsset _ParameterY;
    [SerializeField, Seconds] private float _ParameterSmoothTime = 0.15f;
    [SerializeField] private FootSound footSound; 
    private Dictionary<PlayerControllerBase.PlayerState, (AnimationClip startClip, AnimationClip EndClip, Vector2
        AnimationHeightRange, bool RightLeg,float AnimSpeed
        )> dict;

    private SmoothedVector2Parameter _SmoothedParameters;
    private Vector2 localMovement;
    public StateAnimations[] ShootAnimationsWithState;

    [System.Serializable]
    public class StateAnimations
    {
        public PlayerControllerBase.PlayerState _state;
        public AnimationClip StartShootClip;
        public AnimationClip EndShootClip;
        [MinMaxSlider(0, 2.7f, true)] public Vector2 AnimationHeightRange = new Vector2(1, 1.5f);
        public bool RightLeg;
        public float AnimationSpeed = 1;
    }

    private void Start()
    {
        dict = new Dictionary<PlayerControllerBase.PlayerState, (AnimationClip, AnimationClip, Vector2, bool,float)>();
        foreach (var state in ShootAnimationsWithState)
        {
            dict[state._state] = (state.StartShootClip, state.EndShootClip, state.AnimationHeightRange, state.RightLeg,state.AnimationSpeed);
        }

      animancer.Play(_Transition);
        _SmoothedParameters = new SmoothedVector2Parameter(
            animancer,
            _ParameterX,
            _ParameterY,
            _ParameterSmoothTime);
    }


    public void PlaySelectedShootAnimation(float fadeDuration,
        Action<bool,bool> onEndCallback)
    {
        if (dict.TryGetValue(GameController.Instance.CurrentPlayerController.CurrentPlayerState,
                out var state))
        {
            Transform ballPos = GameController.Instance.BallInteract.transform;
            if (ballPos.position.y < state.AnimationHeightRange.x || ballPos.position.y > state.AnimationHeightRange.y)
            {
                onEndCallback?.Invoke(false,false);
                return;
            }

            bool doOnce = false;
            var anim = animancer.Play(state.startClip, fadeDuration);
            anim.Speed = state.AnimSpeed;

            anim.Events(this).OnEnd = () =>
            {
                if (doOnce)
                    return;
                doOnce = true;
                onEndCallback?.Invoke(state.RightLeg,true);
                var anim = animancer.Play(state.EndClip, .01f);
                anim.Speed = 1;
                anim.Events(this).OnEnd = () => { animancer.Play(_Transition); };
            };
        }
        else
        {
            onEndCallback?.Invoke(state.RightLeg,false);
        }
    }


    private void Update()
    {
       // if (GameController.Instance.CurrentPlayerController == PlayerControllerBase)
            GetLocalMovement();
    }

    public void GetLocalMovement()
    {
        Vector3 velocity = Rigidbody.linearVelocity;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        localMovement = new Vector2(localVelocity.x, localVelocity.z);
        localMovement = localMovement.normalized;
        localMovement *= Mathf.Clamp(velocity.magnitude / 1, 0f, 1f);
        _SmoothedParameters.TargetValue = localMovement;
    }

    public void Step()
    {
        footSound.PlayStepSound();
    }
}