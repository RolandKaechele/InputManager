#if INPUTMANAGER_STM
using System.Collections.Generic;
using UnityEngine;
using StateManager.Runtime;

namespace InputManager.Runtime
{
    /// <summary>
    /// Optional bridge between InputManager and StateManager.
    /// Enable define <c>INPUTMANAGER_STM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Maps each <see cref="AppState"/> to an input profile id and calls
    /// <see cref="InputManager.SetProfile(string)"/> when the state changes.
    /// </para>
    /// </summary>
    [AddComponentMenu("InputManager/State Manager Bridge")]
    [DisallowMultipleComponent]
    public class StateManagerBridge : MonoBehaviour
    {
        [System.Serializable]
        public class StateProfileMapping
        {
            [Tooltip("Application state.")]
            public AppState state;

            [Tooltip("Input profile id to activate when this state becomes active.")]
            public string profileId;
        }

        [SerializeField] private List<StateProfileMapping> stateMappings = new List<StateProfileMapping>();

        private InputManager _input;
        private StateManager.Runtime.StateManager _state;

        private void Awake()
        {
            _input = GetComponent<InputManager>() ?? FindFirstObjectByType<InputManager>();
            _state = GetComponent<StateManager.Runtime.StateManager>()
                     ?? FindFirstObjectByType<StateManager.Runtime.StateManager>();

            if (_input == null) Debug.LogWarning("[InputManager/StateManagerBridge] InputManager not found.");
            if (_state == null) Debug.LogWarning("[InputManager/StateManagerBridge] StateManager not found.");
        }

        private void OnEnable()
        {
            if (_state != null) _state.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (_state != null) _state.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(AppState previous, AppState next)
        {
            if (_input == null) return;
            foreach (var mapping in stateMappings)
            {
                if (mapping.state == next && !string.IsNullOrEmpty(mapping.profileId))
                {
                    _input.SetProfile(mapping.profileId);
                    return;
                }
            }
        }
    }
}
#else
namespace InputManager.Runtime
{
    /// <summary>No-op stub — enable define <c>INPUTMANAGER_STM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("InputManager/State Manager Bridge")]
    public class StateManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[InputManager/StateManagerBridge] Bridge disabled — add INPUTMANAGER_STM to Scripting Define Symbols.");
    }
}
#endif
