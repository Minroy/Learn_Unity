using InventoryModule.IDSystem;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;
namespace InventoryModule.UI
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

        public void Show()
        {
            image.enabled = true;
            //image.sprite = item.icon;
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

    /// <summary>
    /// A universal, SlotContext, that all IslotUIHandler Shares.
    /// </summary>
    partial struct UniSlotContext
    {
        public static IContainerIdentifier CurrentContainer;
        public  static IContainerIdentifier DestinationContainer;

        public static ISlotHandler CurrentSlot;
        public static ISlotHandler DestinationSlot;


        public static IItem CurrentItem;
    }
}