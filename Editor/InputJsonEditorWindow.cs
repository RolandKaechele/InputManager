#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using InputManager.Runtime;
using UnityEditor;
using UnityEngine;

namespace InputManager.Editor
{
    // ────────────────────────────────────────────────────────────────────────────
    // Input Profiles JSON Editor Window
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Editor window for creating and editing <c>input_profiles.json</c> in StreamingAssets.
    /// Open via <b>JSON Editors → Input Manager</b> or via the Manager Inspector button.
    /// </summary>
    public class InputJsonEditorWindow : EditorWindow
    {
        private const string JsonFileName = "input_profiles.json";

        private InputProfileEditorBridge _bridge;
        private UnityEditor.Editor       _bridgeEditor;
        private Vector2                  _scroll;
        private string                   _status;
        private bool                     _statusError;

        [MenuItem("JSON Editors/Input Manager")]
        public static void ShowWindow() =>
            GetWindow<InputJsonEditorWindow>("Input Profiles JSON");

        private void OnEnable()
        {
            _bridge = CreateInstance<InputProfileEditorBridge>();
            Load();
        }

        private void OnDisable()
        {
            if (_bridgeEditor != null) DestroyImmediate(_bridgeEditor);
            if (_bridge      != null) DestroyImmediate(_bridge);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, _statusError ? MessageType.Error : MessageType.Info);

            if (_bridge == null) return;
            if (_bridgeEditor == null)
                _bridgeEditor = UnityEditor.Editor.CreateEditor(_bridge);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _bridgeEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField(
                Path.Combine("StreamingAssets", JsonFileName),
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) Load();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) Save();
            EditorGUILayout.EndHorizontal();
        }

        private void Load()
        {
            var path = Path.Combine(Application.streamingAssetsPath, JsonFileName);
            try
            {
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, JsonUtility.ToJson(new InputManifestEditorWrapper(), true));
                    AssetDatabase.Refresh();
                }

                var w = JsonUtility.FromJson<InputManifestEditorWrapper>(File.ReadAllText(path));
                _bridge.profiles = new List<InputProfile>(
                    w.profiles ?? Array.Empty<InputProfile>());

                if (_bridgeEditor != null) { DestroyImmediate(_bridgeEditor); _bridgeEditor = null; }

                _status     = $"Loaded {_bridge.profiles.Count} input profiles.";
                _statusError = false;
            }
            catch (Exception e)
            {
                _status     = $"Load error: {e.Message}";
                _statusError = true;
            }
        }

        private void Save()
        {
            try
            {
                var w    = new InputManifestEditorWrapper { profiles = _bridge.profiles.ToArray() };
                var path = Path.Combine(Application.streamingAssetsPath, JsonFileName);
                File.WriteAllText(path, JsonUtility.ToJson(w, true));
                AssetDatabase.Refresh();
                _status     = $"Saved {_bridge.profiles.Count} profiles to {JsonFileName}.";
                _statusError = false;
            }
            catch (Exception e)
            {
                _status     = $"Save error: {e.Message}";
                _statusError = true;
            }
        }
    }

    // ── ScriptableObject bridge ──────────────────────────────────────────────
    internal class InputProfileEditorBridge : ScriptableObject
    {
        public List<InputProfile> profiles = new List<InputProfile>();
    }

    // ── Local wrapper mirrors the internal InputManifestJson ─────────────────
    [Serializable]
    internal class InputManifestEditorWrapper
    {
        public InputProfile[] profiles = Array.Empty<InputProfile>();
    }
}
#endif
