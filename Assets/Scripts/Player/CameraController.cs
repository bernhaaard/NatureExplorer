using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Look Sensitivity")]
        public float sensitivityX = 0.1f;
        public float sensitivityY = 0.1f;
    
        [Header("Clamp Angles")]
        public float minimumY = -90f;
        public float maximumY = 90f;

        public Transform playerBody;

        private float _mouseX, _mouseY;
        private float _xRotation;

        private PlayerInput _playerInput;
        private InputAction _lookAction;

        private void Awake()
        {
            _playerInput = new PlayerInput();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            _lookAction = _playerInput.Player.Look;
            _lookAction.Enable();
        }

        private void OnDisable()
        {
            _lookAction.Disable();
        }

        private void Update()
        {
            Vector2 lookInput = _lookAction.ReadValue<Vector2>();
            
            _mouseX = lookInput.x * sensitivityX;
            _mouseY = lookInput.y * sensitivityY;

            _xRotation -= _mouseY;
            _xRotation = Mathf.Clamp(_xRotation, minimumY, maximumY);

            // Apply vertical rotation to the camera only
            transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            
            // Apply horizontal rotation to the player body
            playerBody.Rotate(Vector3.up * _mouseX);

            // Debug.Log($"Mouse X: {_mouseX}, Mouse Y: {_mouseY}, X Rotation: {_xRotation}");
        }
    }
}