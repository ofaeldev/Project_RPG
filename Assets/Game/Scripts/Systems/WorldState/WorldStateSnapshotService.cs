using System.Collections.Generic;
using UnityEngine;

namespace RPGProject.Systems
{
    public sealed class WorldStateSnapshotService
    {
        public WorldObjectStateSnapshot[] CaptureSceneState()
        {
            PersistentWorldObject[] persistentObjects = Object.FindObjectsByType<PersistentWorldObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            var snapshots = new List<WorldObjectStateSnapshot>();

            foreach (PersistentWorldObject persistentObject in persistentObjects)
            {
                if (persistentObject.TryCapture(out WorldObjectStateSnapshot snapshot))
                {
                    snapshots.Add(snapshot);
                }
            }

            return snapshots.ToArray();
        }

        public void RestoreSceneState(IEnumerable<WorldObjectStateSnapshot> snapshots)
        {
            if (snapshots == null)
            {
                return;
            }

            var snapshotsById = new Dictionary<string, WorldObjectStateSnapshot>();
            foreach (WorldObjectStateSnapshot snapshot in snapshots)
            {
                if (!string.IsNullOrWhiteSpace(snapshot.ObjectId))
                {
                    snapshotsById[snapshot.ObjectId] = snapshot;
                }
            }

            PersistentWorldObject[] persistentObjects = Object.FindObjectsByType<PersistentWorldObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (PersistentWorldObject persistentObject in persistentObjects)
            {
                if (snapshotsById.TryGetValue(persistentObject.WorldObjectId, out WorldObjectStateSnapshot snapshot))
                {
                    persistentObject.Restore(snapshot);
                }
            }
        }
    }
}
