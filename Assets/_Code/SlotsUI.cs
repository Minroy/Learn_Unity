using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotsUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
                                      IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
{
    [SerializeField] private Image panel;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Color clickedColor;
    [SerializeField] private Color defaultColor;
    [SerializeField] private float clickFlashDuration;

    private bool droppedSuccessfully;
    [SerializeField] private Image DraggedIcon;


    public static Slot HoveredSlot { get; private set; }
    public static Slot DraggedSlot { get; private set; }
    private Slot slot;



    /// <summary>
    /// the one that inputs data
    /// </summary>
    /// <param name="newSlot"></param>
    public void Bind(Slot newSlot)
    {
        if (slot != null) slot.OnChanged -= Redraw;
        slot = newSlot;
        slot.OnChanged += Redraw;
        Redraw();
    }

    public void UnBind()
    {
        if (slot != null) slot.OnChanged -= Redraw;
        slot = null;
        Redraw();
    }


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
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        panel.enabled = false;
        panel.color = defaultColor;
        if (slot != null)
        {
            HoveredSlot = null;
        }
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
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot.IsEmpty)
            return;

        DraggedSlot = slot;
        DragIcon.Instance.Show(slot.Item);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragIcon.Instance.Move(eventData.position);
    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        Debug.Log("DROP FIRED");

        if (DraggedSlot == null)
        {
            Debug.Log("No dragged slot");
            return;
        }

        if (DraggedSlot == slot)
        {
            Debug.Log("Dropped on itself");
            return;
        }

        Debug.Log("Swapping");

        DraggedSlot.StackOrSwap(slot);

        DraggedSlot = null;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (droppedSuccessfully)
        {
            // do nothing
        }

        DragIcon.Instance.Hide();
        DraggedSlot = null;
    }
}
