using UnityEngine;
using UnityEngine.InputSystem;

public class RTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float edgeScrollSpeed = 10f;
    [SerializeField] private float edgeSize = 25f;

    [Header("Keyboard Movement")]
    [SerializeField] private bool useKeyboardMove = true;

    [Header("Edge Scroll")]
    [SerializeField] private bool useEdgeScroll = true;

    [Header("Camera Bounds")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minZ = -6f;
    [SerializeField] private float maxZ = 6f;

    [Header("Fixed Height")]
    [SerializeField] private bool lockYPosition = true;
    [SerializeField] private float fixedY = 10f;

    [Header("Debug")]
    [SerializeField] private Vector3 moveInput;

    private void Start()
    {
        if (lockYPosition)
        {
            fixedY = transform.position.y;
        }

        ClampCameraPosition();
    }

    private void Update()
    {
        moveInput = Vector3.zero;

        if (useKeyboardMove)
        {
            HandleKeyboardMove();
        }

        if (useEdgeScroll)
        {
            HandleEdgeScroll();
        }

        MoveCamera();
    }

    private void HandleKeyboardMove()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            moveInput.z += 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            moveInput.z -= 1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            moveInput.x += 1f;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            moveInput.x -= 1f;
        }
    }

    private void HandleEdgeScroll()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (mousePosition.x <= edgeSize)
        {
            moveInput.x -= edgeScrollSpeed / moveSpeed;
        }

        if (mousePosition.x >= Screen.width - edgeSize)
        {
            moveInput.x += edgeScrollSpeed / moveSpeed;
        }

        if (mousePosition.y <= edgeSize)
        {
            moveInput.z -= edgeScrollSpeed / moveSpeed;
        }

        if (mousePosition.y >= Screen.height - edgeSize)
        {
            moveInput.z += edgeScrollSpeed / moveSpeed;
        }
    }

    private void MoveCamera()
    {
        if (moveInput == Vector3.zero)
        {
            return;
        }

        moveInput = moveInput.normalized;

        Vector3 moveAmount = moveInput * moveSpeed * Time.deltaTime;

        transform.position += moveAmount;

        ClampCameraPosition();
    }

    private void ClampCameraPosition()
    {
        Vector3 position = transform.position;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);

        if (lockYPosition)
        {
            position.y = fixedY;
        }

        transform.position = position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 center = new Vector3(
            (minX + maxX) * 0.5f,
            transform.position.y,
            (minZ + maxZ) * 0.5f
        );

        Vector3 size = new Vector3(
            maxX - minX,
            0.1f,
            maxZ - minZ
        );

        Gizmos.DrawWireCube(center, size);
    }
}