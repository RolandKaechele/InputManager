#if INPUTMANAGER_VID
using UnityEngine;
using VideoManager.Runtime;

namespace InputManager.Runtime
{
    /// <summary>
    /// Optional bridge between InputManager and VideoManager.
    /// Enable define <c>INPUTMANAGER_VID</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Pushes the <c>"video"</c> input profile when a video starts (typically a locked/no-input
    /// profile) and pops it when the video ends, restoring the previous profile.
    /// </para>
    /// </summary>
    [AddComponentMenu("InputManager/Video Manager Bridge")]
    [DisallowMultipleComponent]
    public class VideoManagerBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [Tooltip("Input profile id to push while a video is playing. Should have lockMove and lockAction enabled.")]
        [SerializeField] private string videoProfileId = "video";

        // ─── References ───────────────────────────────────────────────────────
        private InputManager _input;
        private VideoManager.Runtime.VideoManager _video;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _input = GetComponent<InputManager>() ?? FindFirstObjectByType<InputManager>();
            _video = GetComponent<VideoManager.Runtime.VideoManager>()
                     ?? FindFirstObjectByType<VideoManager.Runtime.VideoManager>();

            if (_input == null) Debug.LogWarning("[InputManager/VideoManagerBridge] InputManager not found.");
            if (_video == null) Debug.LogWarning("[InputManager/VideoManagerBridge] VideoManager not found.");
        }

        private void OnEnable()
        {
            if (_video != null)
            {
                _video.OnVideoStarted   += OnVideoStarted;
                _video.OnVideoCompleted += OnVideoEnded;
                _video.OnVideoStopped   += OnVideoEnded;
            }
        }

        private void OnDisable()
        {
            if (_video != null)
            {
                _video.OnVideoStarted   -= OnVideoStarted;
                _video.OnVideoCompleted -= OnVideoEnded;
                _video.OnVideoStopped   -= OnVideoEnded;
            }
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnVideoStarted(string _id) => _input?.PushProfile(videoProfileId);
        private void OnVideoEnded(string _id)   => _input?.PopProfile();
    }
}
#else
namespace InputManager.Runtime
{
    /// <summary>No-op stub – enable define <c>INPUTMANAGER_VID</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("InputManager/Video Manager Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class VideoManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
