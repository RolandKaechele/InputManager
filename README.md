# InputManager

Centralized input profile manager for Unity.  
Manages a stack of named input profiles to block or remap player input in response to game state changes (cutscenes, dialogues, mini-games, loading screens).  
Supports JSON-driven profiles for modding and custom key bindings via SaveManager persistence.


## Features

- **SetProfile / PushProfile / PopProfile** — stack-based profile management for modal input contexts
- **Action query** — `IsActionDown(actionId)` with profile-level binding overrides and global fallback
- **Axis query** — `GetAxis(axisId)` returns 0 when movement is locked by the active profile
- **Block detection** — `OnInputBlocked` / `OnInputUnblocked` events for UI feedback
- **JSON / Modding** — define profiles in `StreamingAssets/input_profiles.json`; merged by `id` on top of Inspector data
- **Events** — `OnProfileChanged`, `OnInputBlocked`, `OnInputUnblocked` for reactive integration
- **Rewired integration** — use Rewired as the input backend (replace Unity legacy Input); player id configurable in Inspector (activated via `INPUTMANAGER_REWIRED`)
- **StateManager integration** — auto-switch profile on `AppState` change (activated via `INPUTMANAGER_STM`)
- **CutsceneManager integration** — push `"blocked"` profile on sequence start; pop on end/skip (activated via `INPUTMANAGER_CSM`)
- **DialogueManager integration** — push `"dialogue"` profile on dialogue start; pop on complete (activated via `INPUTMANAGER_DM`)
- **MiniGameManager integration** — push `"minigame"` profile on mini-game start; pop on complete/abort (activated via `INPUTMANAGER_MGM`)
- **MapLoaderFramework integration** — push `"blocked"` profile during chapter loads; pop on loaded (activated via `INPUTMANAGER_MLF`)
- **SaveManager integration** — persist and restore custom key bindings (activated via `INPUTMANAGER_SM`)
- **EventManager integration** — broadcast `input.profileChanged`, `input.blocked`, `input.unblocked` events (activated via `INPUTMANAGER_EM` or `EVENTMANAGER_INP`)
- **Custom Inspector** — live profile controls, active profile display, and registered profile list in Play Mode


## Installation

### Option A — Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL…**
3. Enter:

   ```
   https://github.com/RolandKaechele/InputManager.git
   ```

### Option B — Clone into Assets

```bash
git clone https://github.com/RolandKaechele/InputManager.git Assets/InputManager
```

### Option C — npm / postinstall

```bash
cd Assets/InputManager
npm install
```


## Scene Setup

1. Create a persistent manager GameObject (or reuse your existing manager object).
2. Attach `InputManager`.
3. Set `initialProfileId` to the default profile (e.g. `"gameplay"`).
4. Add profile definitions in the Inspector (or via `input_profiles.json`).
5. Attach any bridge components (see Bridge Components below).

### Recommended default profiles

| Id | lockMove | lockAction | lockLook | Use |
| -- | -------- | ---------- | -------- | --- |
| `gameplay` | false | false | false | Normal play |
| `dialogue` | true | false | false | Dialogue — move locked, confirm available |
| `minigame` | false | false | false | Mini-game — custom bindings |
| `blocked` | true | true | true | Cutscene / loading — all input blocked |


## Quick Start

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `profiles` | *(empty)* | Built-in input profiles |
| `initialProfileId` | `"gameplay"` | Profile activated on Awake |
| `globalBindings` | *(empty)* | Fallback key bindings for all profiles |
| `loadFromJson` | `false` | Merge profiles from `input_profiles.json` |
| `jsonPath` | `"input_profiles.json"` | Path relative to `StreamingAssets/` |
| `maxStackDepth` | `8` | Maximum profile stack depth |
| `verboseLogging` | `false` | Log all profile transitions to Console |
| `rewiredPlayerId` *(INPUTMANAGER_REWIRED)* | `0` | Rewired player index to read input from |

### InputProfile fields

