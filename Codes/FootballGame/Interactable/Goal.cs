using System;
using UnityEngine;

public class Goal : MonoBehaviour
{
   public Transform[] GoalPoints;

   private void Start()
   {
      GameController.Instance.Goal = this; 
   }
}
