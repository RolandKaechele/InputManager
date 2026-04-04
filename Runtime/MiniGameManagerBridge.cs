#if INPUTMANAGER_MGM
using UnityEngine;
using MiniGameManager.Runtime;

namespace InputManager.Runtime
{
    /// <summary>
    /// Optional bridge between InputManager and MiniGameManager.
    /// Enable define <c>INPUTMANAGER_MGM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Pushes the <c>"minigame"</c> profile when a mini-game starts and pops it on
    /// completion or abort.
    /// </para>
    /// </summary>
    [AddComponentMenu("InputManager/Mini Game Manager Bridge")]
    [DisallowMultipleComponent]
    public class MiniGameManagerBridge : MonoBehaviour
    {
        [Tooltip("Profile id to push while a mini-game is active.")]
        [SerializeField] private string miniGameProfileId = "minigame";

        private InputManager _input;
        private MiniGameManager.Runtime.MiniGameManager _mgm;

        private void Awake()
        {
            _input = GetComponent<InputManager>() ?? FindFirstObjectByType<InputManager>();
            _mgm   = GetComponent<MiniGameManager.Runtime.MiniGameManager>()
                     ?? FindFirstObjectByType<MiniGameManager.Runtime.MiniGameManager>();

            if (_input == null) Debug.LogWarning("[InputManager/MiniGameManagerBridge] InputManager not found.");
            if (_mgm   == null) Debug.LogWarning("[InputManager/MiniGameManagerBridge] MiniGameManager not found.");
        }

        private void OnEnable()
        {
            if (_mgm != null)
            {
                _mgm.OnMiniGameStarted   += OnMiniGameStarted;
                _mgm.OnMiniGameCompleted += OnMiniGameEnded;
                _mgm.OnMiniGameAborted   += OnMiniGameAborted;
            }
        }

        private void OnDisable()
        {
            if (_mgm != null)
            {
                _mgm.OnMiniGameStarted   -= OnMiniGameStarted;
                _mgm.OnMiniGameCompleted -= OnMiniGameEnded;
                _mgm.OnMiniGameAborted   -= OnMiniGameAborted;
            }
        }

        private void OnMiniGameStarted(string id)           => _input?.PushProfile(miniGameProfileId);
        private void OnMiniGameEnded(MiniGameResult result) => _input?.PopProfile();
        private void OnMiniGameAborted(string id)           => _input?.PopProfile();
    }
}
#else
namespace InputManager.Runtime
{
    /// <summary>No-op stub — enable define <c>INPUTMANAGER_MGM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("InputManager/Mini Game Manager Bridge")]
    public class MiniGameManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[InputManager/MiniGameManagerBridge] Bridge disabled — add INPUTMANAGER_MGM to Scripting Define Symbols.");
    }
}
#endif
