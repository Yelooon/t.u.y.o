using UnityEngine;

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraView
    {
        Forward,
        Left,
        Right,
        Back,
        Down
    }

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Camera Points")]
    [SerializeField] private Transform forwardPoint;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private Transform backPoint;
    [SerializeField] private Transform downPoint;

    [Header("Movement")]
    [SerializeField] private float transitionSpeed = 8f;

    private CameraView currentView;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    public CameraView CurrentView => currentView;

    private void Start()
    {
        SetView(CameraView.Forward, true);
    }

    private void Update()
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        playerCamera.transform.position = Vector3.Lerp(
            playerCamera.transform.position,
            targetPosition,
            transitionSpeed * Time.deltaTime
        );

        playerCamera.transform.rotation = Quaternion.Slerp(
            playerCamera.transform.rotation,
            targetRotation,
            transitionSpeed * Time.deltaTime
        );
    }

    public void LookLeft()
    {
        switch (currentView)
        {
            case CameraView.Forward:
                SetView(CameraView.Left);
                break;

            case CameraView.Left:
                SetView(CameraView.Back);
                break;

            case CameraView.Right:
                SetView(CameraView.Forward);
                break;

            case CameraView.Back:
                SetView(CameraView.Right);
                break;

            case CameraView.Down:
                SetView(CameraView.Forward);
                break;
        }
    }

    public void LookRight()
    {
        switch (currentView)
        {
            case CameraView.Forward:
                SetView(CameraView.Right);
                break;

            case CameraView.Right:
                SetView(CameraView.Back);
                break;

            case CameraView.Left:
                SetView(CameraView.Forward);
                break;

            case CameraView.Back:
                SetView(CameraView.Left);
                break;

            case CameraView.Down:
                SetView(CameraView.Forward);
                break;
        }
    }

    public void LookDown()
    {
        SetView(CameraView.Down);
    }

    private void SetView(CameraView newView, bool instant = false)
    {
        currentView = newView;

        Transform targetPoint = GetPointForView(newView);

        targetPosition = targetPoint.position;
        targetRotation = targetPoint.rotation;

        if (instant)
        {
            playerCamera.transform.position = targetPosition;
            playerCamera.transform.rotation = targetRotation;
        }
    }

    private Transform GetPointForView(CameraView view)
    {
        switch (view)
        {
            case CameraView.Forward:
                return forwardPoint;

            case CameraView.Left:
                return leftPoint;

            case CameraView.Right:
                return rightPoint;

            case CameraView.Back:
                return backPoint;

            case CameraView.Down:
                return downPoint;

            default:
                return forwardPoint;
        }
    }
}