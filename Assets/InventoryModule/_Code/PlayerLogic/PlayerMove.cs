using UnityEngine;
namespace InventoryModule.Examples
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMove : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float acceleration = 10f; // how fast we lerp toward target speed

        [Header("Jump / Gravity")]
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedStickForce = -2f; // small downward force to keep grounded checks stable

        [Header("Look")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private bool lockCursor = true;

        private CharacterController _controller;
        private Vector3 _velocity;      // includes gravity/jump, separate from horizontal move
        private float _currentSpeed;
        private float _cameraPitch;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
        }

        private void HandleLook()
        {
            if (cameraTransform == null) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _cameraPitch -= mouseY;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -85f, 85f);

            cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleMovement()
        {
            bool isGrounded = _controller.isGrounded;

            if (isGrounded && _velocity.y < 0f)
            {
                _velocity.y = groundedStickForce;
            }

            // Input axes
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");

            Vector3 inputDir = (transform.right * inputX + (transform.forward * inputZ)).normalized;
            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            targetSpeed = inputDir.sqrMagnitude > 0.01f ? targetSpeed : 0f;

            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            Vector3 horizontalMove = inputDir * _currentSpeed;

            // Jump
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Gravity
            _velocity.y += gravity * Time.deltaTime;

            Vector3 finalMove = horizontalMove + Vector3.up * _velocity.y;
            _controller.Move(finalMove * Time.deltaTime);
        }
    }
}