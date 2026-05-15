# RPG Project Systems Notes

Last updated: 2026-05-14

This file tracks implemented systems and future ownership decisions so we avoid duplicated code and keep architecture consistent.

## Dialogue And Quest

Core scripts:
- `Assets/Game/Scripts/Gameplay/Dialogue/DialogueDefinition.cs`
- `Assets/Game/Scripts/Gameplay/Dialogue/QuestDefinition.cs`
- `Assets/Game/Scripts/Gameplay/Dialogue/QuestKillTarget.cs`
- `Assets/Game/Scripts/Systems/Dialogue/DialogueManager.cs`
- `Assets/Game/Scripts/Systems/Dialogue/QuestManager.cs`
- `Assets/Game/Scripts/Systems/Dialogue/DialogueUIController.cs`

Implemented:
- Dialogue ScriptableObjects with lines, speaker side, portrait per line, choices, optional next dialogue, and quest link.
- Dialogue choices can accept/decline/continue/close and can branch to another dialogue.
- Dialogue and choices support optional `DialogueCondition` entries based on `QuestState`.
- Dialogue conditions also support item requirements through `InventoryRequirement`.
- `DialogueManager` owns active dialogue state, seen-dialogue progress, choice filtering, dialogue chaining, and dialogue progress snapshots.
- `QuestManager` owns quest state, objective progress, quest log entries, reward claiming, and quest progress snapshots.
- Quests can grant configured inventory item rewards when claimed; reward claiming fails without changing quest state if inventory space is insufficient.
- `QuestKillTarget` reports kill progress to `QuestManager`.
- `NPCInteractionTarget` picks the correct dialogue for available, active, completed, reward claimed, or failed quest states.
- Dialogue UI supports typewriter text, portraits, up to four choice buttons, and keyboard choice selection.
- Dialogue typewriter timing is isolated in `DialogueTypewriter`, and dialogue keyboard shortcuts are isolated in `DialogueKeyboardInput`.

Important:
- Do not create a second dialogue progress store. Use `DialogueManager.CreateProgressSnapshot()` and `LoadProgressSnapshot()`.
- Do not create a second quest progress store. Use `QuestManager.CreateProgressSnapshot()` and `LoadProgressSnapshot()`.
- Future save/load should replace or wrap `GameProgressSaveManager`, not duplicate its snapshot logic.

## Global Feedback UI

Core scripts:
- `Assets/Game/Scripts/Systems/UI/GameplayUIEvents.cs`
- `Assets/Game/Scripts/Systems/UI/GameplayUIManager.cs`
- `Assets/Game/Scripts/Systems/UI/GameplayInputBlocker.cs`
- `Assets/Game/Scripts/Systems/UI/GlobalFeedbackUIController.cs`

Implemented:
- Central gameplay input blocking while dialogue, inventory, quest log, loot, or UI pointer interaction is active.
- UI close/open actions keep gameplay blocked for a few extra frames so button clicks do not leak into movement or interaction.
- Global temporary feedback messages for info, success, warning, error, quest, and loot.
- Gameplay systems request UI feedback through `GameplayUIEvents`; `GameplayUIManager` forwards those requests to visual controllers and listens to quest/item-use events for feedback.
- Inspector-adjustable visible time, fade time, same-source cooldown, and replacement delay.
- Same object feedback is rate-limited to prevent spam.
- Different object feedback replaces the current message with a short delay.
- Quest feedback listens to `QuestManager` events.

Connected interactions:
- `DoorInteractionTarget`
- `ContainerInteractionTarget`
- `EnemyInteractionTarget`
- `KeyPickupTarget`
- Quest accepted, objective updated, quest completed, reward claimed, failed.

Important:
- Any UI panel that should pause movement/action must call `GameplayInputBlocker.Instance.SetBlocker(this, visible)`.
- Player movement/action controllers should query `GameplayInputBlocker`, not individual UI systems.
- When showing interaction feedback, pass `source: gameObject` so cooldown/replacement works correctly.
- Gameplay scripts should not call `GlobalFeedbackUIController.Instance` directly. Use `GameplayUIEvents` so UI remains centralized.

