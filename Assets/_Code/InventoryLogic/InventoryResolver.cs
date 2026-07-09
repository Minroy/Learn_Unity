namespace InventoryModule
{
    /// <summary>
    /// You shouldn't be Messing around with the Logic presented in here. Its Hard Coded Logic to handle the dynamicInventory. 
    /// And other logic. Optionally you can open and check. 
    /// </summary>
    public static class InventoryResolver
    {
        //Set of rules It Takes
        public static void ResolveAdd(Slot source, Slot destination, SlotContext ctx)
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
            // 4. if Traget is full, do regualarStackorswap. 
            else if (ctx.destinationContainer.IsDynamic)
            {
                destination.StackOrSwap(source);
            }
            // 5. Target is Dynamic No empty slots available -> Insert and Shift Right
            else if (!ctx.destinationContainer.IsDynamic)
            {
                ctx.destinationContainer.AddAtIndex(source, ctx.TargetIndex);
                source.Clear();
            }


            // Always run your layout pruning at the end of modifications
            ctx.SourceContainer.PruneEmptySlots();
            ctx.destinationContainer.PruneEmptySlots();
        }


        public static void ResolveRemoval(Slot source, Slot Destination, SlotContext ctx)
        {

        }



        public static void ResolveQuickSwapping(Slot source, SlotContext ctx)
        {
            // Assign destination target via your custom QuickTransferTo property
            ctx.destinationContainer = ctx.SourceContainer.QuickTransferTo;

            // Guard clause: if no target is assigned by devs, halt execution safely
            if (ctx.destinationContainer == null)
            {
                return;
            }

            // --- Rule 1: Let AddToContainer handle the data layer transaction ---
            int originalAmount = source.Amount;
            int remaining = ctx.destinationContainer.AddToContainer(source.Item, originalAmount);

            // --- Rule 2 & 3: Source is Dynamic logic ---
            if (ctx.SourceContainer.IsDynamic)
            {
                int itemsMoved = originalAmount - remaining;

                if (itemsMoved > 0)
                {
                    // If everything or a partial stack moved, deduct it
                    source.SlotRemove(itemsMoved);
                }

                // Rule 2: If it fully emptied out, wipe the slot clean
                if (source.IsEmpty)
                {
                    source.Clear();
                }

                // Rule 3: If destination was full/hit MaxSize, 'remaining' stays untouched 
                // in the source slot automatically because we only remove what actually moved.
            }
            // --- Rule 4: Source is Not Dynamic (Fixed Size) logic ---
            else if (!ctx.SourceContainer.IsDynamic)
            {
                int itemsMoved = originalAmount - remaining;
                if (itemsMoved > 0)
                {
                    source.SlotRemove(itemsMoved); // Leaves the leftover item amount in the source slot
                }
            }

            // Always keep both layout UI screens optimized and clean
            ctx.SourceContainer.PruneEmptySlots();
            ctx.destinationContainer.PruneEmptySlots();
        }
    }
}

