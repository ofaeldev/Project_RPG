using UnityEngine;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    public sealed class InventoryDragIconPresenter
    {
        private RectTransform dragIcon;

        public bool HasIcon => dragIcon != null;

        public void Create(Canvas rootCanvas, InventoryItemStack stack)
        {
            Destroy();

            if (rootCanvas == null || stack == null)
            {
                return;
            }

            var dragObject = new GameObject("InventoryDragIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            dragObject.transform.SetParent(rootCanvas.transform, false);
            dragIcon = (RectTransform)dragObject.transform;
            dragIcon.sizeDelta = new Vector2(54f, 54f);

            Image image = dragObject.GetComponent<Image>();
            image.sprite = stack.Item != null ? stack.Item.Icon : null;
            image.color = image.sprite != null ? Color.white : new Color(0.78f, 0.82f, 0.88f, 0.85f);

            CanvasGroup group = dragObject.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
        }

        public void MoveTo(Vector2 screenPosition)
        {
            if (dragIcon != null)
            {
                dragIcon.position = screenPosition;
            }
        }

        public void Destroy()
        {
            if (dragIcon != null)
            {
                Object.Destroy(dragIcon.gameObject);
                dragIcon = null;
            }
        }
    }
}
