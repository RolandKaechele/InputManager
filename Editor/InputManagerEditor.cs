using UnityEditor;
using UnityEngine;

namespace InputManager.Editor
{
    [CustomEditor(typeof(InputManager.Runtime.InputManager))]
    public class InputManagerEditor : UnityEditor.Editor
    {
        private string _previewId = string.Empty;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Open JSON Editor")) InputJsonEditorWindow.ShowWindow();

            var manager = (InputManager.Runtime.InputManager)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Live Controls", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use live controls.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Active Profile", manager.CurrentProfileId ?? "(none)");
            EditorGUILayout.LabelField("Fully Blocked",  manager.IsFullyBlocked().ToString());

            EditorGUILayout.Space();
            _previewId = EditorGUILayout.TextField("Profile Id", _previewId);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Profile"))  manager.SetProfile(_previewId);
            if (GUILayout.Button("Push Profile")) manager.PushProfile(_previewId);
            if (GUILayout.Button("Pop Profile"))  manager.PopProfile();
            EditorGUILayout.EndHorizontal();

            if (manager.Profiles.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Registered Profiles", EditorStyles.boldLabel);
                foreach (var kvp in manager.Profiles)
                {
                    bool active = kvp.Key == manager.CurrentProfileId;
                    EditorGUILayout.LabelField($"  {(active ? "►" : "○")} {kvp.Key}", kvp.Value.displayName ?? kvp.Key);
                }
            }

            Repaint();
        }
    }
}
