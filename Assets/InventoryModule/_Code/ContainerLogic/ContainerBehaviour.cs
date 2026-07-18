#pragma warning disable
using UnityEngine;
using InventoryModule.Windows;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventoryModule
{
    public class ContainerBehaviour : ContainerModule
    {
        public bool ShowInEditor;
        [SerializeField] protected bool isDynamic;
        [SerializeField] protected GameObject SlotPrefab;
        [SerializeField] protected int StartSize = 1;

        public Transform SlotParent
        {
            get
            {
                if (mainViewer != null)
                    return mainViewer.SlotContentsLocation;

                return transform;
            }
            set { }
        }

        protected Viewer mainViewer;

        [Header("Inventory Settings")]
        protected InventoryList inventoryList;

        public bool IsDynamic => isDynamic;
        public InventoryList ContainerList => inventoryList;
        public bool IsFull => inventoryList != null && inventoryList.IsFull;
        public bool IsRegistered { get; private set; }

        public bool HasEmptySlots
        {
            get
            {
                if (inventoryList == null) return false;
                foreach (var slots in inventoryList)
                {
                    if (slots.IsEmpty)
                        return true;
                }
                return false;
            }
        }

        #region OnSpawnedExecutions methods

        public void Awake()
        {
            IsRegistered = false;

            inventoryList ??= GenerateList(isDynamic, StartSize);

            inventoryList.OnSlotsAdd += AddSlots;
            inventoryList.OnSlotsRemoved += RemoveSlot;
            InventoryManager.Register(this);
        }

        public void Start()
        {
            if (mainViewer == null)
                mainViewer = GetComponentInChildren<Viewer>();

            OnActivated();
            Hide();
        }

        protected virtual void OnActivated() { }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // CRITICAL: Unsubscribe first to prevent multiple stacked registrations
            EditorApplication.delayCall -= RunEditorValidate;
            EditorApplication.delayCall += RunEditorValidate;
#endif
        }
        #endregion

#if UNITY_EDITOR
        private void RunEditorValidate()
        {
            if (this == null)
                return;

            // Clear out queue immediately to prevent double-execution loops
            EditorApplication.delayCall -= RunEditorValidate;

            if (StartSize < 0)
                Debug.LogWarning("StartSize cannot be a negative number");

            // Data layer always processes independently
            inventoryList ??= GenerateList(isDynamic, StartSize);

            if (!Application.isPlaying)
            {
                // Re-cache the viewer safely if Unity cleared the reference on editor reload
                if (mainViewer == null)
                {
                    mainViewer = GetComponentInChildren<Viewer>();
                }

                // 1. Process structural layout state first
                GenerateViewerEditor(ShowInEditor);

                // 2. Evaluate physical slot permission rule
                if (UpdateVisuals && SlotPrefab != null)
                {
                    if (CountCurrentSlots() != StartSize)
                    {
                        ResetAndGenerateSlots(StartSize);
                    }
                }
                else
                {
                    // Clean sweep both parent scopes if permission is revoked or prefab missing
                    ClearAllEditorSlots();
                }
            }
        }
#endif

        public virtual InventoryList GenerateList(bool isDynamic, int startSize, int maxSize = -1)
        {
            return new InventoryList(isDynamic, startSize, maxSize);
        }

        public virtual int AddToContainer(ItemSO itemType, int amount)
        {
            return inventoryList.TryAdd(itemType, amount);
        }

        public virtual int RemoveForContainer(ItemSO itemType, int amount)
        {
            return inventoryList.TryRemove(itemType, amount);
        }

        public void AddAtIndex(Slot slotData, int index)
        {
            EnqueTaskAysc(async () =>
            {
                inventoryList.AddAtIndex(slotData, index);

                GameObject slotObj = Instantiate(SlotPrefab, SlotParent);
                slotObj.transform.SetSiblingIndex(index);
                slotObj.name = $"Slot_{index}";
                await Awaitable.NextFrameAsync();

                if (this != null)
                {
                    int childCount = SlotParent.childCount;
                    for (int i = index; i < childCount; i++)
                    {
                        InventoryManager.BindSingleSlot(this, i);
                    }
                }
            });
        }

        #region Removal Logic

        public void PruneEmptySlots()
        {
            EnqueTaskAysc(async () =>
            {
                if (!isDynamic) return;

                bool changed = false;
                int lowestRemovedIndex = int.MaxValue;

                for (int i = SlotParent.childCount - 1; i >= 0; i--)
                {
                    if (i >= inventoryList.Count) continue;

                    Slot slot = inventoryList[i];
                    if (slot.IsEmpty && inventoryList.MinSize < inventoryList.Count)
                    {
                        InventoryManager.UnBindSingleSlot(this, i);
                        inventoryList.DeleteSlot(i);

                        if (UpdateVisuals)
                            Destroy(SlotParent.GetChild(i).gameObject);

                        changed = true;
                        lowestRemovedIndex = Mathf.Min(lowestRemovedIndex, i);
                    }
                }

                if (changed)
                {
                    await Awaitable.NextFrameAsync();
                    if (this != null)
                    {
                        int childCount = SlotParent.childCount;
                        for (int i = lowestRemovedIndex; i < childCount; i++)
                        {
                            InventoryManager.BindSingleSlot(this, i);
                        }
                    }
                }
            });
        }

        public void RemoveSlotAtIndex(int index)
        {
            EnqueTaskAysc(async () =>
            {
                if (index < 0 || index >= inventoryList.Count) return;

                InventoryManager.UnBindSingleSlot(this, index);
                inventoryList.DeleteSlot(index);

                if (UpdateVisuals)
                    Destroy(SlotParent.GetChild(index).gameObject);

                await Awaitable.NextFrameAsync();
                if (this != null)
                {
                    int childCount = SlotParent.childCount;
                    for (int i = index; i < childCount; i++)
                    {
                        InventoryManager.BindSingleSlot(this, i);
                    }
                }
            });
        }

        public void RemoveAmountAtIndex(int amountToRemove, int index)
        {
            EnqueTaskAysc(async () =>
            {
                if (index < 0 || index >= inventoryList.Count) return;

                inventoryList.RemoveAmountIndex(amountToRemove, index);

                await Awaitable.NextFrameAsync();
                if (this != null)
                {
                    InventoryManager.BindSingleSlot(this, index);
                }
            });
        }

        private void RemoveSlot(int index)
        {
            inventoryList.DeleteSlot(index);

            if (UpdateVisuals)
                Destroy(SlotParent.GetChild(index).gameObject);
        }
        #endregion

        #region GET ACCESS INFO

        public Slot GetSlot(int index) => inventoryList[index];

        public void RegistingSatus(bool status)
        {
            IsRegistered = status;
        }

        public void OnDestroy()
        {
#if UNITY_EDITOR
            mainViewer = null;
            if (inventoryList != null)
            {
                inventoryList.OnSlotsAdd -= AddSlots;
                inventoryList.OnSlotsRemoved -= RemoveSlot;
            }
#endif
        }
        #endregion

        #region SLOT GENERATION During Editor

        private int CountCurrentSlots()
        {
            Transform targetParent = SlotParent;
            if (targetParent == null) return 0;

            int count = 0;
            for (int i = 0; i < targetParent.childCount; i++)
            {
                if (targetParent.GetChild(i).name.StartsWith("Slot_"))
                {
                    count++;
                }
            }
            return count;
        }
        private void ResetAndGenerateSlots(int amount)
        {
            ClearAllEditorSlots();

            if (SlotPrefab == null || !UpdateVisuals) return;

            Transform targetParent = SlotParent;
#if UNITY_EDITOR
            for (int i = 0; i < amount; i++)
            {
                GameObject slot = PrefabUtility.InstantiatePrefab(SlotPrefab, targetParent) as GameObject;
                if (slot != null)
                {
                    slot.name = $"Slot_{i}";
                    Undo.RegisterCreatedObjectUndo(slot, "Add Editor Slot");
                }
            }
#endif
        }
        private void ClearAllEditorSlots()
        {
#if UNITY_EDITOR
            // Clean up any loose slots under the root container
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child != null && child.name.StartsWith("Slot_"))
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }

            // Clean up any loose slots inside the active layout viewer
            if (mainViewer != null && mainViewer.SlotContentsLocation != null)
            {
                var viewerParent = mainViewer.SlotContentsLocation;
                for (int i = viewerParent.childCount - 1; i >= 0; i--)
                {
                    var child = viewerParent.GetChild(i);
                    if (child != null && child.name.StartsWith("Slot_"))
                    {
                        Undo.DestroyObjectImmediate(child.gameObject);
                    }
                }
            }
