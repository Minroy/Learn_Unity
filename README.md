# Learn_Unity

# Contributor Note — Instance Items

## Current Focus

The current focus is designing the **runtime instance system** for inventory items.

The `ItemID` system is already being worked on separately. Do **not** redesign or replace the current `uint ItemID` system while working on instances.

---

## ItemID vs InstanceID

### ItemID

`ItemID` identifies **what type of item something is**.

For example:

```text
Iron Sword → ItemID 123456789
Health Potion → ItemID 987654321
Wood → ItemID 456789123
```

Every copy of the same item type uses the same `ItemID`.

`ItemID` is assigned/baked during editor time and is intended to remain persistent.

---

## InstanceID

`InstanceID` identifies **one specific runtime instance of an item**.

For example, two Iron Swords can have:

```text
Iron Sword
ItemID: 123456789
InstanceID: A
Durability: 40

Iron Sword
ItemID: 123456789
InstanceID: B
Durability: 91
```

They are the same item type, so they share the same `ItemID`.

However, they are different individual objects at runtime, so they have different `InstanceID`s.

The purpose of `InstanceID` is to allow the system to distinguish between individual items when their state is different.

---

## InstanceData

`InstanceData` represents the **data that makes a particular instance different from another instance of the same ItemID**.

For example, an ordinary Iron Sword might only need:

```text
ItemID
```

But an individual sword could have:

```text
Durability
Damage
Enchantments
Custom Name
Upgrades
Modification Data
```

Those values belong to the **instance**, not the general item definition.

Another example:

```text
Arrow ItemID: 123
Instance A:
    Damage = 12
    MaxCapacity = 122

Arrow ItemID: 123
Instance B:
    Damage = 25
    MaxCapacity = 200
```

Both are still the same `ItemID`.

Their instance data is what makes them different.

---

# What Needs To Be Designed

The main unresolved problem is:

> **How should the InventoryModule represent, identify, access, modify, save, load, and communicate instance-specific data?**

This needs to work with systems such as:

* Save System
* Loading
* Inventory operations
* UI
* Tooltips
* Displayers
* Item modification
* Item copying/duplication
* Runtime item creation
* Potential future networking

The design also needs to support instance data of different forms, including things such as:

```text
structs
classes
primitive values
custom developer data
collections
multiple pieces of data
```

Do not assume that instance data will always be one simple struct.

---

## Important Distinction

Keep these concepts separate:

```text
ItemID
    ↓
What item is this?

InstanceID
    ↓
Which individual copy is this?

InstanceData
    ↓
What is different about this individual copy?
```

Example:

```text
                 Iron Sword
                     │
              ┌──────┴──────┐
              │             │
           ItemID        Instance
          123456            │
                     ┌──────┴──────┐
                     │             │
                InstanceID    InstanceData
                    ABC       Durability = 40
                              Damage = 25
                              Name = "Old Sword"
```

---

## Current Status

### Already established

* `ItemID` uses `uint`.
* `ItemID` identifies the item type.
* `ItemID` is editor-generated/baked.
* Runtime instances need their own identity.
* Multiple instances can share the same `ItemID`.
* Instance-specific information must be distinguishable from the base item definition.

### Not finalized

* Exact `InstanceID` lifecycle.
* How instance data is represented.
* How systems discover what instance data an item contains.
* How systems access instance data.
* How instance data is modified.
* How instance data is copied.
* How instance data is serialized.
* How instance data is restored.
* How instance data interacts with the SaveSystem.
* How instance data should be exposed to UI/Tooltips/Displayers.
* How instance data should eventually interact with networking.

**Do not prematurely lock in an implementation.**

The goal right now is to find a clean architecture that can handle the above requirements without making the InventoryModule unnecessarily complicated or restrictive for developers.

This is a inventory Module. Designed to Help devs Make Inventory Systems faster, by providing them tools, services, and ease of life features, that inventory systems Come with. 
Its has many unquie features. 
- Items and automatically Given an ID, and runtime instance-able items also have a runtime ID. 
- Incredibally fast, and Low memory consuming Sytem. 
- Auto handles Large assets, Lag points, and Insanse Object adding to Inventory. 
- custom build Inventory List(s), that behaves like a List. so you can do all sorts of fancy stuff a list can, but more, that revovles around Inventory item Staorage. 
- Fancy Editor tools, help debug, setup, and create rules. 
- Abilty to create your own InventoryList, COntainerbehaviours, Basically anything. and implement it to the Module. it will autoUse your porvived Custom Logic. (excepts include IDsystem namespace)
- Container behaviour, a Custom build Mono, that makes anything a type of Container. etc backpack,chest, hotbars. 
- Build-in Save system. Potimised to Use less space, fast, and lag proof.
- and fancy UI. including a build in displayer, which displayes the canvas.
- Bunch of pre-build fetures you can ship and use. eg. StackorSwap, or ability to Find and remove items. without you needing to write code
- build in events.  
- Generic support for multiple InventoryList connected to one container. 
- Adreessable support. 
- Furture we have plans to explande this to Networking, For netcode, and purrnet. (2 versions)



TODO :

Creation of an InstanceID factory. 

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
Eaxple. MultipleInventory MultiInven<FixedInventory> =  new MultiInven<FixedInventory> { new FixedInventory{ item,item sword}, new FixedInventory{wood,item sword} }

ContainerBehaviour - This makes any Class a Container. A middle men between UI, UNityEngine, and InventoryModule. 
Handles everythings needed for containers. 

Savesystem. (Use of bitwriter, and compression)
Better TaskQueue system. 

InventoryManager (Dont destroy on load, needs MainDisplayer, A ItemAssetBasel, and couple of other settings)
Displayer -  Displayers given contents of the COntainers, inventory. Can Handler multiple or single. and autoSetups itself. 

InstanceID: Create a Override/ regeneration of instanceID at runtime. without SaveSYstem breaking. 







//TODO attributes. 

[IgnoreSave] : Ignores the Type of InventoryModule to Ignore, this while saving. 
[OverrideID] : Lets you write a custom ID, which the ItemIDRegistry system, will ignore. 
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
[Refesh] : Refreshes the entire COntainer. (Examples remapping New UI, with new Slots)
[Addicted] : Inventory will Always Be Full. (Just a gig of an attribute)