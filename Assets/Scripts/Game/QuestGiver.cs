using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game
{
    public class QuestGiver : MonoBehaviour
    {
        [Serializable]
        private class DialogueResponse
        {
            public string text;
            public string nextId;
        }

        [Serializable]
        private class DialogueEntry
        {
            public string id;
            public string text;
            public string nextId;
            public List<DialogueResponse> responses;
        }

        [SerializeField] private TextAsset jsonFile;
        [SerializeField] private InputActionReference nextDialogueAction;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI questMessageText;
        [SerializeField] private TextMeshProUGUI continuePromptText;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private CameraController cameraController;

        private List<DialogueEntry> _dialogueEntries;
        private DialogueEntry _currentEntry;
        private bool _isDialogueActive;
        private bool _waitingForInput;
        private bool _canProgress = false;

        private const string IntroId = "intro_1";
        private const string ExitId = "exit";

        private void Awake()
        {
            ParseDialogue();
        }

        private void OnEnable()
        {
            nextDialogueAction.action.performed += OnNextDialoguePerformed;

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i;
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
            }
        }

        private void OnDisable()
        {
            nextDialogueAction.action.performed -= OnNextDialoguePerformed;

            foreach (var button in choiceButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        private void Start()
        {
            SetupInitialState();
        }

        private void SetupInitialState()
        {
            dialoguePanel.SetActive(false);
            DisableChoiceButtons();
            continuePromptText.text = "Press Enter to continue";
        }

        private void ParseDialogue()
        {
            if (jsonFile != null)
            {
                string jsonText = jsonFile.text;
                _dialogueEntries = JsonConvert.DeserializeObject<List<DialogueEntry>>(jsonText);
                Debug.Log($"Parsed {_dialogueEntries.Count} dialogue entries.");
            }
            else
            {
                Debug.LogError("JSON file is not assigned!");
            }
        }

        private void OnNextDialoguePerformed(InputAction.CallbackContext context)
        {
            if (_canProgress)
            {
                ProgressDialogue();
            }
        }


        public void ShowInitialMessage()
        {
            ToggleDialogue(true);
        }

        private void ToggleDialogue(bool activate)
        {
            _isDialogueActive = activate;
            dialoguePanel.SetActive(_isDialogueActive);

            if (_isDialogueActive)
            {
                EnableDialogueMode();
            }
            else
            {
                DisableDialogueMode();
            }
        }

        private void EnableDialogueMode()
        {
            SetPlayerControl(false);
            SetCursorState(true, CursorLockMode.None);
            ResetDialogue();
        }

        private void DisableDialogueMode()
        {
            SetPlayerControl(true);
            SetCursorState(false, CursorLockMode.Locked);
            DisableChoiceButtons();
            _currentEntry = null;
            _waitingForInput = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("GameManager.Instance is null in DisableDialogueMode");
            }
        }

        private void SetPlayerControl(bool enable)
        {
            if (playerMovement != null) playerMovement.enabled = enable;
            if (cameraController != null) cameraController.enabled = enable;
        }

        private void SetCursorState(bool visible, CursorLockMode lockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = lockMode;
        }

        private void EnableChoiceButtons()
        {
            continuePromptText.gameObject.SetActive(false);
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < _currentEntry.responses.Count)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = _currentEntry.responses[i].text;
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void DisableChoiceButtons()
        {
            foreach (Button button in choiceButtons)
            {
                button.gameObject.SetActive(false);
            }
            continuePromptText.gameObject.SetActive(true);
        }

        private void OnChoiceSelected(int choiceIndex)
        {
            if (_currentEntry.responses != null && choiceIndex < _currentEntry.responses.Count)
            {
                string nextId = _currentEntry.responses[choiceIndex].nextId;
                Debug.Log($"Choice selected: {choiceIndex}, Next ID: {nextId}");

                if (string.IsNullOrEmpty(nextId) || nextId.Equals(ExitId, StringComparison.OrdinalIgnoreCase))
                {
                    ToggleDialogue(false);
                }
                else
                {
                    ProcessNextDialogueEntry(nextId);
                }
            }
            else
            {
                Debug.LogError("Invalid choice selection");
            }
        }

        private void ProcessNextDialogueEntry(string nextId)
        {
            _currentEntry = _dialogueEntries.Find(entry => entry.id == nextId);
            if (_currentEntry != null)
            {
                Debug.Log($"Next entry found: {_currentEntry.id}");
                UpdateDialogueText();
            }
            else
            {
                Debug.LogError($"No dialogue entry found for ID: {nextId}");
                ToggleDialogue(false);
            }
        }

        private void ResetDialogue()
        {
            _currentEntry = _dialogueEntries.Find(entry => entry.id == IntroId);
            UpdateDialogueText();
        }

        private void UpdateDialogueText()
        {
            if (_currentEntry != null)
            {
                questMessageText.text = _currentEntry.text;
                Debug.Log($"Updating dialogue text: {_currentEntry.text}");

                if (_currentEntry.responses is { Count: > 0 })
                {
                    Debug.Log($"Enabling {_currentEntry.responses.Count} choice buttons");
                    EnableChoiceButtons();
                    _waitingForInput = true;
                    _canProgress = false;
                }
                else
                {
                    Debug.Log("No responses available, waiting for user input to continue");
                    DisableChoiceButtons();
                    _waitingForInput = false;
                    _canProgress = true;
                    continuePromptText.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogError("Current entry is null in UpdateDialogueText");
                ToggleDialogue(false);
            }
        }

        private void ProgressDialogue()
        {
            if (_currentEntry != null)
            {
                if (string.IsNullOrEmpty(_currentEntry.nextId) || _currentEntry.nextId.Equals(ExitId, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log("Reached end of dialogue or exit condition");
                    ToggleDialogue(false);
                }
                else
                {
                    ProcessNextDialogueEntry(_currentEntry.nextId);
                }
            }
            else
            {
                Debug.LogError("Current entry is null in ProgressDialogue");
                ToggleDialogue(false);
            }
        }
    }
}