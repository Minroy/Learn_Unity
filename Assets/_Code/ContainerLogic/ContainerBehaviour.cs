using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;





#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventoryModule
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class ContainerBehaviour : MonoBehaviour, IContainerIdentifier
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
        [SerializeField] protected InventoryList inventoryList;
        [SerializeField] protected GameObject SlotPrefab;
        [SerializeField] protected int StartSize = 1;
        [SerializeField] protected bool isFixedSize;


        public bool IsFixedSize => isFixedSize;
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
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public virtual void Awake()
        {
            IsRegistered = false;
            inventoryList = GenerateList(isFixedSize, StartSize);


            inventoryList.OnSlotsAdd += AddSlots;
            inventoryList.OnSlotsRemoved += RemoveSlots;

            InventoryManager.Register(this);
        }



        private void OnValidate()
        {
            if (StartSize < 0) Debug.LogWarning("StartSize cannot be a negative number");
            if (SlotPrefab == null)
                return;

            inventoryList ??= GenerateList(isFixedSize, StartSize);
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

        public virtual InventoryList GenerateList(bool isFixedSize, int startSize, int maxSize = -1)
        {
            return new InventoryList(isFixedSize, startSize, maxSize);
        }

        public virtual int AddToContainer(ItemSO itemType, int amount)
        {
            return inventoryList.TryAdd(itemType, amount);
        }

        public virtual int RemoveForContainer(ItemSO itemType, int amount)
        {
            return inventoryList.TryRemove(itemType, amount);
        }


        //Bug : Naming Issue
        // TODO : Fix Later. // LOW
        public async void AddAtIndex(Slot slotData, int index)
        {
            // 1. Insert the underlying slot data structure
            inventoryList.AddAtIndex(slotData, index);

            // 2. Instantiate and visually place the physical UI element first
            GameObject slotObj = Instantiate(SlotPrefab, transform);
            slotObj.transform.SetSiblingIndex(index);
            slotObj.name = $"Slot_{index}s";

            await UniTask.Yield();
            // 3. Now refresh the container so the manager binds the exact indices perfectly
            InventoryManager.RefreshContainer(this);
        }




        /// <summary>
        /// Locate and Find emptySlots and destroy them
        /// </summary>

        /// Bug Satus
        /// Bug 1 : Index was provided out of range.
        /// Solution was, Waiting until destroy has Finished doing it stuff. and executing it next frame. 
        /// Problem duting the Unity Cycle. When trying to call register. It got a cached transform.Values.
        /// resulting in register getting a null ref at the index. cuase destroy didnt update transform. 

        /// Bug 2 : Item Naming is messed up in herieacy is messed up.
        // TODO FIX // LOW

        public async void PruneEmptySlots()
        {
            try
            {
                if (isFixedSize) return;

                bool changed = false;
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    if (i >= inventoryList.Count) continue;

                    Slot slot = inventoryList[i];
                    if (slot.IsEmpty && inventoryList.MinSize < inventoryList.Count)
                    {
                        inventoryList.RemoveAtIndex(i);
                        Destroy(transform.GetChild(i).gameObject);
                        changed = true;
                    }
                }

                if (changed)
                {
                    // Wait for Unity to finish destroying the physical slot transforms 
                    await UniTask.Yield();
                    if (this != null)
                        InventoryManager.RefreshContainer(this);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        /// <summary>
        /// Removes A slot at at index
        /// </summary>
        /// <param name="index"></param>
        public virtual async void RemoveSlotAtIndex(int index)
        {
            if (index < 0 || index > inventoryList.Count) return;

            Debug.Log("calling remove");
            inventoryList.RemoveAtIndex(index);

            await UniTask.Yield();
            if (this != null)
                InventoryManager.RefreshContainer(this);
        }

        public virtual async void RemoveAmountAtIndex(int amountToRemove ,int index)
        {
            if (index < 0 || index > inventoryList.Count) return;

            Debug.Log("calling remove");
            inventoryList.RemoveAmountIndex(amountToRemove,index);

            await UniTask.Yield();
            if (this != null)
                InventoryManager.RefreshContainer(this);
        }

        private void RemoveSlots(int index)
        {
            Destroy(transform.GetChild(index).gameObject);
        }


        #region        // ---------------- GET ACCESS INFO ----------------//

        public Slot GetSlot(int index)
        {
            return inventoryList[index];
        }

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



        #region       // ---------------- SLOT GENERATION During Editor ----------------//

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

            // Runtime: Only dynamic containers can change size
            if (isFixedSize) return;

            // FIX: Safely unbind from the manager BEFORE killing the UI components
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

            if (isFixedSize) return;

            // 1. Bulk-spawn all the new physical layouts first
            int currentCount = transform.childCount;
            for (int i = 0; i < amount; i++)
            {
                GameObject slotObj = Instantiate(SlotPrefab, transform);
                slotObj.name = $"Slot_{currentCount + i}";
            }


            InventoryManager.RefreshContainer(this);
        }
        #endregion
    }
}