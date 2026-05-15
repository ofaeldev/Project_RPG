using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class CombatFollowToggleUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private AutoAttackController autoAttackController;

        [SerializeField]
        private Toggle followToggle;

        [SerializeField]
        private TMP_Text labelText;

        [Header("Text")]
        [SerializeField]
        private string enabledLabel = "Follow: ON";

        [SerializeField]
        private string disabledLabel = "Follow: OFF";

        private void Awake()
        {
            ResolveAutoAttackController();
            ApplyStateToUI();
        }

        private void OnEnable()
        {
            if (followToggle != null)
            {
                followToggle.onValueChanged.AddListener(OnFollowToggleChanged);
            }

            ApplyStateToUI();
        }

        private void OnDisable()
        {
            if (followToggle != null)
            {
                followToggle.onValueChanged.RemoveListener(OnFollowToggleChanged);
            }
        }

        private void ResolveAutoAttackController()
        {
            if (autoAttackController != null)
            {
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                autoAttackController = player.GetComponent<AutoAttackController>();
            }
        }

        private void ApplyStateToUI()
        {
            ResolveAutoAttackController();
            bool followsTarget = autoAttackController == null || autoAttackController.FollowTarget;

            if (followToggle != null)
            {
                followToggle.SetIsOnWithoutNotify(followsTarget);
            }

            UpdateLabel(followsTarget);
        }

        private void OnFollowToggleChanged(bool shouldFollow)
        {
            ResolveAutoAttackController();
            autoAttackController?.SetFollowTarget(shouldFollow);
            UpdateLabel(shouldFollow);
        }

        private void UpdateLabel(bool shouldFollow)
        {
            if (labelText != null)
            {
                labelText.text = shouldFollow ? enabledLabel : disabledLabel;
            }
        }
    }
}
