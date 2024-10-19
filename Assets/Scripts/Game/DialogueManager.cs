using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game
{
    [Serializable]
    public class DialogueNode
    {
        public string id;
        public string text;
        public string nextId;
        public List<DialogueResponse> responses;
    }

    [Serializable]
    public class DialogueResponse
    {
        public string text;
        public string nextId;
    }

    public class DialogueManager : MonoBehaviour
    {
        public TextAsset dialogueJSON;
        public TextMeshProUGUI dialogueText;
        public GameObject buttonPrefab;
        public RectTransform buttonContainer;
        public GameObject dialoguePanel;
        public Camera mainCamera;
        public string startingNodeId = "intro_1";

        private Dictionary<string, DialogueNode> _dialogueNodes;
        private DialogueNode _currentNode;
        private readonly List<GameObject> _currentButtons = new List<GameObject>();
        private bool _isDialogueActive = false;

        [Header("Styling")]
        public Color dialogueTextColor = Color.white;
        public Color buttonTextColor = Color.white;
        public Color buttonBackgroundColor = new Color(0.2f, 0.2f, 0.2f);

        void Start()
        {
            LoadDialogue();
            StyleDialogueText();
            HideDialogueUI();
        }

        void LoadDialogue()
        {
            _dialogueNodes = new Dictionary<string, DialogueNode>();
            List<DialogueNode> nodes = JsonConvert.DeserializeObject<List<DialogueNode>>(dialogueJSON.text);
            foreach (var node in nodes)
            {
                _dialogueNodes[node.id] = node;
            }
            Debug.Log($"Loaded {_dialogueNodes.Count} dialogue nodes");
        }

        void StyleDialogueText()
        {
            dialogueText.color = dialogueTextColor;
            dialogueText.fontSize = 48; // Increased from 24 to 48
            dialogueText.alignment = TextAlignmentOptions.Center;
        }

        public void BeginDialogue(string startNodeId)
        {
            if (!_dialogueNodes.ContainsKey(startNodeId))
            {
                Debug.LogError($"Dialogue node with ID '{startNodeId}' not found!");
                return;
            }
            Debug.Log($"Beginning dialogue with node: {startNodeId}");
            ShowDialogueUI();
            StartDialogue(startNodeId);
        }

        public void StartDialogue(string nodeId)
        {
            if (_dialogueNodes.TryGetValue(nodeId, out DialogueNode node))
            {
                _isDialogueActive = true;
                ShowDialogueUI();
                DisableCameraMovement();
                DisplayNode(node);
            }
            else
            {
                Debug.LogError($"Failed to start dialogue. Node with ID '{nodeId}' not found.");
            }
        }

        void DisplayNode(DialogueNode node)
        {
            Debug.Log($"Displaying node: {node.id}");
            _currentNode = node;
            dialogueText.text = node.text;

            ClearCurrentButtons();

            if (node.responses != null && node.responses.Count > 0)
            {
                ShowButtons();
                for (int i = 0; i < node.responses.Count; i++)
                {
                    CreateButton(node.responses[i], i);
                }
            }
            else
            {
                HideButtons();
                Invoke(nameof(ProgressToNextNode), 2f);
            }
        }

        void CreateButton(DialogueResponse response, int index)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            buttonText.text = response.text;
            buttonText.color = buttonTextColor;
            buttonText.fontSize = 36; // Increased from 18 to 36

            button.onClick.AddListener(() => OnResponseClick(index));

            ColorBlock colors = button.colors;
            colors.normalColor = buttonBackgroundColor;
            colors.highlightedColor = Color.Lerp(buttonBackgroundColor, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(buttonBackgroundColor, Color.black, 0.2f);
            button.colors = colors;

            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonObj.GetComponent<RectTransform>());

            _currentButtons.Add(buttonObj);
        }

        void ClearCurrentButtons()
        {
            foreach (GameObject button in _currentButtons)
            {
                Destroy(button);
            }
            _currentButtons.Clear();
        }

        void OnResponseClick(int index)
        {
            string nextId = _currentNode.responses[index].nextId;
            Debug.Log($"Response clicked. Next node: {nextId}");
            if (nextId == "end")
            {
                EndDialogue();
            }
            else
            {
                StartDialogue(nextId);
            }
        }

        void ProgressToNextNode()
        {
            if (_currentNode.nextId == "end")
            {
                EndDialogue();
            }
            else
            {
                StartDialogue(_currentNode.nextId);
            }
        }

        void EndDialogue()
        {
            _isDialogueActive = false;
            HideDialogueUI();
            EnableCameraMovement();
            Debug.Log("Dialogue ended");
        }

        void ShowDialogueUI()
        {
            dialoguePanel.SetActive(true);
            Debug.Log("Dialogue UI shown");
        }

        void HideDialogueUI()
        {
            dialoguePanel.SetActive(false);
            Debug.Log("Dialogue UI hidden");
        }

        void ShowButtons()
        {
            buttonContainer.gameObject.SetActive(true);
        }

        void HideButtons()
        {
            buttonContainer.gameObject.SetActive(false);
        }

        void DisableCameraMovement()
        {
            if (mainCamera != null && mainCamera.GetComponent<CameraController>() != null)
            {
                mainCamera.GetComponent<CameraController>().enabled = false;
                Debug.Log("Camera movement disabled");
            }
        }

        void EnableCameraMovement()
        {
            if (mainCamera != null && mainCamera.GetComponent<CameraController>() != null)
            {
                mainCamera.GetComponent<CameraController>().enabled = true;
                Debug.Log("Camera movement enabled");
            }
        }

        void Update()
        {
            // Check for Q key press to start dialogue
            if (Input.GetKeyDown(KeyCode.Q) && !_isDialogueActive)
            {
                Debug.Log("Q key pressed. Starting dialogue.");
                BeginDialogue(startingNodeId);
            }

            // Handle mouse clicks
            if (_isDialogueActive && Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }

        void HandleMouseClick()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // The click is over a UI element, let the button handle it
                Debug.Log("Click detected over UI");
            }
            else if (_currentButtons.Count == 0)
            {
                // No buttons are present, progress to the next node
                ProgressToNextNode();
            }
            else
            {
                Debug.Log("Click detected, but not over a button");
            }
        }
    }
}