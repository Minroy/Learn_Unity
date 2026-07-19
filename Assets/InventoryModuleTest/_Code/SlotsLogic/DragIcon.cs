using UnityEngine;
using UnityEngine.UI;
namespace InventoryModule
{
    public class DragIcon : MonoBehaviour
    {
        [SerializeField] private Image image;
        public static DragIcon Instance;

        public void Awake()
        {
            image.enabled = false;
            Instance = this;
        }

        public void Show(ItemSO item)
        {
            image.enabled = true;
            image.sprite = item.icon;
        }

        public void Hide()
        {

            image.enabled = false;
        }

        public void Move(Vector2 position)
        {
            transform.position = position;
        }


    }

    public struct SlotContext
    {
        public ContainerBehaviour destinationContainer; // the container you are Moving into
        public ContainerBehaviour SourceContainer; // the container you Reside in; 

        public int SourceIndex;
        public int TargetIndex;

        public Slot DestinationSlot;
        public bool IsKeyHeldDown;
    }
}