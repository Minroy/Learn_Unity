using UnityEngine;
using InventoryModule.Windows;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventoryModule
{
    public class SlotGenerator : ContainerModule
    {
        [SerializeField] private GameObject slotPrefab;

        private ContainerBehaviour container;
        private Viewer viewer;

        [SerializeField] private Transform slotLocation;

        public Transform SlotRoot
        {
            get
            {
                if (slotLocation != null)
                    return slotLocation;

                if (viewer != null)
                    return viewer.SlotContentsLocation;

                return transform;
            }
        }

        public void Initialize(ContainerBehaviour owner, Viewer ownerViewer)
        {
            container = owner;
            viewer = ownerViewer;
        }

        public void AddAtIndex(int index)
        {
            if (!UpdateVisuals)
                return;

            GameObject slot = Instantiate(slotPrefab, SlotRoot);
            slot.transform.SetSiblingIndex(index);
            slot.name = $"Slot_{index}";
        }

        public void AddSlots(int amount)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject slot =
                        PrefabUtility.InstantiatePrefab(slotPrefab, SlotRoot) as GameObject;

                    slot.name = $"Slot_{SlotRoot.childCount - 1}";
                    Undo.RegisterCreatedObjectUndo(slot, "Create Slot");
                }

                return;
            }
#endif

            if (!UpdateVisuals)
                return;

            int start = SlotRoot.childCount;

            for (int i = 0; i < amount; i++)
            {
                GameObject slot = Instantiate(slotPrefab, SlotRoot);
                slot.name = $"Slot_{start + i}";
            }
        }

        public void RemoveSlot(int index)
        {
            if (index >= SlotRoot.childCount)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(SlotRoot.GetChild(index).gameObject);
                return;
            }
#endif

            Destroy(SlotRoot.GetChild(index).gameObject);
        }

        public void RemoveAll()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                while (SlotRoot.childCount > 0)
                    Undo.DestroyObjectImmediate(SlotRoot.GetChild(0).gameObject);

                return;
            }
#endif

            while (SlotRoot.childCount > 0)
                Destroy(SlotRoot.GetChild(0).gameObject);
        }

#if UNITY_EDITOR


        public void GenerateSlots(int amount)
        {
            RemoveAll();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject slot = PrefabUtility.InstantiatePrefab(slotPrefab, SlotRoot) as GameObject;

                    slot.name = $"Slot_{i}";
                    Undo.RegisterCreatedObjectUndo(slot, "Create Slot");
                }

                return;
            }
#endif

            for (int i = 0; i < amount; i++)
            {
                GameObject slot = Instantiate(slotPrefab, SlotRoot);
                slot.name = $"Slot_{i}";
            }
        }


        public void GenerateViewer(bool enabled)
        {
            if (container == null)
                return;

            if (enabled)
            {
                if (viewer != null)
                    return;

                GameObject go = new GameObject("Viewer", typeof(Viewer));

                viewer = go.GetComponent<Viewer>();
                viewer.CreateSlotContainer("SlotContents");
                viewer.transform.SetParent(container.transform, false);

                while (container.transform.childCount > 1)
                {
                    Transform child = container.transform.GetChild(0);

                    if (child == viewer.transform)
                    {
                        child.SetAsLastSibling();
                        continue;
                    }

                    child.SetParent(viewer.SlotContentsLocation, false);
                }
            }
            else
            {
                if (viewer == null)
                    return;

                while (viewer.SlotContentsLocation.childCount > 0)
                    viewer.SlotContentsLocation.GetChild(0).SetParent(container.transform);

                Undo.DestroyObjectImmediate(viewer.gameObject);

                viewer = null;
            }
        }

#endif
    }
}