using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class GameProgressSaveManager : MonoBehaviour
    {
        public static GameProgressSaveManager Instance { get; private set; }

        [Header("Storage")]
        [SerializeField]
        private string playerPrefsKey = "RPGProject.Progress";

        [Header("Behaviour")]
        [SerializeField]
        private bool loadOnStart = true;

        [SerializeField]
        private bool saveOnQuit = true;

        private readonly ISaveStorage storage = new PlayerPrefsSaveStorage();
        private readonly WorldStateSnapshotService worldStateSnapshots = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (loadOnStart)
            {
                Load();
            }
        }

        private void OnApplicationQuit()
        {
            if (saveOnQuit)
            {
                Save();
            }
        }

        public void Save()
        {
            var saveData = new GameProgressSaveData
            {
                quests = ToArray(QuestManager.Instance?.CreateProgressSnapshot()),
                dialogues = ToArray(DialogueManager.Instance?.CreateProgressSnapshot()),
                inventory = ToArray(InventoryManager.Instance?.CreateInventorySnapshot()),
                worldObjects = worldStateSnapshots.CaptureSceneState(),
            };

            string json = JsonUtility.ToJson(saveData);
            storage.Save(playerPrefsKey, json);
        }

        public bool Load()
        {
            if (!storage.TryLoad(playerPrefsKey, out string json))
            {
                return false;
            }

            GameProgressSaveData saveData = JsonUtility.FromJson<GameProgressSaveData>(json);
            QuestManager.Instance?.LoadProgressSnapshot(saveData.quests, notifyChanges: true);
            DialogueManager.Instance?.LoadProgressSnapshot(saveData.dialogues);
            InventoryManager.Instance?.LoadInventorySnapshot(saveData.inventory, notifyChanges: true);
            worldStateSnapshots.RestoreSceneState(saveData.worldObjects);
            return true;
        }

        public void DeleteSave()
        {
            storage.Delete(playerPrefsKey);
        }

        private static T[] ToArray<T>(IReadOnlyList<T> values)
        {
            if (values == null || values.Count == 0)
            {
                return Array.Empty<T>();
            }

            var array = new T[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                array[i] = values[i];
            }

            return array;
        }
    }
}
