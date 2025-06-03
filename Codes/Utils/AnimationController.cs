using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [FoldoutGroup("IdleAnimations")] [SerializeField]
    private bool ActivateIdle;

    [FoldoutGroup("IdleAnimations")] [Required("Animation Required")] [ShowIf("ActivateIdle")] [SerializeField]
    private AnimationClip IdleAnimation;

    [FoldoutGroup("WalkAnimation")] [SerializeField]
    private bool ActivateWalk;

    [FoldoutGroup("WalkAnimation")] [Required("Animation Required")] [ShowIf("ActivateWalk")] [SerializeField]
    private AnimationClip WalkAnimation;

    private AnimancerComponent animancer;

    private void Awake()
    {
        transform.TryGetComponent(out animancer);
    }

    public void StartIdle()
    {
        if (ActivateIdle)
            animancer.Play(IdleAnimation, .5f);
    }

    public void StartWalk()
    {
        if (ActivateWalk)
            animancer.Play(WalkAnimation, .5f);
    }

    public void StartWalk(float walkSpeed)
    {
        if (!ActivateWalk)
            return;

        var animation = animancer.Play(IdleAnimation);
        animation.Speed = walkSpeed; 
    }

    private void OnDestroy()
    {
        IdleAnimation = null;
        WalkAnimation = null;
        animancer = null;
    }
}