## Quest Log UI

Core script:
- `Assets/Game/Scripts/Systems/UI/QuestLogUIController.cs`

Implemented:
- Canvas UI button named `QuestLogToggleButton`.
- Canvas panel named `QuestLogPanel`.
- Opens/closes via UI button or keyboard key `J`.
- Reads quest entries through `QuestManager.GetQuestLogEntries()`.
- Shows quest title, state, description, objective progress, and reward text.

Important:
- UI should read quest data through `QuestManager.GetQuestLogEntries()`, not through internal dictionaries.

## NPC Quest Indicator

Core script:
- `Assets/Game/Scripts/Systems/UI/NPCQuestIndicator.cs`

Implemented:
- World-space TextMeshPro marker above NPCs.
- Shows `!` for available/locked, `...` for active, `?` for completed.
- Indicator has a small bob/pulse animation, bold text, high sorting order, and Inspector-tunable colors/symbols for readability.
- Hides after reward claimed or failed by default.
- Refreshes on `QuestManager.QuestStateChanged`.

Important:
- Indicator depends on `NPCInteractionTarget.QuestToOffer`.
- If NPC presentation grows later, keep indicator visual logic here rather than in `NPCInteractionTarget`.
- Keep quest marker presentation self-contained in `NPCQuestIndicator`; other systems should only change quest state through `QuestManager`.

## Interaction Targets

Core scripts:
- `Assets/Game/Scripts/Inputs/PlayerInputReader.cs`
- `Assets/Game/Scripts/Inputs/PlayerInputState.cs`
- `Assets/Game/Scripts/Systems/Movement/PlayerMovementController.cs`
- `Assets/Game/Scripts/Character/Movement/CharacterMotor2D.cs`
- `Assets/Game/Scripts/Systems/Interaction/PlayerActionController.cs`
- `Assets/Game/Scripts/Gameplay/Interaction/Targets/RightClickActionTarget.cs`
- `Assets/Game/Scripts/Gameplay/Interaction/Targets/NPCInteractionTarget.cs`
- `Assets/Game/Scripts/Gameplay/Interaction/Targets/DoorInteractionTarget.cs`
- `Assets/Game/Scripts/Gameplay/Interaction/Targets/ContainerInteractionTarget.cs`
- `Assets/Game/Scripts/Gameplay/Interaction/Targets/EnemyInteractionTarget.cs`
- `Assets/Game/Scripts/Gameplay/Interaction/Targets/KeyPickupTarget.cs`

Implemented:
- Player input uses one capture component: `PlayerInputReader` reads movement, pointer, click-to-move, and right-click action state.
- `PlayerInputReader` publishes input events and stores values in pure `PlayerInputState`; movement/action controllers subscribe instead of polling one-shot input every frame.
- `PlayerMovementController` converts movement input and click-to-move targets into movement intent.
- `CharacterMotor2D` is the only player movement component that writes to `Rigidbody2D` velocity.
- Click-to-move uses `CharacterMotor2D.SetMovementTarget()` so physics movement is clamped in `FixedUpdate` and cannot overshoot/tremble around the clicked point.
- `PlayerActionController` bridges action input to world interaction/combat targets.
- Right-click action abstraction with `RightClickActionType`.
- NPC talks.
- Doors open or show locked feedback.
- Containers show locked or loot feedback.
- Enemies can be defeated and report quest kill progress.
- Keys can be picked up as an interaction target.

