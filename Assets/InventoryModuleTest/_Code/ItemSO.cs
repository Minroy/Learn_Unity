using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    public string description;
    public string displayName;
    public Sprite icon;
    public int MaxAmount = 128;

    public ItemType ItemType;
    public ItemRarity Rarity;


    public void ChangeItemRarity(ItemRarity rarity)
    {
        Rarity = rarity;
    }
    public void ChangeItemType(ItemType itemType)
    {
       ItemType = itemType;
    }

    public new ItemType GetType()
    {
        return ItemType;
    }

    public ItemRarity GetRarity()
    {
        return Rarity;
    }
}

public enum ItemType
{
    Weapons,Food,Potions,QuestItem,Rewards, CraftingMats,Resources
}

public enum ItemRarity
{
    Uncommon, Common, Rare, Mythic, Legendary
}