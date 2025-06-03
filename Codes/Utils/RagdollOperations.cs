using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RagdollOperations : MonoBehaviour
{
    [SerializeField] private List<Transform> BodyParts;
    [SerializeField] private List<Collider> BodyPartColliders;
    [SerializeField] private List<Rigidbody> BodyPartRigs; 
    [SerializeField] private bool HaveMainCollider;

    [SerializeField, EnableIf("HaveMainCollider")]
    private GameObject MainObject;

    [Button]
    private void FindAllJoints()
    {
        var joint = transform.GetComponentsInChildren<Collider>();
        foreach (Collider characterJoint in joint)
        {
            BodyParts.Add(characterJoint.transform);
        }
    }
    [Button]
    private void FindAllColliders(){
    
        var joint = transform.GetComponentsInChildren<Collider>();
        foreach (Collider characterJoint in joint)
        {
            BodyPartColliders.Add(characterJoint);
        }
    }
    [Button]
    private void FindAllRigs(){
    
        var joint = transform.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody characterJoint in joint)
        {
            BodyPartRigs.Add(characterJoint);
        }
    }
    [Button]
    public void ChangeColliderState(bool enable)
    {
        foreach (var collider in BodyPartColliders)
        {
            collider.enabled = enable; 
        }
    }

    [Button]
    public void SetLayer()
    {
        foreach (var part in BodyPartColliders)
        {
            part.gameObject.layer = 8; 
        }
    }
    [Button]
    public void DoRagdoll(bool isRagdoll)
    {
        foreach (Transform part in BodyParts)
        {
            part.TryGetComponent<Collider>(out var col);
            if (col)
            {
                col.enabled = isRagdoll;
            }

            part.TryGetComponent<Rigidbody>(out var rig);
            if (rig)
            {
                rig.useGravity = isRagdoll;
            }
        }

        if (HaveMainCollider)
        {
            MainObject.GetComponent<Collider>().enabled = !isRagdoll;
            MainObject.GetComponent<Rigidbody>().useGravity = !isRagdoll;
        }

        GetComponent<Animator>().enabled = !isRagdoll;
    }

    public void MoveForce(float force)
    {
        foreach (var rig in BodyPartRigs)
        {
            if (rig != null)
            {
                rig.velocity = Vector3.forward*force;
            }
        }
    }
}