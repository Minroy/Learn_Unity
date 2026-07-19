#pragma warning disable
using InventoryModule.Iterfaces;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    [DefaultExecutionOrder(-1000)]
    public sealed class InventoryManager : MonoBehaviour
    {
        // Thread-safe Singleton Instance for your developers
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private bool Dontdestroyonload;


        // the canvas where the inventoryLogic will work. 
        public static Canvas MainCanvas;

        // this is gonna be used for Displaying the contains in a container. 
        public static ContainerBehaviour MainDisplayerContainer; //Make a special CLass for this



        private static int AddIdentifier = 0;

        public static List<IContainerIdentifier> Containers = new();
        // TODO Low-Medium
        // implemet a ID system for Containers.
        public static void AddIdenfierToContainer()
        {

        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Containers.Clear();
        }



        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;


            if (Dontdestroyonload)
                DontDestroyOnLoad(this);
        }

        #region   RESGISTERING SLOTS
        private static void BindSlots(ContainerBehaviour container)
        {
            if (!container.IsRegistered)
            {
                Debug.Log($"[InventoryManager] Registering: {container.name}");
                int childCount = container.SlotParent.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    if (container.SlotParent.GetChild(i).TryGetComponent<SlotsUI>(out var slotUI))
                    {
                        slotUI.Bind(container, container.GetSlot(i));
                    }
                }
                container.RegistingSatus(true);

                Containers.Add(container);
            }
        }

        private static void UnBindSlots(ContainerBehaviour container)
        {
            // FIX: Only clear if it is actually registered right now
            if (container.IsRegistered)
            {
                Debug.Log($"[InventoryManager] Unregistering: {container.name}");
                int childCount = container.SlotParent.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    if (container.SlotParent.GetChild(i).TryGetComponent<SlotsUI>(out var slotUI))
                    {
                        slotUI.UnBind();
                    }
                }
                container.RegistingSatus(false);
                Containers.Remove(container);
            }
        }

        /// <summary>
        /// OPTIMIZED INCREMENTAL BIND: Binds just one newly added slot instantly without looping the whole container.
        /// </summary>
        public static void BindSingleSlot(ContainerBehaviour container, int index)
        {
            if (index < 0 || index >= container.SlotParent.childCount) return;

            if (container.SlotParent.GetChild(index).TryGetComponent<SlotsUI>(out var slotUI))
            {
                slotUI.Bind(container, container.GetSlot(index));
            }
        }

        /// <summary>
        /// OPTIMIZED INCREMENTAL UNBIND: UnBinds just one newly removed slot instantly without looping the whole container.
        /// </summary>
        public static void UnBindSingleSlot(ContainerBehaviour container, int index)
        {
            if (index < 0 || index >= container.SlotParent.childCount) return;

            if (container.SlotParent.GetChild(index).TryGetComponent<SlotsUI>(out var slotUI))
            {
                slotUI.UnBind();
            }
        }

        /// <summary>
        /// Wipes all current UI bindings and fully re-maps them to the backend data indexes.
        /// </summary>
        public static async void RefreshContainer(ContainerBehaviour container)
        {
            if (container == null) return;

            int childCount = container.SlotParent.childCount;

            // 1. Force unbind all current UI elements to clear dirty states
            for (int i = 0; i < childCount; i++)
            {
                if (container.SlotParent.GetChild(i).TryGetComponent<SlotsUI>(out var slotUI))
                    slotUI.UnBind();
            }

            // Using Unity 6 native Awaitable instead of UniTask.Yield
            await Awaitable.NextFrameAsync();


            // 2. Freshly bind every UI slot to its exact, current data index
            for (int i = 0; i < childCount; i++)
            {
                if (container.SlotParent.GetChild(i).TryGetComponent<SlotsUI>(out var slotUI))
                {
                    slotUI.Bind(container, container.GetSlot(i));
                }
            }

            container.RegistingSatus(true);
        }

        /// <summary>
        /// Registers the Containers with their respective Data data.
        /// </summary>
        public static void Register(params ContainerBehaviour[] containers)
        {
            foreach (var container in containers)
            {
                BindSlots(container);
            }
        }

        /// <summary>
        /// Safely unregisters and clears UI slot tracking from the containers.
        /// </summary>
        public static void UnRegister(params ContainerBehaviour[] containers)
        {
            foreach (var container in containers)
            {
                UnBindSlots(container);
            }
        }
        #endregion


        #region DATA MANIPULATION API

        [MustUseReturnValue]
        [Pure]
        public static int AddItemToContainer(ContainerBehaviour container, ItemSO item, int amount = 1)
        {
            int remaining = container.AddToContainer(item, amount);
            if (remaining > 0)
                Debug.LogWarning($"{container.name} has {remaining} left over items that didn't fit.");
            return remaining;
        }

        [MustUseReturnValue]
        [Pure]
        public static int RemoveItemFromContainer(ContainerBehaviour container, ItemSO item, int amount = 1)
        {
            int remaining = container.RemoveForContainer(item, amount);
            if (remaining > 0)
            {
                Debug.LogWarning($"{container.name} has {remaining} items which couldn't be removed.");
            }
            return remaining;
        }
        #endregion


        #region    GET INFORMATION

        /// <summary>
        ///  Returns the currently hovereved Container
        /// </summary>
        public static ContainerBehaviour GetHoveredContainer()
        {
            return SlotsUI.HoveredContainer;
        }
        public static Slot GetHoveredSlot()
        {
            return SlotsUI.HoveredSlot;
        }
        public static int GetHoverContainer()
        {
            return SlotsUI.HoveredSlotIndex;
        }

        public static void GetHoveredInfo(out ContainerBehaviour Hoveredcontainer, out Slot HoveredSlot)
        {
            Hoveredcontainer = SlotsUI.HoveredContainer;
            HoveredSlot = SlotsUI.HoveredSlot;
        }
        public static void GetHoveredInfo(out ContainerBehaviour Hoveredcontainer, out Slot HoveredSlot, out int HoveredIndex)
        {
            Hoveredcontainer = SlotsUI.HoveredContainer;
            HoveredSlot = SlotsUI.HoveredSlot;
            HoveredIndex = SlotsUI.HoveredSlotIndex;
        }

        #endregion
    }
}