using UnityEngine;

namespace Game
{
    public class UnderwaterEffect : MonoBehaviour
    {
        public Color underwaterColor = new Color(0, 0.4f, 0.7f, 0.3f);
        public GameObject player;
        public Camera playerCamera;
        
        [Header("Fog Settings")]
        public float underwaterFogDensity = 0.25f;
        public float underwaterFogStartDistance = 0f;
        public float underwaterFogEndDistance = 100f;

        [Header("Transition")]
        public float transitionDuration = 1f;

        private Camera _mainCamera;
        private Color _originalColor;
        private float _originalFogDensity;
        private float _originalFogStartDistance;
        private float _originalFogEndDistance;
        
        private float _targetFogDensity;
        private Color _targetColor;
        private float _transitionTimer;
        private bool _isTransitioning;
        private bool _isUnderwater;

        private void Start()
        {
            if (playerCamera == null)
            {
                Debug.LogError("UnderwaterEffect: Player camera not assigned. Please assign it in the inspector.");
            }
            _mainCamera = playerCamera;
            
            // Store original settings
            _originalColor = RenderSettings.fogColor;
            _originalFogDensity = RenderSettings.fogDensity;
            _originalFogStartDistance = RenderSettings.fogStartDistance;
            _originalFogEndDistance = RenderSettings.fogEndDistance;
        }

        private void Update()
        {
            if (_isTransitioning)
            {
                UpdateTransition();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == player)
            {
                StartTransition(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == player)
            {
                StartTransition(false);
            }
        }

        private void StartTransition(bool enteringWater)
        {
            _isTransitioning = true;
            _transitionTimer = 0f;
            _isUnderwater = enteringWater;

            if (enteringWater)
            {
                _targetFogDensity = underwaterFogDensity;
                _targetColor = underwaterColor;
            }
            else
            {
                _targetFogDensity = _originalFogDensity;
                _targetColor = _originalColor;
            }
        }

        private void UpdateTransition()
        {
            _transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_transitionTimer / transitionDuration);

            // Smoothly interpolate fog density
            RenderSettings.fogDensity = Mathf.Lerp(
                _isUnderwater ? _originalFogDensity : underwaterFogDensity,
                _targetFogDensity,
                t
            );

            // Smoothly interpolate colors
            Color currentColor = Color.Lerp(
                _isUnderwater ? _originalColor : underwaterColor,
                _targetColor,
                t
            );
            RenderSettings.fogColor = currentColor;
            _mainCamera.backgroundColor = currentColor;

            // Smoothly interpolate fog distances
            RenderSettings.fogStartDistance = Mathf.Lerp(
                _isUnderwater ? _originalFogStartDistance : underwaterFogStartDistance,
                _isUnderwater ? underwaterFogStartDistance : _originalFogStartDistance,
                t
            );
            RenderSettings.fogEndDistance = Mathf.Lerp(
                _isUnderwater ? _originalFogEndDistance : underwaterFogEndDistance,
                _isUnderwater ? underwaterFogEndDistance : _originalFogEndDistance,
                t
            );

            if (_transitionTimer >= transitionDuration)
            {
                _isTransitioning = false;
            }
        }
    }
}