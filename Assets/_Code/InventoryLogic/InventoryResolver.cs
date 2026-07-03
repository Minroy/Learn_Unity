using UnityEngine;

namespace InventoryModule
{
    //Rule 1. If the container its moving into is same then do swaping
    //rule 2. if the inventory is not dynamic so regalar swap
    //rule 3. If the inventory is full. swaporstack.
    //rule 3.5 if inventory is unma

    // rule 4. If dynamice grab the index of the slot you hovered over and add to that index. 
    public static class InventoryResolver
    {
        public static void ResolveAdd(Slot source, Slot Destination, DragContext ctx)
        {

            Debug.Log("shift held " + ctx.IsKeyHeldDown);

            if (Destination.IsEmpty)
            {
                Debug.Log("Empty");
                Destination.StackOrSwap(source);
                return;
            }

            if(ctx.destinationContainer == ctx.SourceContainer)
            {
                Destination.StackOrSwap(source);
                return;
            }

            if (ctx.destinationContainer.IsFull)
            {
                Debug.Log("Full");
                Destination.StackOrSwap(source);
                return;
            }


            if (!ctx.IsKeyHeldDown)
            {
                Destination.StackOrSwap(source);
            }
            else // we know that the keyneeded for Input.
            {

                if (ctx.destinationContainer.HasEmptySlots)
                {
                    ctx.destinationContainer.AddToContainer(source.Item,source.Amount);
                    return;
                }

                if (ctx.destinationContainer.IsFixedSize)
                {
                    Destination.StackOrSwap(source);
                    return;
                }
                // the source container is the same as destinationContainer
                // just add.
                if (ctx.SourceContainer == ctx.destinationContainer)
                {
                    Destination.StackOrSwap(source);
                    Debug.Log("Same Container");
                    return;
                }

                if (ctx.destinationContainer.IsFixedSize)
                {
                    return;
                }

                if (!ctx.destinationContainer.IsFixedSize)
                {
                    Debug.Log(source.Item.displayName);
                    ctx.destinationContainer.AddAtIndex(source, ctx.TargetIndex);
                    source.Clear();
                }
            }

        }
    }
}