Important:
- Do not write directly to player `Rigidbody2D` from input or action controllers. Route movement through `CharacterMotor2D`.
- For RPG-style click movement, route world destinations through `PlayerMovementController.SetMoveTarget()`; do not approximate it by repeatedly setting a normalized direction toward the cursor.
- Keep input capture separate from gameplay logic: `PlayerInputReader` reads input, controllers bridge intent, targets/managers own rules.
- Prefer event/state handoff from input capture to gameplay controllers. Do not add per-feature input polling when a `PlayerInputReader` event or `PlayerInputState` value is enough.
- Do not add extra player input readers for each feature unless the input surface becomes large enough to justify a deliberate split.
- `ItemPickupTarget` is the generic pickup target for inventory items.
- `KeyPickupTarget` inherits from `ItemPickupTarget` and only changes feedback text.
- Doors and containers should check `InventoryManager`, not local item ownership.

## Inventory And Items

Core scripts:
- `Assets/Game/Scripts/Gameplay/Inventory/ILootSource.cs`
- `Assets/Game/Scripts/Gameplay/Inventory/LootTableDefinition.cs`
- `Assets/Game/Scripts/Gameplay/Inventory/LootDropEntry.cs`
- `Assets/Game/Scripts/Gameplay/Inventory/LootRarity.cs`
- `Assets/Game/Scripts/Gameplay/Inventory/ItemDefinition.cs`
- `Assets/Game/Scripts/Gameplay/Inventory/ItemStackDefinition.cs`
- `Assets/Game/Scripts/Gameplay/Inventory/ItemUseEffect.cs`
- `Assets/Game/Scripts/Gameplay/Inventory/FeedbackItemUseEffect.cs`
- `Assets/Game/Scripts/Systems/Inventory/InventoryManager.cs`
- `Assets/Game/Scripts/Systems/Inventory/InventoryUIController.cs`
- `Assets/Game/Scripts/Systems/Inventory/InventorySlotUI.cs`
- `Assets/Game/Scripts/Systems/Inventory/InventoryWorldDropper.cs`
- `Assets/Game/Scripts/Systems/Inventory/LootService.cs`

Implemented:
- `ItemDefinition` ScriptableObject with stable `itemId`, display name, description, category, icon, and stacking rules.
- `ItemStackDefinition` for Inspector-configured item/amount pairs.
- `InventoryManager` owns all runtime item ownership and item amounts.
- `InventoryRequirement` centralizes item requirement data and checks/consume logic for doors, containers, and conditions.
- `InventoryRequirementService` isolates requirement satisfaction against `IInventoryService` so locked interactions do not duplicate check/consume flow.
- Inventory supports add/remove/has/get amount by `ItemDefinition` or item id.
- Items can optionally reference an `ItemUseEffect` ScriptableObject.
- `InventoryManager.TryUseItem()` runs the item effect, shows feedback, and consumes one item only when `consumeOnUse` is enabled and the effect succeeds.
- `FeedbackItemUseEffect` is the first simple use effect and can be reused for keys, lore items, or placeholder items.
- Inventory exposes change events by item reference and item id.
- Inventory creates and loads snapshots for save/load.
- Simple `InventoryPanel` UI opens via button or keyboard key `I`.
- Inventory UI uses reusable item slots with icon, amount, category, selection state, and empty slots.
- Inventory UI supports number-key selection, click selection, an item action menu with use/drop/cancel, and uses selected item through the `UseButton` or key `U`.
- Inventory slots support drag/drop reordering through `InventoryManager.MoveItemToIndex()`.
- Dragging an item outside the inventory asks `InventoryWorldDropper` to create an `ItemPickupTarget` in the world, then removes one item from inventory only if the world drop succeeds.
- Inventory UI has a details area for selected item description, category, amount, and usability.
- Inventory details text formatting is isolated in `InventoryDetailsFormatter`.
- Inventory drag icon presentation is isolated in `InventoryDragIconPresenter`, and inventory keyboard shortcuts are isolated in `InventoryKeyboardInput`.
- Inventory panel-level visual operations are isolated in `InventoryPanelView`, while `InventoryUIController` remains the compatibility orchestrator for serialized scene references and inventory flow.
- Inventory refresh, slot binding, empty state, details, and button state presentation are isolated in `InventoryPresenter`.
- Inventory selection/drag state is isolated in `InventoryInteractionFlow`.
- Inventory slot merge/move behavior is isolated in `InventorySlotTransferService`.
- Inventory world-drop flow is isolated in `InventoryDropFlow`.
- Loot UI opens any `ILootSource`, not only containers.
- `LootService` centralizes loot flow: open loot UI when available, fallback to claim-all, and player-facing loot feedback.
- `LootClaimService` isolates loot stack transfer into inventory so containers/corpses do not duplicate add-item loops.
- Dead enemies can expose loot through `CorpseLootSource` while reusing the same loot UI.
- Loot tables roll entries once per corpse, with per-item chance and min/max amount.

