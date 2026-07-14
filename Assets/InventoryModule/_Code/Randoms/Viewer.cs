#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule.Windows
{
    [RequireComponent(typeof(GraphicRaycaster), typeof(CanvasScaler))]
    public class Viewer : MonoBehaviour
    {
        public string ViewerName = "Viewer";
        public string SlotContainerName = "SlotContainer";

        private Canvas Canvas;

        [SerializeField]
        private GameObject slotContainer;

        public GameObject SlotContainer => slotContainer;

        public Transform SlotContentsLocation
        {
            get
            {
                if (slotContainer == null)
                    CreateSlotContainer();

                return slotContainer.transform;
            }
        }

        public bool IsReady { get; private set; }


        private void Awake()
        {
            Canvas = GetComponent<Canvas>();

            CreateSlotContainer();

            IsReady = true;
        }

        private void OnValidate()
        {
            gameObject.name = ViewerName;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this != null)
                        CreateSlotContainer();
                };
            }
#endif
        }

        public void CreateSlotContainer()
        {
            if (slotContainer != null)
                return;

            Transform existing = transform.Find(SlotContainerName);

            if (existing != null)
            {
                slotContainer = existing.gameObject;
                return;
            }

            slotContainer = new GameObject(SlotContainerName,typeof(RectTransform),
                typeof(GridLayoutGroup),typeof(CanvasGroup));


            slotContainer.transform.SetParent(transform, false);
        }
    }
}