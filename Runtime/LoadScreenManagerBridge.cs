#if INPUTMANAGER_LSM
using UnityEngine;
using LoadScreenManager.Runtime;

namespace InputManager.Runtime
{
    /// <summary>
    /// Optional bridge between InputManager and LoadScreenManager.
    /// Enable define <c>INPUTMANAGER_LSM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Pushes the <c>"loading"</c> input profile when a load screen appears (blocking all input)
    /// and pops it when the load screen is dismissed.
    /// </para>
    /// </summary>
    [AddComponentMenu("InputManager/Load Screen Manager Bridge")]
    [DisallowMultipleComponent]
    public class LoadScreenManagerBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [Tooltip("Input profile id to push while a loading screen is visible. Should have lockMove and lockAction enabled.")]
        [SerializeField] private string loadingProfileId = "loading";

        // ─── References ───────────────────────────────────────────────────────
        private InputManager _input;
        private LoadScreenManager.Runtime.LoadScreenManager _lsm;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _input = GetComponent<InputManager>() ?? FindFirstObjectByType<InputManager>();
            _lsm   = GetComponent<LoadScreenManager.Runtime.LoadScreenManager>()
                     ?? FindFirstObjectByType<LoadScreenManager.Runtime.LoadScreenManager>();

            if (_input == null) Debug.LogWarning("[InputManager/LoadScreenManagerBridge] InputManager not found.");
            if (_lsm   == null) Debug.LogWarning("[InputManager/LoadScreenManagerBridge] LoadScreenManager not found.");
        }

        private void OnEnable()
        {
            if (_lsm != null)
            {
                _lsm.OnScreenShown  += OnScreenShown;
                _lsm.OnScreenHidden += OnScreenHidden;
            }
        }

        private void OnDisable()
        {
            if (_lsm != null)
            {
                _lsm.OnScreenShown  -= OnScreenShown;
                _lsm.OnScreenHidden -= OnScreenHidden;
            }
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnScreenShown(string _id) => _input?.PushProfile(loadingProfileId);
        private void OnScreenHidden()          => _input?.PopProfile();
    }
}
#else
namespace InputManager.Runtime
{
    /// <summary>No-op stub – enable define <c>INPUTMANAGER_LSM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("InputManager/Load Screen Manager Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class LoadScreenManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
