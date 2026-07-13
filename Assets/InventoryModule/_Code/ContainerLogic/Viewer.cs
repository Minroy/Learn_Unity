using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule
{
    public class Viewer : MonoBehaviour
    {
        [SerializeField] string ViewerName = "Veiwer";
        [SerializeField] string childname = "childname";
        private bool IsReady;

        private Canvas _canvas;
        private CanvasScaler _scaler;
        private GraphicRaycaster _raycaster;
        private CanvasGroup _canvasGroup;


        public Canvas Canvas => _canvas;
        public CanvasScaler Scaler => _scaler;
        public GraphicRaycaster Raycaster => _raycaster;
        public CanvasGroup CanvasGroup => _canvasGroup;


        [SerializeField] private Transform _childTransform;
        public Transform ChildTransform => _childTransform;

        private void Awake()
        {
            SetupViewer();
        }


        private void OnValidate()
        {
            
            if(ViewerName == null || ViewerName.Length == 0)
            {
                gameObject.name = "Veiwer";
            }
            else
            {
                gameObject.name = ViewerName;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this != null)
                        SetupViewer();
                };
            }
#endif
        }


        private void SetupViewer()
        {
            if (IsReady)
                return;


            // Canvas
            if (!TryGetComponent(out _canvas))
            {
                _canvas = gameObject.AddComponent<Canvas>();
            }

            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.overrideSorting = true;


            // Canvas Scaler
            if (!TryGetComponent(out _scaler))
            {
                _scaler = gameObject.AddComponent<CanvasScaler>();
            }


            // Graphic Raycaster
            if (!TryGetComponent(out _raycaster))
            {
                _raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }


            // Canvas Group
            if (!TryGetComponent(out _canvasGroup))
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }


            IsReady = true;
            _ = CreateView(childname);
        }


        public Transform CreateView(string viewName)
        {
            GameObject view = new GameObject(
                viewName,
                typeof(RectTransform),
                typeof(GridLayoutGroup)
            );


            view.transform.SetParent(transform, false);


            RectTransform rect = view.GetComponent<RectTransform>();

            _childTransform = view.transform.GetChild(0);
            return _childTransform;
        }
    }
}