Scene/asset setup:
- `InventoryManager` is attached to `Managers`.
- `InventoryUIController` is attached to `UIManager`.
- `InventoryWorldDropper` is attached to `Managers`.
- `LootService` is attached to `Managers`.
- `InventoryToggleButton` and `InventoryPanel` exist under `Canvas`.
- Example item asset created at `Assets/Game/ScriptableObjects/Items/BasicKey.asset`.
- Example use effect asset created at `Assets/Game/ScriptableObjects/Items/UseEffects/BasicKeyUseEffect.asset`.

Important:
- Use `ItemDefinition` references for real content whenever possible.
- Use string ids only as fallback/migration while assets are being created.
- Do not create separate item ownership on doors, containers, quests, or pickups. They should query `InventoryManager`.
- Future item conditions for quests/dialogue should query `InventoryManager`.
- Do not put item-specific use logic in `InventoryManager`. Create/assign an `ItemUseEffect` asset instead.
- Do not duplicate item lock checks. Use `InventoryRequirement` and satisfy/consume it through `InventoryRequirementService`.
- Do not create dropped-item logic inside UI slots. Use `InventoryWorldDropper`.
- Do not open `LootUIController` or show loot claim feedback directly from interaction targets. Use `LootService`.
- Loot sources should own content/state/claim rules, but not decide UI flow.
- Quest item rewards should stay on `QuestDefinition` and be granted through `QuestManager.TryClaimReward()`, not directly from NPCs or UI.
- Future equipment, hotbar, or container swaps should reuse `InventorySlotUI` and add transfer rules above `InventoryManager`, not duplicate slot visuals.

## Combat And Health

Core scripts:
- `Assets/Game/Scripts/Gameplay/Combat/HealthComponent.cs`
- `Assets/Game/Scripts/Gameplay/Combat/HealthChange.cs`
- `Assets/Game/Scripts/Gameplay/Combat/HealthChangeType.cs`
- `Assets/Game/Scripts/Gameplay/Combat/CombatStatsDefinition.cs`
- `Assets/Game/Scripts/Gameplay/Combat/ICombatStatsProvider.cs`
- `Assets/Game/Scripts/Gameplay/Combat/CombatAttackSettings.cs`
- `Assets/Game/Scripts/Gameplay/Combat/DamageResolver.cs`
- `Assets/Game/Scripts/Gameplay/Combat/BasicDamageResolver.cs`
- `Assets/Game/Scripts/Gameplay/Combat/EnemyCombatBehaviorSettings.cs`
- `Assets/Game/Scripts/Systems/Combat/CombatActor.cs`
- `Assets/Game/Scripts/Systems/Combat/AutoAttackController.cs`
- `Assets/Game/Scripts/Systems/Combat/EnemyCombatController.cs`
- `Assets/Game/Scripts/Systems/Combat/CombatFollowToggleUIController.cs`
- `Assets/Game/Scripts/Systems/Combat/HealthChangeFeedbackPresenter.cs`
- `Assets/Game/Scripts/Systems/Combat/FloatingCombatText.cs`
- `Assets/Game/Scripts/Systems/Combat/HealthBarPresenter.cs`
- `Assets/Game/Scripts/Systems/Combat/HitFlashPresenter.cs`
- `Assets/Game/Scripts/Systems/Combat/CorpseDecayController.cs`
- `Assets/Game/Scripts/Gameplay/Combat/CorpseLootSource.cs`

