using System;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager.Runtime
{
    /// <summary>
    /// Defines a single key binding for an action.
    /// </summary>
    [Serializable]
    public class InputActionBinding
    {
        [Tooltip("Unique action id, e.g. 'jump', 'confirm', 'cancel'.")]
        public string actionId;

        [Tooltip("Primary key code.")]
        public KeyCode primaryKey = KeyCode.None;

        [Tooltip("Secondary / alternative key code.")]
        public KeyCode secondaryKey = KeyCode.None;
    }

    /// <summary>
    /// Defines a named input profile that restricts or remaps input actions.
    /// </summary>
    [Serializable]
    public class InputProfile
    {
        [Tooltip("Unique identifier for this profile.")]
        public string id;

        [Tooltip("Human-readable display name.")]
        public string displayName;

        [Tooltip("Block all movement input while this profile is active.")]
        public bool lockMove;

        [Tooltip("Block all action (confirm/cancel/attack) input while this profile is active.")]
        public bool lockAction;

        [Tooltip("Block all look / camera-orbit input while this profile is active.")]
        public bool lockLook;

        [Tooltip("Category tag, e.g. 'gameplay', 'dialogue', 'blocked'.")]
        public string category;

        [Tooltip("Action bindings specific to this profile. Overrides the global bindings when active.")]
        public List<InputActionBinding> bindings = new List<InputActionBinding>();
    }

    /// <summary>
    /// JSON root wrapper used when loading input profiles from StreamingAssets.
    /// </summary>
    [Serializable]
    internal class InputManifestJson
    {
        public List<InputProfile> profiles = new List<InputProfile>();
    }
}
