#if INPUTMANAGER_SM
using System.Text;
using UnityEngine;
using SaveManager.Runtime;

namespace InputManager.Runtime
{
    /// <summary>
    /// Optional bridge between InputManager and SaveManager.
    /// Enable define <c>INPUTMANAGER_SM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Persists all global key bindings as a JSON string in the save slot so custom
    /// remappings survive application restarts.
    /// </para>
    /// </summary>
    [AddComponentMenu("InputManager/Save Manager Bridge")]
    [DisallowMultipleComponent]
    public class SaveManagerBridge : MonoBehaviour
    {
        private const string SaveKey = "input.bindings";

        private InputManager _input;
        private SaveManager.Runtime.SaveManager _save;

        private void Awake()
        {
            _input = GetComponent<InputManager>() ?? FindFirstObjectByType<InputManager>();
            _save  = GetComponent<SaveManager.Runtime.SaveManager>()
                     ?? FindFirstObjectByType<SaveManager.Runtime.SaveManager>();

            if (_input == null) Debug.LogWarning("[InputManager/SaveManagerBridge] InputManager not found.");
            if (_save  == null) Debug.LogWarning("[InputManager/SaveManagerBridge] SaveManager not found.");
        }

        private void OnEnable()
        {
            if (_save != null)
            {
                _save.OnSaved  += OnSaved;
                _save.OnLoaded += OnLoaded;
            }
        }

        private void OnDisable()
        {
            if (_save != null)
            {
                _save.OnSaved  -= OnSaved;
                _save.OnLoaded -= OnLoaded;
            }
        }

        private void OnSaved(int slot)
        {
            if (_input == null || _save == null) return;

            // Serialize all profiles' bindings as a simple key=code pairs per profile
            var sb = new StringBuilder();
            foreach (var kvp in _input.Profiles)
            {
                foreach (var b in kvp.Value.bindings)
                    sb.Append($"{kvp.Key}:{b.actionId}={(int)b.primaryKey};");
            }
            _save.SetCustom(SaveKey, sb.ToString());
        }

        private void OnLoaded(int slot)
        {
            if (_input == null || _save == null) return;

            string data = _save.GetCustom(SaveKey);
            if (string.IsNullOrEmpty(data)) return;

            // Deserialize and apply overrides
            foreach (string entry in data.Split(';'))
            {
                if (string.IsNullOrEmpty(entry)) continue;
                // format: profileId:actionId=keyCode
                int colon = entry.IndexOf(':');
                int eq    = entry.IndexOf('=');
                if (colon < 0 || eq < 0) continue;

                string profileId = entry.Substring(0, colon);
                string actionId  = entry.Substring(colon + 1, eq - colon - 1);
                if (!int.TryParse(entry.Substring(eq + 1), out int keyInt)) continue;

                if (!_input.Profiles.TryGetValue(profileId, out var profile)) continue;
                foreach (var b in profile.bindings)
                {
                    if (b.actionId.Equals(actionId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        b.primaryKey = (KeyCode)keyInt;
                        break;
                    }
                }
            }
        }
    }
}
#else
namespace InputManager.Runtime
{
    /// <summary>No-op stub — enable define <c>INPUTMANAGER_SM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("InputManager/Save Manager Bridge")]
    public class SaveManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[InputManager/SaveManagerBridge] Bridge disabled — add INPUTMANAGER_SM to Scripting Define Symbols.");
    }
}
#endif
