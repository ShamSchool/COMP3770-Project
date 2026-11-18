# Timefall code documentation

| Class | Purpose and relationships |
| --- | --- |
| PlayerMovement | Handles player movement and jumping; checks the PauseMenu state. |
| TimeWarpManager | Switches past/present objects and notifies WarpAudio after a successful swap. |
| HazardContact | Requests a respawn through PlayerRespawn when the player touches a hazard. |
| PlayerRespawn | Returns the player to the level start or saved checkpoint and notifies WarpAudio of death. |
| CheckpointTerminal | Updates PlayerRespawn when the player activates a terminal with F. |
| EnemyMovement | Moves an enemy and turns it at obstacles or ledges using Rigidbody2D and SpriteRenderer. |
| WarpAudio | Plays music and effects in response to TimeWarpManager and PlayerRespawn events. |
| MainMenu | Displays menu panels and creates LevelButton instances. |
| LevelButton | Loads the scene selected from MainMenu. |
| PauseMenu | Pauses, resumes, restarts, or returns to the main menu. |
| MovingBackground | Scrolls a UI RawImage background. |
| MovingProps | Moves and wraps UI props using RectTransform. |
| NPCInteraction | Displays DialoguePage text and audio when a nearby player presses F. |
| DialoguePage | Stores dialogue text and optional audio for NPCInteraction. |
