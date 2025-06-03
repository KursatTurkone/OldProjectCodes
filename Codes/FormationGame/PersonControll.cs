using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using UnityEngine;

public class PersonControll : MonoBehaviour
{
    private LevelManager _levelManager;
      private bool _charactersLevelActive;
      [SerializeField] private PersonInfo PersonInfo;
      [SerializeField] private Material MaterialOfCollected;
      [SerializeField] private Material MaterialOfUnCollected; 
      [SerializeField] private GameObject MeshOfCharacter; 

      private void Start()
    {
        _levelManager = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>(); 
     
    }

      public void Collected(bool collected)
      {
          if (collected)
          {
              MeshOfCharacter.GetComponent<SkinnedMeshRenderer>().material = MaterialOfCollected; 
          }
          else
          {
              MeshOfCharacter.GetComponent<SkinnedMeshRenderer>().material = MaterialOfUnCollected; 
          }
      }
    public void Died()
    {
        MMVibrationManager.Haptic(HapticTypes.MediumImpact);
        Destroy(transform.GetComponent<CollectablePerson>()); 
        var ragdoll = transform.GetComponent<RagdollOperations>();
        ragdoll.ChangeColliderState(true);
        ragdoll.DoRagdoll(true);
       DOVirtual.DelayedCall(1.5f,()=>ragdoll.SetLayer() ) ;
    }
    
}
