#if INPUTMANAGER_EM
using UnityEngine;
using EventManager.Runtime;

namespace InputManager.Runtime
{
    /// <summary>
    /// Optional bridge between InputManager and EventManager.
    /// Enable define <c>INPUTMANAGER_EM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named <see cref="GameEvent"/>s:
    /// <list type="bullet">
    ///   <item><c>"input.profileChanged"</c> — <see cref="GameEvent.stringValue"/> = new profile id</item>
    ///   <item><c>"input.blocked"</c>  — fired when all input is blocked</item>
    ///   <item><c>"input.unblocked"</c> — fired when input is restored</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("InputManager/Event Manager Bridge")]
    [DisallowMultipleComponent]
    public class EventManagerBridge : MonoBehaviour
    {
        [Tooltip("Event name fired on profile change.")]
        [SerializeField] private string profileChangedEventName = "input.profileChanged";

        [Tooltip("Event name fired when all input is blocked.")]
        [SerializeField] private string blockedEventName = "input.blocked";

        [Tooltip("Event name fired when input is unblocked.")]
        [SerializeField] private string unblockedEventName = "input.unblocked";

        private EventManager.Runtime.EventManager _events;
        private InputManager _input;

        private void Awake()
        {
            _events = GetComponent<EventManager.Runtime.EventManager>()
                      ?? FindFirstObjectByType<EventManager.Runtime.EventManager>();
            _input  = GetComponent<InputManager>() ?? FindFirstObjectByType<InputManager>();

            if (_events == null) Debug.LogWarning("[InputManager/EventManagerBridge] EventManager not found.");
            if (_input  == null) Debug.LogWarning("[InputManager/EventManagerBridge] InputManager not found.");
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.OnProfileChanged += OnProfileChanged;
                _input.OnInputBlocked   += OnInputBlocked;
                _input.OnInputUnblocked += OnInputUnblocked;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnProfileChanged -= OnProfileChanged;
                _input.OnInputBlocked   -= OnInputBlocked;
                _input.OnInputUnblocked -= OnInputUnblocked;
            }
        }

        private void OnProfileChanged(string id)  => _events?.Fire(new GameEvent(profileChangedEventName, id));
        private void OnInputBlocked()              => _events?.Fire(new GameEvent(blockedEventName));
        private void OnInputUnblocked()            => _events?.Fire(new GameEvent(unblockedEventName));
    }
}
#else
namespace InputManager.Runtime
{
    /// <summary>No-op stub — enable define <c>INPUTMANAGER_EM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("InputManager/Event Manager Bridge")]
    public class EventManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[InputManager/EventManagerBridge] Bridge disabled — add INPUTMANAGER_EM to Scripting Define Symbols.");
    }
}
#endif
