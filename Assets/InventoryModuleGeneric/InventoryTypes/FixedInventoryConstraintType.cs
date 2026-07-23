using InventoryModule.IDSystem;

namespace InventoryModule
{
    /// <summary>
    /// A fixed Inventory, thats doesnt change Size. <typeparamref name="TItem"/> Implments IItemData
    /// </summary>
    public class FixedInventory<TItem> : FixedInventory where TItem : IItemData
    {
        public FixedInventory(int startSize) : base(startSize)
        {
            

        }
    }
}
