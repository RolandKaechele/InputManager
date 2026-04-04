#if INPUTMANAGER_DM
using UnityEngine;
using DialogueManager.Runtime;

namespace InputManager.Runtime
{
    /// <summary>
    /// Optional bridge between InputManager and DialogueManager.
    /// Enable define <c>INPUTMANAGER_DM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Pushes the <c>"dialogue"</c> profile when dialogue starts (disabling movement)
    /// and pops it on completion.
    /// </para>
    /// </summary>
    [AddComponentMenu("InputManager/Dialogue Manager Bridge")]
    [DisallowMultipleComponent]
    public class DialogueManagerBridge : MonoBehaviour
    {
        [Tooltip("Profile id to push while dialogue is active.")]
        [SerializeField] private string dialogueProfileId = "dialogue";

        private InputManager _input;
        private DialogueManager.Runtime.DialogueManager _dm;

        private void Awake()
        {
            _input = GetComponent<InputManager>() ?? FindFirstObjectByType<InputManager>();
            _dm    = GetComponent<DialogueManager.Runtime.DialogueManager>()
                     ?? FindFirstObjectByType<DialogueManager.Runtime.DialogueManager>();

            if (_input == null) Debug.LogWarning("[InputManager/DialogueManagerBridge] InputManager not found.");
            if (_dm    == null) Debug.LogWarning("[InputManager/DialogueManagerBridge] DialogueManager not found.");
        }

        private void OnEnable()
        {
            if (_dm != null)
            {
                _dm.OnDialogueStarted   += OnDialogueStarted;
                _dm.OnDialogueCompleted += OnDialogueCompleted;
            }
        }

        private void OnDisable()
        {
            if (_dm != null)
            {
                _dm.OnDialogueStarted   -= OnDialogueStarted;
                _dm.OnDialogueCompleted -= OnDialogueCompleted;
            }
        }

        private void OnDialogueStarted(string id)   => _input?.PushProfile(dialogueProfileId);
        private void OnDialogueCompleted(string id) => _input?.PopProfile();
    }
}
#else
namespace InputManager.Runtime
{
    /// <summary>No-op stub — enable define <c>INPUTMANAGER_DM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("InputManager/Dialogue Manager Bridge")]
    public class DialogueManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[InputManager/DialogueManagerBridge] Bridge disabled — add INPUTMANAGER_DM to Scripting Define Symbols.");
    }
}
#endif
