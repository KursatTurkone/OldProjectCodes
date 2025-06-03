using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class Formation : MonoBehaviour
{
    public List<Vector3> Positions = new List<Vector3>();

    [SerializeField] private float CharacterRadius;
    [SerializeField] private GameObject TestObject;
    [SerializeField] private int circlerCharacterAmount;
    private float angleBetweenObjects;
    private int characterCount = 0;
    private float cirlerMotionRadius;

    private void Awake()
    {
        cirlerMotionRadius = CharacterRadius;
    }

    [Button]
    public void IncreaseAmount(int amount)
    {
        if (amount < Positions.Count)
            return;
        Vector3 circlerPosition;

        circlerPosition.x = cirlerMotionRadius * Mathf.Cos(Mathf.Deg2Rad * angleBetweenObjects);
        circlerPosition.z = cirlerMotionRadius * Mathf.Sin(Mathf.Deg2Rad * angleBetweenObjects);
        circlerPosition.y = 0; //fanInstantiateOffsetY;

        float angleAddAmount = 360f / circlerCharacterAmount;

        angleBetweenObjects += angleAddAmount;

        characterCount++;

        if (circlerCharacterAmount == characterCount)
        {
            angleBetweenObjects = 0;
            circlerCharacterAmount *= 2;
            cirlerMotionRadius += CharacterRadius;
            characterCount = 0;
        }

        Positions.Add(circlerPosition);
    }

    [Button]
    public void DecreaseAmount()
    {
        float angleAddAmount = 360f / circlerCharacterAmount;

        angleBetweenObjects -= angleAddAmount;

        if (characterCount == 0)
        {
            angleBetweenObjects = 360;
            circlerCharacterAmount /= 2;
            cirlerMotionRadius -= CharacterRadius;
            characterCount = circlerCharacterAmount;
        }

        Positions.RemoveAt(Positions.Count - 1);
        characterCount--;
    }

    [Button]
    private void TestWright()
    {
        foreach (var VARIABLE in Positions)
        {
            Debug.Log(VARIABLE + " / POS ");
            var obj = Instantiate(TestObject, transform);
            obj.transform.localPosition = VARIABLE;
        }
    }
}