Implemented:
- Reusable `HealthComponent` for player, enemies, NPCs, or destructible objects.
- Tracks current/max hit points, normalized health, dead/alive state.
- Supports damage, healing, max-health changes, and explicit revive.
- Publishes `HealthChanged`, `Died`, and `Revived` events with immutable `HealthChange` data.
- Damage clamps at zero and death fires once while already dead.
- Healing does not revive dead targets; revive is explicit.
- `CombatActor` owns runtime attack/defense attributes for active combatants and implements `ICombatStatsProvider`.
- `CharacterCombatStats` remains as a legacy/lightweight compatibility stats provider for non-actor combat objects, but new enemies should prefer `CombatActor` directly.
- `CombatStatsDefinition` stores reusable base attack/defense values as ScriptableObjects.
- `BasicDamageResolver` currently calculates `baseDamage + attacker attack - target defense`, clamped at zero.
- Right-clicking an enemy selects it as a `CombatTarget`; the player auto-chases and attacks while the target is alive.
- Player target follow can be toggled through `AutoAttackController.SetFollowTarget()` and the visible combat follow UI.
- When player target follow is disabled and the selected target is out of range, `AutoAttackController` routes the attempt through `CombatActor.AttackOutOfRange` and requests a warning via `GameplayUIEvents`.
- `CombatAttackSettings` stores starter attack range, damage, and attack cadence.
- `DamageResolver` is the extension point for future hit formulas; `BasicDamageResolver` currently combines base damage, attack, and defense.
- `CombatActor` centralizes combat identity and attack execution for players and enemies: stats, current target, target selection frame, targetability, attack range checks, attack cooldown, damage resolution, damage application, and combat events.
- `CombatSelectionPresenter` owns combat selection visuals for `CombatActor` and legacy `CombatTarget`; combat components publish selection state instead of building frame visuals directly.
- `AutoAttackController` now owns player-specific combat decisions only: follow target, chase selected target, and request attacks through `CombatActor`.
- `EnemyCombatController` now owns enemy AI decisions only: acquire player, chase/hold/flee, and request attacks through `CombatActor`.
- `CombatTarget` remains for compatibility/tests, but new combatants should use `CombatActor` as their targetable component.
- `HealthChangeFeedbackPresenter` listens to `HealthComponent.HealthChanged` and spawns floating combat text for damage/healing.
- `FloatingCombatText` animates hit values upward and fades them out, so future variable-hit systems only need to publish the final damage through `HealthComponent`.
- `HealthBarPresenter` shows an enemy health bar above damaged targets.
- `HitFlashPresenter` briefly flashes a target sprite when damage lands.
- `CorpseDecayController` leaves dead enemies in the world for loot, darkens them over time, then disables the body.
- Tutorial enemies now use the smaller visual presenters directly instead of a combined enemy-only visual component.
- `CorpseLootSource` owns corpse loot contents and one-time loot state.
- `EnemyCombatController` gives enemies a simple combat loop: acquire player, optionally chase, attack in range, or flee if behavior settings request it.
- `EnemyCombatBehaviorSettings` stores enemy combat style data so future melee, ranged, coward, caster, stealth, or other archetypes can branch by data instead of hard-coded enemy classes.
- Tutorial scene now gives the Player 100 HP and each mist rat 20 HP as starter combat values.
- Tutorial scene gives the Player starter combat stats, 1.5 attack range, and 1 attack per second.