#endif
        }

        public void GenerateSlots(int amount)
        {
            ResetAndGenerateSlots(amount);
        }

        protected void AddSlots(int amount)
        {
            if (!isDynamic) return;

            EnqueTaskAysc(async () =>
            {
                int currentCount = SlotParent.childCount;

                if (UpdateVisuals)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        GameObject slotObj = Instantiate(SlotPrefab, SlotParent.transform);
                        slotObj.name = $"Slot_{currentCount + i}";
                    }
                }

                await Awaitable.NextFrameAsync();
                if (this != null)
                {
                    int finalCount = SlotParent.childCount;
                    for (int i = currentCount; i < finalCount; i++)
                    {
                        InventoryManager.BindSingleSlot(this, i);
                    }
                }
            });
        }
        #endregion

#if UNITY_EDITOR
        protected void GenerateViewerEditor(bool toGenerate)
        {
            try
            {
                if (toGenerate)
                {
                    if (mainViewer == null)
                    {
                        mainViewer = GetComponentInChildren<Viewer>();
                    }

                    if (mainViewer == null)
                    {
                        var viewerChild = new GameObject("Viewer", typeof(Viewer));
                        Undo.RegisterCreatedObjectUndo(viewerChild, "Create Viewer GameObject");
                        mainViewer = viewerChild.GetComponent<Viewer>();
                        mainViewer.transform.SetParent(transform, false);
                    }

                    mainViewer.CreateSlotContainer("SlotContains");

                    // Safely isolate existing slot representations before reparenting
                    System.Collections.Generic.List<Transform> childrenToMove = new System.Collections.Generic.List<Transform>();
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        var child = transform.GetChild(i);
                        if (child != mainViewer.transform && child.name.StartsWith("Slot_"))
                        {
                            childrenToMove.Add(child);
                        }
                    }

                    foreach (var child in childrenToMove)
                    {
                        Undo.SetTransformParent(child, SlotParent, "Reparent Slots to Viewer");
                    }
                }
                else
                {
                    if (mainViewer == null)
                    {
                        mainViewer = GetComponentInChildren<Viewer>();
                    }

                    if (mainViewer == null) return;

                    Transform currentParent = SlotParent;
                    if (currentParent != null && currentParent != transform)
                    {
                        System.Collections.Generic.List<Transform> childrenToRestore = new System.Collections.Generic.List<Transform>();
                        for (int i = 0; i < currentParent.childCount; i++)
                        {
                            var child = currentParent.GetChild(i);
                            if (child.name.StartsWith("Slot_"))
                            {
                                childrenToRestore.Add(child);
                            }
                        }

                        foreach (var child in childrenToRestore)
                        {
                            Undo.SetTransformParent(child, transform, "Restore Slots to Root");
                        }
                    }

                    Undo.DestroyObjectImmediate(mainViewer.gameObject);
                    mainViewer = null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
#endif
    }
}