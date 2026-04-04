#if INPUTMANAGER_CSM
using UnityEngine;
using CutsceneManager.Runtime;

namespace InputManager.Runtime
{
    /// <summary>
    /// Optional bridge between InputManager and CutsceneManager.
    /// Enable define <c>INPUTMANAGER_CSM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Pushes the <c>"blocked"</c> profile when a cutscene starts so the player cannot move,
    /// and pops it on completion or skip.
    /// </para>
    /// </summary>
    [AddComponentMenu("InputManager/Cutscene Manager Bridge")]
    [DisallowMultipleComponent]
    public class CutsceneManagerBridge : MonoBehaviour
    {
        [Tooltip("Profile id to push while a cutscene is playing.")]
        [SerializeField] private string blockedProfileId = "blocked";

        private InputManager _input;
        private CutsceneManager.Runtime.CutsceneManager _csm;

        private void Awake()
        {
            _input = GetComponent<InputManager>() ?? FindFirstObjectByType<InputManager>();
            _csm   = GetComponent<CutsceneManager.Runtime.CutsceneManager>()
                     ?? FindFirstObjectByType<CutsceneManager.Runtime.CutsceneManager>();

            if (_input == null) Debug.LogWarning("[InputManager/CutsceneManagerBridge] InputManager not found.");
            if (_csm   == null) Debug.LogWarning("[InputManager/CutsceneManagerBridge] CutsceneManager not found.");
        }

        private void OnEnable()
        {
            if (_csm != null)
            {
                _csm.OnSequenceStarted   += OnSequenceStarted;
                _csm.OnSequenceCompleted += OnSequenceEnded;
                _csm.OnSequenceSkipped   += OnSequenceEnded;
            }
        }

        private void OnDisable()
        {
            if (_csm != null)
            {
                _csm.OnSequenceStarted   -= OnSequenceStarted;
                _csm.OnSequenceCompleted -= OnSequenceEnded;
                _csm.OnSequenceSkipped   -= OnSequenceEnded;
            }
        }

        private void OnSequenceStarted(string sequenceId) => _input?.PushProfile(blockedProfileId);
        private void OnSequenceEnded(string sequenceId)   => _input?.PopProfile();
    }
}
#else
namespace InputManager.Runtime
{
    /// <summary>No-op stub — enable define <c>INPUTMANAGER_CSM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("InputManager/Cutscene Manager Bridge")]
    public class CutsceneManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[InputManager/CutsceneManagerBridge] Bridge disabled — add INPUTMANAGER_CSM to Scripting Define Symbols.");
    }
}
#endif
