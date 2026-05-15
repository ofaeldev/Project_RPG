using System;
using UnityEngine;

namespace RPGProject.Systems
{
    [Serializable]
    public struct WorldObjectStateSnapshot
    {
        [SerializeField]
        private string objectId;

        [SerializeField]
        private bool flagA;

        [SerializeField]
        private bool flagB;

        [SerializeField]
        private int value;

        public string ObjectId => objectId;
        public bool FlagA => flagA;
        public bool FlagB => flagB;
        public int Value => value;

        public WorldObjectStateSnapshot(string objectId, bool flagA, bool flagB = false, int value = 0)
        {
            this.objectId = objectId;
            this.flagA = flagA;
            this.flagB = flagB;
            this.value = value;
        }
    }

    public interface IPersistentWorldState
    {
        WorldObjectStateSnapshot CaptureWorldState(string worldObjectId);
        void RestoreWorldState(WorldObjectStateSnapshot snapshot);
    }

    [DisallowMultipleComponent]
    public sealed class PersistentWorldObject : MonoBehaviour
    {
        [Tooltip("Stable id used by save/load. Keep unique inside the scene/world.")]
        [SerializeField]
        private string worldObjectId = string.Empty;

        public string WorldObjectId => worldObjectId;
        public bool HasValidId => !string.IsNullOrWhiteSpace(worldObjectId);

        private void Reset()
        {
            if (string.IsNullOrWhiteSpace(worldObjectId))
            {
                worldObjectId = $"{gameObject.scene.name}/{gameObject.name}";
            }
        }

        public bool TryCapture(out WorldObjectStateSnapshot snapshot)
        {
            snapshot = default;
            if (!HasValidId)
            {
                return false;
            }

            IPersistentWorldState state = GetComponent<IPersistentWorldState>();
            if (state == null)
            {
                return false;
            }

            snapshot = state.CaptureWorldState(worldObjectId);
            return true;
        }

        public void Restore(WorldObjectStateSnapshot snapshot)
        {
            if (!HasValidId || snapshot.ObjectId != worldObjectId)
            {
                return;
            }

            IPersistentWorldState state = GetComponent<IPersistentWorldState>();
            state?.RestoreWorldState(snapshot);
        }
    }
}
