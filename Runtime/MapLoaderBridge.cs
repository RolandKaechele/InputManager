#if INPUTMANAGER_MLF
using UnityEngine;
using MapLoaderFramework.Runtime;

namespace InputManager.Runtime
{
    /// <summary>
    /// Optional bridge between InputManager and MapLoaderFramework.
    /// Enable define <c>INPUTMANAGER_MLF</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Pushes the <c>"blocked"</c> profile when a chapter change starts and pops it when
    /// the new map has finished loading, preventing player input during scene transitions.
    /// </para>
    /// </summary>
    [AddComponentMenu("InputManager/Map Loader Bridge")]
    [DisallowMultipleComponent]
    public class MapLoaderBridge : MonoBehaviour
    {
        [Tooltip("Profile id to push while a map is loading.")]
        [SerializeField] private string blockedProfileId = "blocked";

        private InputManager _input;
        private MapLoaderManager _mlf;

        private void Awake()
        {
            _input = GetComponent<InputManager>() ?? FindFirstObjectByType<InputManager>();
            _mlf   = GetComponent<MapLoaderManager>() ?? FindFirstObjectByType<MapLoaderManager>();

            if (_input == null) Debug.LogWarning("[InputManager/MapLoaderBridge] InputManager not found.");
            if (_mlf   == null) Debug.LogWarning("[InputManager/MapLoaderBridge] MapLoaderManager not found.");
        }

        private void OnEnable()
        {
            if (_mlf != null)
            {
                _mlf.OnChapterChanged += OnChapterChanged;
                _mlf.OnMapLoaded      += OnMapLoaded;
            }
        }

        private void OnDisable()
        {
            if (_mlf != null)
            {
                _mlf.OnChapterChanged -= OnChapterChanged;
                _mlf.OnMapLoaded      -= OnMapLoaded;
            }
        }

        private void OnChapterChanged(int previous, int current) => _input?.PushProfile(blockedProfileId);
        private void OnMapLoaded(MapData mapData)                 => _input?.PopProfile();
    }
}
#else
namespace InputManager.Runtime
{
    /// <summary>No-op stub — enable define <c>INPUTMANAGER_MLF</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("InputManager/Map Loader Bridge")]
    public class MapLoaderBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[InputManager/MapLoaderBridge] Bridge disabled — add INPUTMANAGER_MLF to Scripting Define Symbols.");
    }
}
#endif
