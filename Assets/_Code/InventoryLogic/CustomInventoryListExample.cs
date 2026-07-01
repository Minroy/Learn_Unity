public class CustomInventoryListExample : InventoryList
{
    private ItemRarity AllowRarity;

    public CustomInventoryListExample(ItemRarity rarity, bool isFixedSize, int startSize, int maxSize = -1) : base(isFixedSize, startSize, maxSize)
    {
        AllowRarity = rarity;
    }

    public override int TryAdd(ItemSO item, int amount = 1)
    {
        if (item.GetRarity() != AllowRarity)
            return amount;

       return base.TryAdd(item, amount);
    }
}