Important:
- `HealthComponent` owns only hit points and death state.
- Do not put attack timing, enemy AI, armor, mitigation, damage types, loot, quest progress, or UI directly in `HealthComponent`.
- Do not put combat attributes in `HealthComponent`; use `CombatActor` for combatants or another `ICombatStatsProvider` only when there is no actor.
- Do not calculate combat formulas inside controllers. Controllers choose when to attack; `DamageResolver` decides how much damage is produced.
- Do not duplicate attack cooldown/range/damage application in player or enemy controllers. Route attack execution through `CombatActor`.
- Do not put attack cadence or damage application in `EnemyInteractionTarget`; it should route Attack actions into `AutoAttackController`.
- Do not make enemy-specific AI scripts for every creature type until behavior data cannot express the difference. Prefer `EnemyCombatBehaviorSettings` first.
- `EnemyInteractionTarget` may react to death for enemy-specific outcomes such as quest kill reporting or deactivation.
- Future combat systems should call `ApplyDamage()` / `Heal()` and react to events instead of writing health fields directly.
- Do not calculate damage inside visual feedback presenters; they should only display already-resolved `HealthChange` data.
- Do not disable dead enemies immediately when they should be lootable; let `CorpseDecayController` own body lifetime.
- Inspired by Tibia's separation between hit points, incoming damage, creature combat properties, and damage mitigation.

## Loot UI

Core script:
- `Assets/Game/Scripts/Systems/Inventory/LootService.cs`
- `Assets/Game/Scripts/Systems/Inventory/LootUIController.cs`

Implemented:
- Central `LootService` for opening loot UI, fallback claim-all, and loot feedback.
- Central loot panel named `LootPanel`.
- Containers and corpse loot sources open this panel instead of granting loot directly when `LootUIController` is present.
- Current version supports review plus `Pegar Tudo` / close.
- Loot panel view operations are isolated in `LootPanelView`.
- Loot content text formatting is isolated in `LootContentFormatter`.
- Loot keyboard shortcuts are isolated in `LootKeyboardInput`.
- Loot stack claiming is isolated in `LootClaimService`.
- If no `LootUIController` exists, containers still fallback to granting all loot immediately.
- `EnemyInteractionTarget` and `ContainerInteractionTarget` delegate loot flow to `LootService`.

Important:
- Keep container loot claiming inside `ContainerInteractionTarget.ClaimAllLoot()`.
- UI should request claims from `ILootSource.ClaimAllLoot()`, not add items directly to inventory.
- Interaction targets should request `LootService.OpenOrClaimAll()` instead of knowing about `LootUIController`.
- Corpse loot should live in `CorpseLootSource`; death/decay components should not grant inventory items directly.
- Prefer `LootTableDefinition` for enemy loot; direct `ItemStackDefinition[]` on `CorpseLootSource` is only a fallback.

## World State Persistence

Core script:
- `Assets/Game/Scripts/Systems/WorldState/PersistentWorldState.cs`

Implemented:
- `PersistentWorldObject` stores a stable `worldObjectId`.
- `IPersistentWorldState` lets components snapshot/restore their own small state.
- `DoorInteractionTarget`, `ContainerInteractionTarget`, and `ItemPickupTarget` implement world state persistence.
- `GameProgressSaveManager` saves/restores world object state snapshots.

Important:
- Any future world object that must persist should get `PersistentWorldObject` and implement `IPersistentWorldState`.
- Keep world state snapshots small and local. Complex systems should own their own snapshots like inventory/quest/dialogue already do.

## Save Load

Core script:
- `Assets/Game/Scripts/Systems/GameProgressSaveManager.cs`

Implemented:
- Temporary PlayerPrefs-based progress save/load.
- Saves quest snapshots and dialogue snapshots.
- Saves inventory snapshots.
- Saves world object snapshots for persistent pickups, doors, and containers.
- Supports `loadOnStart` and `saveOnQuit`.

Future decision:
- This is a bridge system. When global player save/load is implemented, move player position/stats/inventory/world state into a broader save system and keep these snapshot APIs as submodules.
- Do not duplicate quest/dialogue save data in a separate format without migrating this manager.

## Scene Connections

Scene:
- `Assets/Game/Scenes/TutorialScene.unity`

Connected under `Managers`:
- `Dialogue System`: `DialogueManager`, `QuestManager`
- `Inventory System`: `InventoryManager`, `InventoryWorldDropper`
- `Loot System`: `LootService`
- `Gameplay System`: `GameplayInputBlocker`
- `Save System`: `GameProgressSaveManager`

