using TMPro;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshPro))]
    public sealed class FloatingCombatText : MonoBehaviour
    {
        private TextMeshPro text;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private float lifetime = 0.8f;
        private float elapsed;
        private Color startColor = Color.white;

        private void Awake()
        {
            text = GetComponent<TextMeshPro>();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float progress = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;
            float easedProgress = 1f - (1f - progress) * (1f - progress);

            transform.position = Vector3.Lerp(startPosition, endPosition, easedProgress);

            if (text != null)
            {
                Color color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0f, progress);
                text.color = color;
            }

            FaceCamera();

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(string value, Color color, Vector3 worldPosition, Vector3 worldOffset, float duration)
        {
            if (text == null)
            {
                text = GetComponent<TextMeshPro>();
            }

            lifetime = Mathf.Max(0.05f, duration);
            elapsed = 0f;
            startColor = color;
            startPosition = worldPosition;
            endPosition = worldPosition + worldOffset;
            transform.position = startPosition;

            text.text = value;
            text.color = color;
            FaceCamera();
        }

        private void FaceCamera()
        {
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}
