using UnityEngine;

namespace RPGProject.Systems
{
    public sealed class PlayerPrefsSaveStorage : ISaveStorage
    {
        public void Save(string key, string payload)
        {
            PlayerPrefs.SetString(key, payload);
            PlayerPrefs.Save();
        }

        public bool TryLoad(string key, out string payload)
        {
            payload = string.Empty;
            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }

            payload = PlayerPrefs.GetString(key);
            return !string.IsNullOrWhiteSpace(payload);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