Connected under `UIManager`:
- `Gameplay UI System`: `GameplayUIManager`, `GlobalFeedbackUIController`
- `Dialogue UI System`: `DialogueUIController`
- `Quest Log UI System`: `QuestLogUIController`
- `Inventory UI System`: `InventoryUIController`
- `Loot UI System`: `LootUIController`
- `Combat UI System`: `CombatFollowToggleUIController`

Connected on Canvas:
- Dialogue panel and choice buttons.
- `GlobalFeedback`
- `QuestLogToggleButton`
- `QuestLogPanel`
- `InventoryToggleButton`
- `InventoryPanel`
- `LootPanel`
- `CombatFollowToggle`

Connected on NPCs:
- Existing NPC has `NPCQuestIndicator`.

Tutorial scene:
- `SampleScene` was renamed to `TutorialScene` and is now the first playable tutorial scene.
- Generated by `Assets/Editor/TutorialQuestSceneBuilder.cs` through `RPG Project > Build Tutorial Quest Scene`.
- Opening story: the player wakes in Vale de Lumen after a strange mist reaches the fields. Lio sends the player to Mira, the village guard.
- First quest: `O primeiro sinal` asks the player to talk to Mira.
- Second quest: `Ratos na nevoa` asks the player to defeat 10 mist rats.
- Returning to Mira after the rat quest grants `OldChapelKey` / `Chave da Capela Antiga`, the hook for the next tutorial beat.
- Tutorial scene disables automatic `GameProgressSaveManager` load/save so old PlayerPrefs progress cannot skip quest steps during testing.
- Tutorial scene now connects player combat stats, rat combat stats, rat movement, rat attack settings, rat chase behavior, enemy combat AI, and the follow toggle UI.

## Current Stop Point

Paused on:
- Combat polish after adding attack/defense attributes, optional target follow, and simple enemy AI.