| Field | Description |
| ----- | ----------- |
| `id` | Unique id, e.g. `"gameplay"`, `"blocked"` |
| `displayName` | Human-readable label |
| `lockMove` | Block movement input |
| `lockAction` | Block action input (confirm, cancel, attack…) |
| `lockLook` | Block camera/look input |
| `category` | Tag, e.g. `"gameplay"`, `"modal"` |
| `bindings` | Per-profile key overrides (list of `InputActionBinding`) |

### Code usage

```csharp
var inp = FindFirstObjectByType<InputManager.Runtime.InputManager>();

inp.SetProfile("gameplay");
inp.PushProfile("dialogue");  // overlay dialogue profile
inp.PopProfile();             // return to previous

bool jump   = inp.IsActionDown("jump");
float moveH = inp.GetAxis("Horizontal");
bool blocked = inp.IsFullyBlocked();

// Subscribe to events
inp.OnProfileChanged += id => Debug.Log($"Profile: {id}");
inp.OnInputBlocked   += () => Debug.Log("Input blocked");
```


## Rewired Integration

When `INPUTMANAGER_REWIRED` is defined, `IsActionDown` and `GetAxis` are routed through Rewired instead of Unity's legacy Input system.

**Setup:**

1. Install Rewired from the Unity Asset Store.
2. Add the Rewired `InputManager` prefab to your scene.
3. Add `INPUTMANAGER_REWIRED` to **Project Settings → Player → Scripting Define Symbols**.
4. Set `rewiredPlayerId` on the InputManager Inspector (default `0`).
5. Use the same string action names in `IsActionDown`/`GetAxis` that are configured in your Rewired Input Manager.

> Profile-level blocking (`lockMove`, `lockAction`) and the profile stack work identically whether Rewired is active or not.


## Bridge Components

| Component | Define | Effect |
| --------- | ------ | ------ |
| `StateManagerBridge` | `INPUTMANAGER_STM` | Set profile mapped to `AppState` |
| `CutsceneManagerBridge` | `INPUTMANAGER_CSM` | Push `"blocked"` on sequence start; pop on end/skip |
| `DialogueManagerBridge` | `INPUTMANAGER_DM` | Push `"dialogue"` on start; pop on complete |
| `MiniGameManagerBridge` | `INPUTMANAGER_MGM` | Push `"minigame"` on start; pop on complete/abort |
| `MapLoaderBridge` | `INPUTMANAGER_MLF` | Push `"blocked"` on chapter change; pop on loaded |
| `SaveManagerBridge` | `INPUTMANAGER_SM` | Persist/restore custom key bindings |
| `EventManagerBridge` | `INPUTMANAGER_EM` | Fire `input.profileChanged/blocked/unblocked` via EventManager |

EventManager can also re-broadcast InputManager events using `InputEventBridge` (define: `EVENTMANAGER_INP`).


## JSON / Modding

Place `input_profiles.json` in `StreamingAssets/` (path is configurable):

```json
{
  "profiles": [
    {
      "id": "driving",
      "displayName": "Driving",
      "lockMove": false,
      "lockAction": false,
      "lockLook": false,
      "category": "gameplay",
      "bindings": [
        { "actionId": "accelerate", "primaryKey": 119, "secondaryKey": 273 }
      ]
    }
  ]
}
```

JSON entries are **merged by id** — mods can add new profiles or override Inspector definitions without reimporting.


## Optional Integrations

| Define | Integration |
| ------ | ----------- |
| `INPUTMANAGER_REWIRED` | InputManager ←→ Rewired (input backend) |
| `INPUTMANAGER_STM` | InputManager ←→ StateManager |
| `INPUTMANAGER_CSM` | InputManager ←→ CutsceneManager |
| `INPUTMANAGER_DM` | InputManager ←→ DialogueManager |
| `INPUTMANAGER_MGM` | InputManager ←→ MiniGameManager |
| `INPUTMANAGER_MLF` | InputManager ←→ MapLoaderFramework |
| `INPUTMANAGER_SM` | InputManager ←→ SaveManager (key binding persistence) |
| `INPUTMANAGER_EM` | InputManager → EventManager (fire events) |
| `EVENTMANAGER_INP` | EventManager ← InputManager (re-broadcast) |
