namespace InventoryModule
{
    //Rule 1. If the container its moving into is same then do swaping
    //rule 2. if the inventory is not dynamic so regalar swap
    //rule 3. If the inventory is full. swaporstack.

    // rule 4. If dynamice grab the index of the slot you hovered over and add to that index. 
    public static class InventoryResolver
    {
        public static void Resovle(Slot source, Slot Destination, DragContext ctx, bool shiftHeld)
        {

            if (shiftHeld == false)
            {
                if (Destination.IsEmpty)
                {
                    Destination.StackOrSwap(source);
                }
                //Rule 1;
                if (source == Destination)
                {
                    Destination.StackOrSwap(source);
                    return;
                }

                if (ctx.destinationContainer.IsFixedSize)
                {
                    Destination.StackOrSwap(source);
                    return;
                }

                if (ctx.destinationContainer.IsFull)
                {
                    Destination.StackOrSwap(source);
                    return;
                }
            }

        }


    }
}