Implemented in the latest pass:
- Added pure `PlayerInputState` and event-driven handoff from `PlayerInputReader` to movement/action controllers.
- `PlayerMovementController` and `PlayerActionController` now subscribe to click/movement/action events instead of polling all one-shot input every frame.
- Added `GameplayUIEvents` and `GameplayUIManager` so gameplay asks for UI feedback through a central UI event path.
- Consolidated quest feedback and item-use feedback into `GameplayUIManager`, removing separate feedback presenter components.
- Tutorial scene now has a dedicated `UIManager` GameObject for UI controllers, separate from gameplay/service `Managers`.
- Replaced direct `GlobalFeedbackUIController.Instance` calls in gameplay/inventory/quest feedback flow with `GameplayUIEvents`.
- Added `CombatActor` as the central runtime attack executor shared by player and enemies.
- Expanded `CombatActor` so new combatants no longer need separate `CharacterCombatStats` or `CombatTarget` components.
- Added `ICombatStatsProvider`; `BasicDamageResolver` now reads stats through this provider, keeping `CharacterCombatStats` as legacy compatibility.
- `AutoAttackController` and `EnemyCombatController` now delegate attack range, cooldown, target state, damage resolution, and damage application to `CombatActor`.
- Tutorial scene and `TutorialQuestSceneBuilder` now serialize stats/attack settings directly on `CombatActor` for the player and mist rats.
- Tutorial scene removed `CharacterCombatStats` and `CombatTarget` from combat actors.
- `CombatTarget.Health` now resolves its `HealthComponent` lazily, making tests/editor setup safer.
- Added `CombatActorTests` covering target selection, cooldown damage application, and target death cleanup.
- Added `LootService` as the central loot flow service.
- `LootUIController`, `EnemyInteractionTarget`, and `ContainerInteractionTarget` now delegate loot claim/open decisions to `LootService`.
- `CorpseLootSource` and `ContainerInteractionTarget` keep loot state/claim logic but no longer own player-facing loot feedback.
- Added follow-off out-of-range combat feedback through `CombatActor.AttackOutOfRange` and `GameplayUIEvents`.
- Extracted combat selection visuals into `CombatSelectionPresenter`.
- Extracted feedback cooldown policy into `FeedbackRateLimiter` and `GameplayFeedbackMessage`.
- Extracted save storage/snapshot helpers into `ISaveStorage`, `PlayerPrefsSaveStorage`, `GameProgressSaveData`, and `WorldStateSnapshotService`.
- Extracted quest reward granting and quest log projection into `QuestRewardService` and `QuestLogProjection`.
- Added `IInventoryService` as a first step toward replacing direct inventory singletons in domain services.
- Extracted inventory details text formatting into `InventoryDetailsFormatter`.
- Extracted dialogue typewriter and dialogue keyboard shortcut handling into `DialogueTypewriter` and `DialogueKeyboardInput`.
- Extracted inventory drag icon presentation and inventory keyboard shortcut handling into `InventoryDragIconPresenter` and `InventoryKeyboardInput`.
- Extracted inventory panel view operations into `InventoryPanelView` without changing existing scene references.
- Extracted loot panel view, content formatting, and keyboard shortcut handling into `LootPanelView`, `LootContentFormatter`, and `LootKeyboardInput`.
- Extracted inventory selection/drag flow, slot transfer, and world-drop orchestration into `InventoryInteractionFlow`, `InventorySlotTransferService`, and `InventoryDropFlow`.
- Extracted inventory refresh and slot/details/button presentation into `InventoryPresenter`.
- Extracted loot stack transfer into pure `LootClaimService`, and `ContainerInteractionTarget` now delegates inventory add-item loops to it.
- Extracted inventory requirement satisfaction into pure `InventoryRequirementService`, and doors/containers now delegate unlock check/consume flow to it.
- Reduced hard target lookup in `EnemyCombatController` and `CinemachinePlayerFollower` by adding serialized/injected target references with existing lookup kept as fallback.
- Tutorial scene and `TutorialQuestSceneBuilder` now include `LootService` on `Managers`.
- `PlayerInputReader` is now the single player input capture layer. `PlayerMovementController` and `PlayerActionController` consume it as intent bridges.
- Removed duplicated input readers/controllers: `PlayerMovementInputReader`, `PlayerActionInputReader`, and `PlayerInputMovementController`.
- `CombatStatsDefinition` and `CharacterCombatStats` add simple attack/defense attributes.
- `BasicDamageResolver` calculates `baseDamage + attacker attack - target defense`, clamped at zero.
- `AutoAttackController` supports optional target follow through `SetFollowTarget()`.
- `CombatFollowToggleUIController` connects a visible `Follow: ON/OFF` toggle to the player auto-attack follow mode.
- `EnemyCombatBehaviorSettings` stores enemy behavior data for attack mode and movement policy.
- `EnemyCombatController` gives enemies a first simple loop: find player, optionally chase, attack in range, or flee if configured.
- Tutorial scene and `TutorialQuestSceneBuilder` were updated to connect player stats, rat stats, rat movement, rat attack, rat behavior, enemy AI, and follow UI.
- Added `DamageResolverTests` covering attack/defense damage calculation and non-negative damage.

Validated:
- `RPGProject.Runtime.csproj` builds with 0 errors.
- `Assembly-CSharp-Editor.csproj` builds with 0 errors.
- `RPGProject.EditModeTests.csproj` builds with 0 errors.
- Unity EditMode tests passed: 29/29.
- Short Play Mode smoke test produced no console errors or warnings.

Recommended next work:
1. Consider moving corpse loot claiming out of `CorpseLootSource` into a non-MonoBehaviour loot model/service if inventory transfer rules become more complex.
2. Add visual/animation state for enemy chasing and attacking, preferably by listening to `CombatActor` and health events.
3. Review enemy detection and attack pacing in Play Mode for feel.
4. Decide whether ranged enemies should attack without moving as the first alternate enemy archetype.
5. Later, expand `EnemyCombatBehaviorSettings` with escape, spell, invisibility, and ranged/caster-specific rules instead of creating separate one-off enemy scripts.
