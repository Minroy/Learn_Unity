using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventoryModule
{
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



        private bool isCreated = false;

        [Header("Inventory Settings")]
        protected InventoryList inventoryList;
        [SerializeField] protected GameObject SlotPrefab;
        [SerializeField] protected int StartSize = 1;
        [SerializeField] protected bool isDynamic;

        public bool IsDynamic => isDynamic;
        public InventoryList ContainerList => inventoryList;
        public bool IsFull => inventoryList != null && inventoryList.IsFull;
        public bool IsRegistered { get; private set; }

        //TODO: Fix caching problem. LOW.
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


        #region OnSpawnedExecutions methods

        public void Awake()
        {
            IsRegistered = false;

            inventoryList ??= GenerateList(isDynamic, StartSize);

            inventoryList.OnSlotsAdd += AddSlots;
            inventoryList.OnSlotsRemoved += RemoveSlot;
            InventoryManager.Register(this);

            OnActivated();
        }



        public void Start()
        {
            Hide();
        }



        /// <summary>
        /// Fires once the Container is Done registering and setting up. (Fired by Awake)
        /// </summary>
        protected virtual void OnActivated() { }

        #endregion


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



        /// <summary>
        /// Generates a inventoryList. (one time thing)
        /// </summary>
        /// <returns></returns>
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

                GameObject slotObj = Instantiate(SlotPrefab, gameObject.transform);
                slotObj.transform.SetSiblingIndex(index);
                slotObj.name = $"Slot_{index}";

                await Awaitable.NextFrameAsync();

                if (this != null)
                {
                    // Everything from the shifted index down to the new end needs to be rebound
                    int childCount = gameObject.transform.childCount;
                    for (int i = index; i < childCount; i++)
                    {
                        gameObject.transform.GetChild(i).name = $"Slot_{i}";
                        InventoryManager.BindSingleSlot(this, i);
                    }
                }
            });
        }



        #region Removale Logic

        /// <summary>
        /// Locate and Find emptySlots and destroy them
        /// </summary>
        public void PruneEmptySlots()
        {
            EnqueueContainer(async () =>
            {
                if (!isDynamic) return;

                bool changed = false;
                int lowestRemovedIndex = int.MaxValue;

                for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
                {
                    if (i >= inventoryList.Count) continue;

                    Slot slot = inventoryList[i];
                    if (slot.IsEmpty && inventoryList.MinSize < inventoryList.Count)
                    {
                        // Unbind immediately before destroying to prevent dangling reference issues
                        InventoryManager.UnBindSingleSlot(this, i);

                        inventoryList.RemoveAtIndex(i);
                        Destroy(gameObject.transform.GetChild(i).gameObject);

                        changed = true;
                        lowestRemovedIndex = Mathf.Min(lowestRemovedIndex, i);
                    }
                }

                if (changed)
                {
                    await Awaitable.NextFrameAsync();
                    if (this != null)
                    {
                        // Only rebind the elements that shifted upwards from the lowest deleted point
                        int childCount = gameObject.transform.childCount;
                        for (int i = lowestRemovedIndex; i < childCount; i++)
                        {
                            gameObject.transform.GetChild(i).name = $"Slot_{i}";
                            InventoryManager.BindSingleSlot(this, i);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Removes a Slot at index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveSlotAtIndex(int index)
        {
            EnqueueContainer(async () =>
            {
                if (index < 0 || index >= inventoryList.Count) return;

                InventoryManager.UnBindSingleSlot(this, index);
                inventoryList.RemoveAtIndex(index);
                Destroy(gameObject.transform.GetChild(index).gameObject);

                await Awaitable.NextFrameAsync();
                if (this != null)
                {
                    // Rebind shifted remaining slots from the removed position down
                    int childCount = gameObject.transform.childCount;
                    for (int i = index; i < childCount; i++)
                    {
                        gameObject.transform.GetChild(i).name = $"Slot_{i}";
                        InventoryManager.BindSingleSlot(this, i);
                    }
                }
            });
        }

        /// <summary>
        /// Removes a set amount at the slot. clears if its empty.
        /// </summary>
        /// <param name="amountToRemove"></param>
        /// <param name="index"></param>
        public void RemoveAmountAtIndex(int amountToRemove, int index)
        {
            EnqueueContainer(async () =>
            {
                if (index < 0 || index >= inventoryList.Count) return;

                inventoryList.RemoveAmountIndex(amountToRemove, index);

                await Awaitable.NextFrameAsync();
                if (this != null)
                {
                    // Quantity changes don't alter indices or child hierarchies, just rebind the target slot
                    InventoryManager.BindSingleSlot(this, index);
                }
            });
        }

        private void RemoveSlot(int index)
        {
            Destroy(transform.GetChild(index).gameObject);
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
            inventoryList.OnSlotsAdd -= AddSlots;
            inventoryList.OnSlotsRemoved -= RemoveSlot;
        }
        #endregion





        #region SLOT GENERATION During Editor

        public void GenerateSlots(int amount)
        {
            RemoveSlots();
            AddSlots(amount);
        }

        //remove slots form the Gameobject
        private void RemoveSlots()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
                    Undo.DestroyObjectImmediate(gameObject.transform.GetChild(i).gameObject);
                return;
            }
#endif
            if (!isDynamic) return;

            InventoryManager.UnRegister(this);

            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
                Destroy(gameObject.transform.GetChild(i).gameObject);
        }


        //Adds slots to the Gameobjects
        protected void AddSlots(int amount)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject slot = PrefabUtility.InstantiatePrefab(SlotPrefab, gameObject.transform) as GameObject;
                    slot.name = $"Slot_{i}";
                    Undo.RegisterCreatedObjectUndo(slot, "Add Slot");
                }
                return;
            }
#endif
            if (!isDynamic) return;

            EnqueueContainer(async () =>
            {
                int currentCount = gameObject.transform.childCount;
                for (int i = 0; i < amount; i++)
                {
                    GameObject slotObj = Instantiate(SlotPrefab, gameObject.transform.transform);
                    slotObj.name = $"Slot_{currentCount + i}";
                }

                await Awaitable.NextFrameAsync();
                if (this != null)
                {
                    // Bind only the newly created tail elements
                    int finalCount = gameObject.transform.childCount;
                    for (int i = currentCount; i < finalCount; i++)
                    {
                        InventoryManager.BindSingleSlot(this, i);
                    }
                }
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

        #region Generate DisplayComponents
    

        #endregion
    }
}