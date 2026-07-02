using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventoryModule
{
    public class SlotsUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
                                          IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {

        private ContainerBehaviour SourceContainer; // the Parent container this slotUI belongs too
        [SerializeField] private Image panel;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Color clickedColor;
        [SerializeField] private Color defaultColor;
        [SerializeField] private float clickFlashDuration;

        [SerializeField] private Image DraggedIcon;

        public static DragContext DragInfo;
        public static int HoveredSlotIndex { get; private set; }
        public static Slot HoveredSlot { get; private set; }
        public static Slot DraggedSlot { get; private set; } // this is the traget slot. 


        private Slot slot;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            HoveredSlot = null;
            DraggedSlot = null;
            HoveredSlotIndex = -1;
            DragInfo = default;
        }

        /// <summary>
        /// Bind with a Corresponding slot. helps to know if slot has changed or not. 
        /// </summary>
        /// <param name="newSlot"></param>
        public void Bind(ContainerBehaviour container,Slot newSlot)
        {
            // 1. Clean up the old slot subscription if it exists
            if (slot != null)
            {
                slot.OnChanged -= Redraw;
            }

            // 2. Always assign the container and the incoming slot data
            SourceContainer = container;
            slot = newSlot;

            // 3. Guard against the backend data being null before subscribing
            if (slot != null)
                slot.OnChanged += Redraw;
            else
                Debug.LogWarning($"[SlotsUI] {gameObject.name} bound to a null Slot data reference.");

            Redraw();
        }

        // unbinds with the slot it is connected to. 
        public void UnBind()
        {
            if (slot != null) slot.OnChanged -= Redraw;
            slot = null;
            Redraw();
        }

        /// <summary>
        /// This Draws the UI of the slot. Depending the data the slot has provided.
        /// </summary>
        private void Redraw()
        {
            panel.enabled = false;
            bool hasItem = slot != null && !slot.IsEmpty;
            icon.enabled = hasItem;
            icon.sprite = hasItem ? slot.Item.icon : null;
            amountText.text = hasItem ? slot.Amount.ToString() : "";
        }

        public void OnPointerClick(PointerEventData eventData) => StartCoroutine(FlashClick());
        public void OnPointerEnter(PointerEventData eventData)
        {
            panel.enabled = true;
            panel.color = defaultColor;
            HoveredSlot = slot;
            HoveredSlotIndex = transform.GetSiblingIndex();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            panel.enabled = false;
            panel.color = defaultColor;
            HoveredSlot = null;
            HoveredSlotIndex = -1;
        }
        private IEnumerator FlashClick()
        {
            panel.color = clickedColor;
            yield return new WaitForSeconds(clickFlashDuration);
            panel.color = defaultColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            panel.enabled = true;
            panel.color = clickedColor;
            HoveredSlot = slot;
            HoveredSlotIndex = transform.GetSiblingIndex();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (slot.IsEmpty)
                return;

            DraggedSlot = slot;

            DragInfo = new DragContext()
            {
                SourceContainer = this.SourceContainer,
                SourceIndex = transform.GetSiblingIndex()
            };


            DragIcon.Instance.Show(slot.Item);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DragIcon.Instance.Move(eventData.position);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (DraggedSlot == null || DraggedSlot == slot)
                return;

            DragInfo.destinationContainer = transform.GetComponentInParent<ContainerBehaviour>();
            DragInfo.TargetIndex = transform.GetSiblingIndex();
            DragInfo.DestinationSlot = slot;

            InventoryResolver.Resovle(DraggedSlot, slot, DragInfo, true);
           

        }



        public void OnEndDrag(PointerEventData eventData)
        {
            DragIcon.Instance.Hide();
            DraggedSlot = null;
            HoveredSlotIndex = -1;
        }
    }
}
