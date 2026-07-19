#pragma warning disable
using InventoryModule.Generics.Iterfaces;
using System;

namespace InventoryModule.Generics.Data {
	public struct Slots : ISlotHandler {	
		public int Amount {
			get;
			set;
		}
		public IItemData Item {
			get;
			set;
		}
		public bool IsEmpty {
			get {
				return (Item == null);
			}
		}
		public int SpaceLeft {
			get {
				return ((Item == null) ? 0 : (Item.MaxAmount - Amount));
			}
		}
		public bool IsFull {
			get {
				return ((Item != null) && (Item.MaxAmount == Amount));
			}
		}
		
		public int Add(IItemData item, int amountToAdd) {
			if((item == null)) {
				throw new ArgumentNullException("item");
			}
			if((amountToAdd < 0)) {
				throw new ArgumentOutOfRangeException("amountToAdd", "Cannot add negative amounts");
			}
			//If slot is empty, initialize it with this item type
			if(IsEmpty) {
				Item = item;
				Amount = 0;
			}
			 else if((Item.Id != item.Id)) {
				return amountToAdd;
			}
			//Calculate exactly what can fit using integer math
			var toAdd = Math.Min(amountToAdd, SpaceLeft);
			Amount += toAdd;
			//Return the remainder that couldn't fit
			return (amountToAdd - toAdd);
		}

		public void Clear() {
			Item = null;
			Amount = 0;
		}

		public IItemData GetData() {
			return Item;
		}

		public int Remove(int amountToRemove) {
			if((amountToRemove < 0)) {
				throw new ArgumentOutOfRangeException("amountToRemove");
			}
			if(IsEmpty) {
				return 0;
			}
			var taken = Math.Min(amountToRemove, Amount);
			Amount -= taken;
			if((Amount <= 0)) {
				Clear();
			}
			return taken;
		}

		public void SetData(IItemData item) {
			if((item == null)) {
				throw new ArgumentNullException("item");
			}
		}
	}

}
