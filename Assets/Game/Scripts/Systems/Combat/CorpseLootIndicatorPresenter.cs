using RPGProject.Gameplay;
using TMPro;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(CorpseLootSource))]
    public sealed class CorpseLootIndicatorPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private HealthComponent health;

        [SerializeField]
        private CorpseLootSource lootSource;

        [Header("Loot Marker")]
        [SerializeField]
        private string markerText = "*";

        [SerializeField]
        private Vector3 markerOffset = new(0f, 0.72f, 0f);

        [SerializeField]
        [Min(0.1f)]
        private float markerFontSize = 2.4f;

        [SerializeField]
        private Color markerColor = new(1f, 0.86f, 0.28f, 1f);

        [SerializeField]
        [Min(0.1f)]
        private float pulseSpeed = 5f;

        [SerializeField]
        [Min(0f)]
        private float pulseHeight = 0.05f;

        private TextMeshPro marker;

        public bool IsMarkerVisible => marker != null && marker.gameObject.activeSelf;

        private void Awake()
        {
            ResolveReferences();
            EnsureMarker();
        }

        private void OnEnable()
        {
            ResolveReferences();
            EnsureMarker();

            if (health != null)
            {
                health.Died += OnHealthStateChanged;
                health.Revived += OnHealthStateChanged;
            }

            RefreshMarker();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnHealthStateChanged;
                health.Revived -= OnHealthStateChanged;
            }

            HideMarker();
        }

        private void LateUpdate()
        {
            RefreshMarker();
            UpdateMarkerPosition();
        }

        private void OnHealthStateChanged(HealthChange change)
        {
            RefreshMarker();
        }

        private void RefreshMarker()
        {
            EnsureMarker();
            if (marker == null)
            {
                return;
            }

            bool shouldShow = health != null && health.IsDead && LootSourceAvailability.HasAvailableLoot(lootSource);
            marker.gameObject.SetActive(shouldShow);
            if (!shouldShow)
            {
                return;
            }

            marker.text = markerText;
            marker.color = markerColor;
            marker.fontSize = markerFontSize;
        }

        private void UpdateMarkerPosition()
        {
            if (marker == null || !marker.gameObject.activeSelf)
            {
                return;
            }

            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseHeight;
            marker.transform.position = transform.position + markerOffset + Vector3.up * pulse;
        }

        private void HideMarker()
        {
            if (marker != null)
            {
                marker.gameObject.SetActive(false);
            }
        }

        private void ResolveReferences()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            if (lootSource == null)
            {
                lootSource = GetComponent<CorpseLootSource>();
            }
        }

        private void EnsureMarker()
        {
            if (marker != null)
            {
                return;
            }

            GameObject markerObject = new("CorpseLootIndicator", typeof(TextMeshPro));
            markerObject.transform.SetParent(transform, true);
            marker = markerObject.GetComponent<TextMeshPro>();
            marker.alignment = TextAlignmentOptions.Center;
            marker.fontStyle = FontStyles.Bold;
            marker.sortingOrder = 84;
            marker.textWrappingMode = TextWrappingModes.NoWrap;
            markerObject.SetActive(false);
        }

    }
}
