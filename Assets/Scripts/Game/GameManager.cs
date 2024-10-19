using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Player;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI endGameText;
        public GameObject endGamePanel;
        public Button restartButton;
        public QuestGiver questGiver;

        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private CameraController cameraController;

        private int _score = 0;
        private float _timeRemaining = 90f;
        private bool _isGameActive = false;
        private bool _isFirstStart = true;
        private const int TargetScore = 10;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            UpdateScoreUI();
            endGamePanel.SetActive(false);
            restartButton.onClick.AddListener(RestartGame);

            if (_isFirstStart)
            {
                questGiver.ShowInitialMessage();
            }
            else
            {
                StartGame();
            }
        }

        private void Update()
        {
            if (_isGameActive)
            {
                if (_timeRemaining > 0)
                {
                    _timeRemaining -= Time.deltaTime;
                    UpdateTimerUI();
                }
                else
                {
                    EndGame(false);
                }
            }
        }

        public void StartGame()
        {
            _isGameActive = true;
            _timeRemaining = 90f;
            _score = 0;
            UpdateScoreUI();
            UpdateTimerUI();
            LockCursor();
        }

        public void CollectiblePickup()
        {
            _score++;
            UpdateScoreUI();

            if (_score >= TargetScore)
            {
                EndGame(true);
            }
        }

        private void UpdateScoreUI()
        {
            scoreText.text = "Score: " + _score + " / " + TargetScore;
        }

        private void UpdateTimerUI()
        {
            int minutes = Mathf.FloorToInt(_timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(_timeRemaining % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private void EndGame(bool isWin)
        {
            _isGameActive = false;
            endGamePanel.SetActive(true);
            
            if (isWin)
            {
                endGameText.text = "You Win!\nTime: " + (90f - _timeRemaining).ToString("F2") + " seconds";
            }
            else
            {
                endGameText.text = "Time's Up!\nScore: " + _score + " / " + TargetScore;
            }

            restartButton.gameObject.SetActive(true);
            UnlockCursor();
        }

        public void RestartGame()
        {
            _isFirstStart = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (playerMovement != null) playerMovement.enabled = true;
            if (cameraController != null) cameraController.enabled = true;
        }

        private void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (playerMovement != null) playerMovement.enabled = false;
            if (cameraController != null) cameraController.enabled = false;
        }
    }
}