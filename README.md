# Learn_Unity


TODO :

Creation of an InstanceID factory. 

Presistenceid, hndling dupicate id. and Deletion of id, so when new ids are given it doesnt give the old ids to the new item. 
Example. If WoodSo = 12, and the user deletes woodSO and creates a new WoodTypeSO. then the system will give it WoodTypeSO = 12. 
This will override the woodso in the savesystem, syatem will also crash. 

Creating Multiple types of InventoryList.
FixedInventory - Its fixed, accepts any IItem.
FixedInventory<T> - Its fixed, accepts the only given type of IItem, where T is IItem. 
FixedInventory<TSlot> - Its fixed, accepts the any given type of IItem, and works with customSlots.
FixedInventory<T,TSlot> - Its fixed, accepts the only given type of IItem, also will use the Custom Slot made by the Devs, where T is IItem, where Tslot is ISlothander

DynamicInventory - Its can increase and decrease its size. 
(same things as fixedInventory,FixedInventory<T>,FixedInventory<T,TSlot>, FixedInventory<TSlot>) overloads. 

StaticInventory. (debating is needed). 
All its contents are shared. 

MultipleInventory - Holds multiple Fixed/dynamic inventorys internally. (Debating is needed) 

ContainerBehaviour - This makes any Class a Container. A middle men between UI, UNityEngine, and InventoryModule. 
Handles everythings needed for containers. 

Savesystem. (Use of bitwriter, and compression)
Better TaskQueue system. 

InventoryManager (Dont destroy on load, needs MainDisplayer, A ItemAssetBasel, and couple of other settings)
Displayer -  Displayers given contents of the COntainers, inventory. Can Handler multiple or single. and autoSetups itself. 


GITCOMmit issues. 
If devs on diffrent bracnhes make diffrent items. then there is a chance it will get the same ids. even tho there are two diffrent items. 
If two devs without communitication makes the Same item. then that item will get 2 dirrent IDS, resulting in git crash, and id system to fuck up. 







//TODO attributes. 

[IgnoreSave] : Ignores the Type of InventoryModule to Ignore, this while saving. 
[OverrideID] : Lets you write a custom ID, which the ItemIDRegistry system, will ignore. 
[PresistanceID] : Lets you Create New IDs, And the Savesystem will remap the new ids to the old ids. (Will be slow). 
[UseGUID] : Use GUIDs as ItemID, Rather then Uint. (only for this items)

[BypassRestrictions] : Attributes That bypass any restrictions placed by the container, inventory. 
[Ban] : will ban, a perticular item(s), even [BypassRestrictions]. 
[whiteList] : Mark the Inventory to be able to Accept only this types of item. [BypassRestrictions] can bypass this.
[BlackList] : Mark the inventory to ignore this type of items. [BypassRestrictions] can bypass this.
[Dispose] : Mark the Inventory, to dispose after Container, is closed. 
[Random] : filles the inventory with random given items. 
[InternalOnly] : this will make the InventoryItems, to be present only Internally. (example. BagPack to keep track items on first 10 index.)
[Trackable] : Makes the container, trackable betweens scenes. 
[DestroyOnEvent(String)] : will destroy the container on a Given event. 
[StaticOnly] : Makes this inventorys Items as Static. (inventory isnt static, the items inside are).

[ThreadSafeMode] : Makes it Safe for threadings. 
[Refesh] : Refreshes the entire COntainer. (Examples remapping ID form [PresistanceID] )
[Addicted] : Inventory will Always Be Full. (Just a gig of an attribute)