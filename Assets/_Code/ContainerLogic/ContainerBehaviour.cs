using UnityEngine;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventoryModule
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class ContainerBehaviour : ContainerModule
    {

        //TODO Medium
        //Hook up logic.to this later, ignore events for now.

        ////public event Action OnContainerOpened;
        ////public event Action OnContainerClosed;
        ////public event Action OnContainerDestroyed;
        ////public event Action OnContainerEnabled;
        ////public event Action OnContainerChanged;
        ////public event Action OnContainerAdded;
        ////public event Action OnContainerRemoved;
        ////public event Action OnContainerSwaped;
        ////public event Action OnContainerFull;




        [Header("Inventory Settings")]
        protected InventoryList inventoryList;
        [SerializeField] protected GameObject SlotPrefab;
        [SerializeField] protected int StartSize = 1;
        [SerializeField] protected bool isDynamic;

        public bool IsDynamic => isDynamic;
        public InventoryList ContainerList => inventoryList;
        public bool IsFull => inventoryList != null && inventoryList.IsFull;
        public bool IsRegistered { get; private set; }

        public bool HasEmptySlots
        {
            get
            {
                foreach (var slots in inventoryList)
                {
                    if (slots.IsEmpty)
                        return true;
                }
                return false;
            }
        }

        public void Awake()
        {
            IsRegistered = false;

            if (inventoryList == null)
                inventoryList = GenerateList(isDynamic, StartSize);

            inventoryList.SetDynamic(isDynamic);

            inventoryList.OnSlotsAdd += AddSlots;
            inventoryList.OnSlotsRemoved += RemoveSlots;

            InventoryManager.Register(this);

            OnActivated();
        }

        /// <summary>
        /// Fires once the Container is Done registering and setting up. (Fired by Awake)
        /// </summary>
        protected virtual void OnActivated() { }

        private void OnValidate()
        {
            if (StartSize < 0) Debug.LogWarning("StartSize cannot be a negative number");
            if (SlotPrefab == null)
                return;

            inventoryList ??= GenerateList(isDynamic, StartSize);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this != null)
                        GenerateSlots(StartSize);
                };
            }
#endif
        }

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

        // ---------------- Structural ops, now routed through the queue ----------------

        public void AddAtIndex(Slot slotData, int index)
        {
            EnqueueContainer(async () =>
            {
                inventoryList.AddAtIndex(slotData, index);

                GameObject slotObj = Instantiate(SlotPrefab, transform);
                slotObj.transform.SetSiblingIndex(index);
                slotObj.name = $"Slot_{index}";

                await Awaitable.NextFrameAsync();

                if (this != null)
                    InventoryManager.RefreshContainer(this); // scoped: only shifted slots need rebinding
            });
        }

        /// <summary>
        /// Locate and Find emptySlots and destroy them
        /// </summary>
        /// 


        public void PruneEmptySlots()
        {
            EnqueueContainer(async () =>
            {
                if (!isDynamic) return;

                bool changed = false;
                int lowestRemoved = int.MaxValue;

                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    if (i >= inventoryList.Count) continue;

                    Slot slot = inventoryList[i];
                    if (slot.IsEmpty && inventoryList.MinSize < inventoryList.Count)
                    {
                        inventoryList.RemoveAtIndex(i);
                        Destroy(transform.GetChild(i).gameObject);
                        changed = true;
                        lowestRemoved = Mathf.Min(lowestRemoved, i);
                    }
                }

                if (changed)
                {
                    await Awaitable.NextFrameAsync();
                    if (this != null)
                        InventoryManager.RefreshContainer(this);
                }
            });
        }

        public void RemoveSlotAtIndex(int index)
        {
            EnqueueContainer(async () =>
            {
                if (index < 0 || index >= inventoryList.Count) return;

                inventoryList.RemoveAtIndex(index);

                await Awaitable.NextFrameAsync();
                if (this != null)
                    InventoryManager.RefreshContainer(this);
            });
        }

        public void RemoveAmountAtIndex(int amountToRemove, int index)
        {
            EnqueueContainer(async () =>
            {
                if (index < 0 || index >= inventoryList.Count) return;

                inventoryList.RemoveAmountIndex(amountToRemove, index);

                await Awaitable.NextFrameAsync();
                if (this != null)
                    InventoryManager.RefreshContainer(this);
            });
        }

        private void RemoveSlots(int index)
        {
            Destroy(transform.GetChild(index).gameObject);
        }

        #region GET ACCESS INFO

        public Slot GetSlot(int index) => inventoryList[index];

        public void RegistingSatus(bool status)
        {
            IsRegistered = status;
        }

        public void OnDestroy()
        {
            inventoryList.OnSlotsAdd -= AddSlots;
            inventoryList.OnSlotsRemoved -= RemoveSlots;
        }
        #endregion

        #region SLOT GENERATION During Editor

        public void GenerateSlots(int amount)
        {
            RemoveSlots();
            AddSlots(amount);
        }

        private void RemoveSlots()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                    Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);
                return;
            }
#endif
            if (!isDynamic) return;

            InventoryManager.UnRegister(this);

            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }

        protected void AddSlots(int amount)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject slot = PrefabUtility.InstantiatePrefab(SlotPrefab, transform) as GameObject;
                    slot.name = $"Slot_{i}";
                    Undo.RegisterCreatedObjectUndo(slot, "Add Slot");
                }
                return;
            }
#endif
            if (!isDynamic) return;

            EnqueueContainer(async () =>
            {
                int currentCount = transform.childCount;
                for (int i = 0; i < amount; i++)
                {
                    GameObject slotObj = Instantiate(SlotPrefab, transform);
                    slotObj.name = $"Slot_{currentCount + i}";
                }

                await Awaitable.NextFrameAsync();
                if (this != null)
                    InventoryManager.RefreshContainer(this); // only the newly added slots need binding
            });
        }

        #endregion

        #region Container TransferHandler

        public ContainerBehaviour QuickTransferTo { get; private set; }

        public void SetTransferTo(ContainerBehaviour containerToTransfer)
        {
            QuickTransferTo = containerToTransfer;
        }

        #endregion
    }
}