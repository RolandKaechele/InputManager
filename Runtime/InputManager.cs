using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace InputManager.Runtime
{
    /// <summary>
    /// Centralized input profile manager for Unity.
    /// Manages a stack of <see cref="InputProfile"/> entries to block or remap input in response
    /// to game state changes (cutscenes, dialogues, mini-games, loading).
    /// Supports JSON-driven profiles for modding.
    /// </summary>
    [AddComponentMenu("Managers/Input Manager")]
    [DisallowMultipleComponent]
#if ODIN_INSPECTOR
    public class InputManager : SerializedMonoBehaviour
#else
    public class InputManager : MonoBehaviour
#endif
    {
        // ──────────────────────────────────────────────────────────
        // Inspector fields
        // ──────────────────────────────────────────────────────────

        [Header("Profiles")]
        [Tooltip("Built-in input profiles. JSON entries are merged on top by id.")]
        [SerializeField] private List<InputProfile> profiles = new List<InputProfile>();

        [Tooltip("Profile id to activate on Awake.")]
        [SerializeField] private string initialProfileId = "gameplay";

        [Header("Global Bindings")]
        [Tooltip("Fallback key bindings used when the active profile has no override for an action.")]
        [SerializeField] private List<InputActionBinding> globalBindings = new List<InputActionBinding>();

        [Header("JSON / Modding")]
        [Tooltip("Load additional profiles from StreamingAssets/<jsonPath>.")]
        [SerializeField] private bool loadFromJson;

        [Tooltip("Path relative to StreamingAssets/.")]
        [SerializeField] private string jsonPath = "input_profiles.json";

        [Header("Stack")]
        [Tooltip("Maximum profile stack depth.")]
        [SerializeField] private int maxStackDepth = 8;

        [Header("Debug")]
        [Tooltip("Log all profile transitions to the Console.")]
        [SerializeField] private bool verboseLogging;

#if INPUTMANAGER_REWIRED
        [Header("Rewired")]
        [Tooltip("Rewired-Spieler-Index, dessen Input gelesen wird (Standard: 0).")]
        [SerializeField] private int rewiredPlayerId = 0;
#endif

        // ──────────────────────────────────────────────────────────
        // Events
        // ──────────────────────────────────────────────────────────

        /// <summary>Fired whenever the active input profile changes. Parameter is the new profile id.</summary>
        public event Action<string> OnProfileChanged;

        /// <summary>Fired when all input actions are blocked (lockMove AND lockAction are true on the active profile).</summary>
        public event Action OnInputBlocked;

        /// <summary>Fired when input is unblocked after a blocked state.</summary>
        public event Action OnInputUnblocked;

        // ──────────────────────────────────────────────────────────
        // State
        // ──────────────────────────────────────────────────────────

        private readonly Dictionary<string, InputProfile> _map        = new Dictionary<string, InputProfile>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, KeyCode>      _globalMap  = new Dictionary<string, KeyCode>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<InputProfile>              _stack      = new Stack<InputProfile>();
        private bool _wasBlocked;

#if INPUTMANAGER_REWIRED
        private Rewired.Player _rewiredPlayer;
#endif

        // ──────────────────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────────────────

        /// <summary>Currently active <see cref="InputProfile"/>, or <c>null</c> if the stack is empty.</summary>
        public InputProfile CurrentProfile => _stack.Count > 0 ? _stack.Peek() : null;

        /// <summary>Id of the currently active profile.</summary>
        public string CurrentProfileId => CurrentProfile?.id;

        /// <summary>All registered profiles, keyed by id.</summary>
        public IReadOnlyDictionary<string, InputProfile> Profiles => _map;

        /// <summary>
        /// Replaces the entire profile stack with the given profile id.
        /// </summary>
        public void SetProfile(string id)
        {
            if (!TryGetProfile(id, out var profile)) return;
            _stack.Clear();
            _stack.Push(profile);
            NotifyProfileChanged(profile);
        }

        /// <summary>
        /// Pushes a profile on top of the stack, overlaying the current profile.
        /// </summary>
        public void PushProfile(string id)
        {
            if (!TryGetProfile(id, out var profile)) return;
            if (_stack.Count >= maxStackDepth)
            {
                Debug.LogWarning($"[InputManager] Max stack depth ({maxStackDepth}) reached. Cannot push '{id}'.");
                return;
            }
            _stack.Push(profile);
            NotifyProfileChanged(profile);
        }

        /// <summary>
        /// Pops the top profile off the stack and returns to the previous one.
        /// </summary>
        public void PopProfile()
        {
            if (_stack.Count <= 1)
            {
                Debug.LogWarning("[InputManager] Cannot pop — only one profile on stack.");
                return;
            }
            _stack.Pop();
            NotifyProfileChanged(_stack.Peek());
        }

        /// <summary>
        /// Returns <c>true</c> if the action with <paramref name="actionId"/> is currently pressed.
        /// Checks active profile bindings first, then falls back to global bindings.
        /// </summary>
        public bool IsActionDown(string actionId)
        {
            if (IsFullyBlocked()) return false;

#if INPUTMANAGER_REWIRED
            var rewiredPlayer = GetRewiredPlayer();
            if (rewiredPlayer != null)
                return rewiredPlayer.GetButton(actionId);
#endif

            // Check profile-specific binding
            var profile = CurrentProfile;
            if (profile != null)
            {
                foreach (var b in profile.bindings)
                {
                    if (!b.actionId.Equals(actionId, StringComparison.OrdinalIgnoreCase)) continue;
                    if (b.primaryKey   != KeyCode.None && Input.GetKey(b.primaryKey))   return true;
                    if (b.secondaryKey != KeyCode.None && Input.GetKey(b.secondaryKey)) return true;
                    return false;
                }
            }

            // Fall back to global binding
            if (_globalMap.TryGetValue(actionId, out var key))
                return key != KeyCode.None && Input.GetKey(key);

            return false;
        }

        /// <summary>
        /// Returns the raw axis value for <paramref name="axisId"/> (delegates to Unity's Input system).
        /// Returns 0 when movement is locked.
        /// </summary>
        public float GetAxis(string axisId)
        {
            if (CurrentProfile != null && CurrentProfile.lockMove) return 0f;

#if INPUTMANAGER_REWIRED
            var rewiredPlayer = GetRewiredPlayer();
            if (rewiredPlayer != null)
                return rewiredPlayer.GetAxis(axisId);
#endif

            return Input.GetAxis(axisId);
        }

        /// <summary>
        /// Returns <c>true</c> when both movement and actions are locked on the active profile.
        /// </summary>
        public bool IsFullyBlocked() => CurrentProfile != null && CurrentProfile.lockMove && CurrentProfile.lockAction;

        // ──────────────────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────────────────

        private void Awake()
        {
            BuildMap();
            if (loadFromJson) LoadJson();
            BuildGlobalMap();

            if (!string.IsNullOrEmpty(initialProfileId) && _map.ContainsKey(initialProfileId))
                SetProfile(initialProfileId);

#if INPUTMANAGER_REWIRED
            // Rewired ist zum Awake-Zeitpunkt ggf. noch nicht bereit — Lazy-Initialisierung via GetRewiredPlayer()
            if (Rewired.ReInput.isReady)
                _rewiredPlayer = Rewired.ReInput.players.GetPlayer(rewiredPlayerId);
#endif
        }

#if INPUTMANAGER_REWIRED
        // ──────────────────────────────────────────────────────────
        // Rewired-Hilfsmethoden
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Gibt den gecachten Rewired-Spieler zurück. Initialisiert bei Bedarf lazy.
        /// </summary>
        private Rewired.Player GetRewiredPlayer()
        {
            if (_rewiredPlayer != null) return _rewiredPlayer;
            if (Rewired.ReInput.isReady)
                _rewiredPlayer = Rewired.ReInput.players.GetPlayer(rewiredPlayerId);
            return _rewiredPlayer;
        }
#endif

        // ──────────────────────────────────────────────────────────
        // Internal helpers
        // ──────────────────────────────────────────────────────────

        private void NotifyProfileChanged(InputProfile profile)
        {
            if (verboseLogging)
                Debug.Log($"[InputManager] Active profile: {profile.id}");

            OnProfileChanged?.Invoke(profile.id);

            bool blocked = IsFullyBlocked();
            if (blocked && !_wasBlocked)    OnInputBlocked?.Invoke();
            else if (!blocked && _wasBlocked) OnInputUnblocked?.Invoke();
            _wasBlocked = blocked;
        }

        private bool TryGetProfile(string id, out InputProfile profile)
        {
            if (string.IsNullOrEmpty(id) || !_map.TryGetValue(id, out profile))
            {
                Debug.LogWarning($"[InputManager] Profile not found: '{id}'");
                profile = null;
                return false;
            }
            return true;
        }

        private void BuildMap()
        {
            _map.Clear();
            foreach (var p in profiles)
            {
                if (string.IsNullOrEmpty(p.id)) continue;
                _map[p.id] = p;
            }
        }

        private void BuildGlobalMap()
        {
            _globalMap.Clear();
            foreach (var b in globalBindings)
            {
                if (string.IsNullOrEmpty(b.actionId)) continue;
                _globalMap[b.actionId] = b.primaryKey;
            }
        }

        private void LoadJson()
        {
            string full = Path.Combine(Application.streamingAssetsPath, jsonPath);
            if (!File.Exists(full))
            {
                Debug.LogWarning($"[InputManager] JSON not found: {full}");
                return;
            }
            try
            {
                string json = File.ReadAllText(full);
                var manifest = JsonUtility.FromJson<InputManifestJson>(json);
                foreach (var p in manifest.profiles)
                {
                    if (string.IsNullOrEmpty(p.id)) continue;
                    _map[p.id] = p;
                }
                if (verboseLogging)
                    Debug.Log($"[InputManager] Loaded {manifest.profiles.Count} profiles from {jsonPath}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputManager] Failed to parse {jsonPath}: {ex.Message}");
            }
        }
    }
}
