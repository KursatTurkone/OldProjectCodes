using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.FirstRunner.Scripts.Envoirement;
using Unity.VisualScripting;
using UnityEngine;

public class BallInteract : InteractableBase
{
    [SerializeField] private GameObject landingMarker;
    private Rigidbody Rigidbody;
    private PlayerInteractor PlayerInteractor;
    public Rigidbody ballRigidbody;
    private Vector3 landingPosition;
    public bool IsActive = true;
    public static event System.Action FailMultiply;
 
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        GameController.Instance.BallInteract = this;
        UIControllerBase.Instance.OnActionButtonEvent += ButtonPressed;
    }

    private void OnDisable()
    {
        UIControllerBase.Instance.OnActionButtonEvent -= ButtonPressed;
    }

    void ButtonPressed()
    {
        if (PlayerInteractor != null)
            PlayerInteractor.InteractStarted(this);
    }

    public override void Interact()
    {
     
    }

    public override bool TryToInteract(PlayerInteractBase playerInteractor)
    {
        playerInteractor.InteractStarted(this);
        return true;
    }


    public override void InteractEnded()
    {
        PlayerInteractor = null;
    }

    public override Rigidbody GetRigidbody()
    {
        return Rigidbody;
    }

    public override void SetInteractor(PlayerInteractor playerInteractor)
    {
        this.PlayerInteractor = playerInteractor;
    }


    void Update()
    {
        if (!IsActive)
        {
            if (landingMarker != null && landingMarker.activeSelf)
                landingMarker.SetActive(false);
            return;
        }
        if (ballRigidbody.linearVelocity.y > 3)
        {
            if (landingMarker != null && !landingMarker.activeSelf)
                landingMarker.SetActive(true);
            PredictLandingPosition();
            UpdateLandingMarkerPosition();
        }
    }

    void PredictLandingPosition()
    {
        Vector3 initialPosition = ballRigidbody.position;
        Vector3 initialVelocity = ballRigidbody.linearVelocity;
        float gravity = Physics.gravity.y;
        

        float y0 = initialPosition.y;
        float vy = initialVelocity.y;

        float a = 0.5f * gravity;
        float b = vy;
        float c = y0;

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return;
        }
        
        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDiscriminant) / (2 * a);
        float t2 = (-b - sqrtDiscriminant) / (2 * a);
        float landingTime = Mathf.Max(t1, t2);

        if (landingTime < 0)
        {
            return;
        }
        Vector3 horizontalVelocity = new Vector3(initialVelocity.x, 0, initialVelocity.z);
        Vector3 horizontalDisplacement = horizontalVelocity * landingTime;
        landingPosition = initialPosition + horizontalDisplacement;
        landingPosition.y = 0;
    }


    void UpdateLandingMarkerPosition()
    {
        if (landingMarker != null)
        {
            landingMarker.transform.position = landingPosition;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!IsActive)
            return;
        if (other.gameObject.tag == "Floor")
        {
            FailMultiply?.Invoke();
            IsActive = false;
        }
    }
    
    
}