using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float groundDrag = 5f;
        public float jumpForce = 5f;
        public float jumpCooldown = 0.25f;
        public float airMultiplier = 0.4f;
        
        [Header("Ground Check")]
        public float playerHeight = 2f;
        public LayerMask whatIsGround;
        public float groundCheckRadius = 0.2f;

        [Header("Slope Handling")]
        public float maxSlopeAngle = 45f;
        private RaycastHit _slopeHit;

        private bool _isGrounded;
        private bool _canJump = true;
        private Vector2 _moveInput;
        private Vector3 _moveDirection;
        private Rigidbody _rb;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _jumpAction;

        private void Awake()
        {
            _playerInput = new PlayerInput();
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
        }

        private void OnEnable()
        {
            _moveAction = _playerInput.Player.Move;
            _moveAction.Enable();

            _jumpAction = _playerInput.Player.Jump;
            _jumpAction.Enable();
            // Remove this line:
            // _jumpAction.performed += OnJump;
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _jumpAction.Disable();
            // Remove this line:
            // _jumpAction.performed -= OnJump;
        }

        private void Update()
        {
            CheckGrounded();
            MyInput();
            SpeedControl();

            // Handle drag
            _rb.drag = _isGrounded ? groundDrag : 0f;
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void MyInput()
        {
            _moveInput = _moveAction.ReadValue<Vector2>();
        }

        private void CheckGrounded()
        {
            // Cast a ray down from the center of the player
            Ray ray = new Ray(transform.position, Vector3.down);
            _isGrounded = Physics.SphereCast(ray, groundCheckRadius, playerHeight * 0.5f + 0.2f, whatIsGround);
        }

        private void MovePlayer()
        {
            // Calculate movement direction
            _moveDirection = transform.forward * -_moveInput.y + transform.right * -_moveInput.x;

            // Check if we're on a slope
            if (_isGrounded && Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * 0.5f + 0.3f, whatIsGround))
            {
                float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                
                if (angle < maxSlopeAngle)
                {
                    // Project movement direction onto slope
                    Vector3 slopeDirection = Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal);
                    _moveDirection = slopeDirection;
                }
            }

            if (_isGrounded)
            {
                _rb.AddForce(_moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
            }
            else
            {
                _rb.AddForce(_moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
            }
        }

        private void SpeedControl()
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
            }
        }

        public void OnJump()
        {
            if (_isGrounded && _canJump)
            {
                _canJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }

        private void Jump()
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void ResetJump()
        {
            _canJump = true;
        }
    }
}