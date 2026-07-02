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
        public static void Resovle(Slot source, Slot Destination, DragContext ctx, bool shiftHeld)
        {

            if (Destination.IsEmpty)
            {
                Debug.Log("Empty");
                Destination.StackOrSwap(source);
                return;
            }
           

            if (shiftHeld == false)
            {
                //Rule 1;
                if (source == Destination)
                {
                    Destination.StackOrSwap(source);
                    Debug.Log("Same");
                    return;
                }

                //
                if (ctx.destinationContainer.IsFixedSize)
                {
                    Destination.StackOrSwap(source);
                    Debug.Log("fixed");
                    return;
                }

                if (ctx.destinationContainer.IsFull)
                {
                    Debug.Log("Full");
                    Destination.StackOrSwap(source);
                    return;
                }
            }
            else // we know that He the keyneeded for Input.
            {
                if(ctx.destinationContainer)

                if (source == Destination)
                {
                    Destination.StackOrSwap(source);
                    Debug.Log("Same");
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

