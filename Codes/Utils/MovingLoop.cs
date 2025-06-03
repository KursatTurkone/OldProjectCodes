using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class MovingLoop : MonoBehaviour
{
    [SerializeField] private Vector3 MoveVector;
    [SerializeField] private float MovingSpeed;
    [SerializeField] private Ease EaseType;
    [SerializeField] private float StartDelay; 
    private float VectorX, VectorY, VectorZ;
    private Tween MovingTween;

    private void Start()
    {
        VectorX = MoveVector.x;
        VectorY = MoveVector.y;
        VectorZ = MoveVector.z;
    }

    [Button]
    public void StartMoving()
    {
        var position = transform.localPosition;
        if (MovingSpeed <= 0)
            return;
        MovingTween.Kill();
        MovingTween = transform
            .DOLocalMove(new Vector3(position.x + VectorX, position.y + VectorY, position.z + VectorZ), MovingSpeed)
            .SetSpeedBased().SetEase(EaseType).OnComplete(() =>
            {
                VectorX = -VectorX;
                VectorY = -VectorY;
                VectorZ = -VectorZ;
                DOVirtual.DelayedCall(StartDelay, StartMoving);
            });
    }

    [Button]
    public void StopMoving()
    {
        MovingTween.Kill();
    }
}