using UnityEngine;

namespace InventoryModule
{
    //Rule 1. If the container its moving into is same then do swaping
    //rule 2. if the inventory is not dynamic so regalar swap
    //rule 3. If the inventory is full. swaporstack.
    //rule 3.5 if inventory is Max or something just do normal stacking or swapping.
    // rule 4. If dynamice grab the index of the slot you hovered over and add to that index. 


    /// <summary>
    /// You shouldn't be Messing around with the Logic presented in here. Its Hard Coded Logic to handle the dynamicInventory. 
    /// </summary>
    public static class InventoryResolver
    {
        public static void ResolveAdd(Slot source, Slot destination, DragContext ctx)
        {
            bool crossContainer = ctx.destinationContainer != ctx.SourceContainer;

            // 1. Same Container -> Always regular swap/stack and stop
            if (!crossContainer)
            {
                destination.StackOrSwap(source);
                ctx.SourceContainer.PruneEmptySlots();
                return;
            }

            // 2. Cross Container, Shift NOT held -> Regular swap/stack and stop
            if (!ctx.IsKeyHeldDown)
            {
                destination.StackOrSwap(source);
                ctx.SourceContainer.PruneEmptySlots();
                ctx.destinationContainer.PruneEmptySlots();
                return;
            }

            // --- BEYOND HERE: CROSS CONTAINER & SHIFT IS HELD ---

            // 3. Target has empty slots -> Fill them up
            if (ctx.destinationContainer.HasEmptySlots)
            {
                //Capture the remainder that didn't fit, and update the source slot!
                int remaining = ctx.destinationContainer.AddToContainer(source.Item, source.Amount);
                source.SetAmount(remaining);
            }
            // 4. Target is Fixed Size and No empty slots available -> Regular Swap/Stack
            else if (ctx.destinationContainer.IsFixedSize)
            {
                destination.StackOrSwap(source);
            }
            // 5. Target is Dynamic No empty slots available -> Insert and Shift Right
            else if (!ctx.destinationContainer.IsFixedSize)
            {
                ctx.destinationContainer.AddAtIndex(source, ctx.TargetIndex);
                source.Clear();
            }


            // Always run your layout pruning at the end of modifications
            ctx.SourceContainer.PruneEmptySlots();
            ctx.destinationContainer.PruneEmptySlots();
        }

        public static void ResolveRemoval(Slot source, Slot Destination, DragContext ctx)
        {

        }
    }
}

