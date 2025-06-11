using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject model;
    [SerializeField] private RagdollOperations ragdollOperations;

    [Header("Follow Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isActiveOnStart = true;

    private bool isFollowing;
    private float initialY;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        initialY = transform.position.y;

        if (isActiveOnStart)
            SetFollowState(true);
    }

    private void Update()
    {
        if (!isFollowing)
            return;

        if (Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Vector3 newPosition = hit.point;
                newPosition.y = initialY;
                transform.position = newPosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            SetFollowState(false);
        }
    }

    public void SetFollowState(bool follow)
    {
        isFollowing = follow;
        GetComponent<Collider>().enabled = !follow;

        if (!follow)
        {
            Vector3 snappedPos = transform.position;
            snappedPos.y = initialY;
            transform.position = snappedPos;
        }

        ragdollOperations.DoRagdoll(follow);
    }

    private void OnMouseDown()
    {
        SetFollowState(true);
    }
}