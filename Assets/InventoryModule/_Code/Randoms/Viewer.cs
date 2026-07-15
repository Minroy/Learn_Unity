using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule.Windows
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasScaler))]
    public class Viewer : MonoBehaviour
    {
        private GameObject slotContainer;

        public string viewerName;
        public string SlotContains;


        public Canvas canvas;

        public Transform SlotContentsLocation
        {
            get
            {
                if (slotContainer == null)
                    CreateSlotContainer(SlotContains);

                return slotContainer.transform;
            }
        }


        private void Awake()
        {
            if (slotContainer == null)
                CreateSlotContainer(SlotContains);
            
        }

        private void OnDestroy()
        {

        }

        private void OnValidate()
        {
            gameObject.name = viewerName;
            if (slotContainer != null)
                slotContainer.name = SlotContains;

#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && slotContainer == null)
                    CreateSlotContainer("SlotContains");
            };
#endif
        }

        public void CreateSlotContainer(string Name)
        {
            canvas = GetComponent<Canvas>();
            gameObject.name = "Viewer";
            if (slotContainer != null)
                return;

            if (string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name))
                Name = nameof(SlotContains);


            slotContainer = new GameObject(
                Name,
                typeof(RectTransform),
                typeof(GridLayoutGroup),
                typeof(CanvasGroup)
            );


            slotContainer.transform.SetParent(transform, false);


            RectTransform rect = slotContainer.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }
}