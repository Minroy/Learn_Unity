public enum ContainerStatus : ushort
{
    None,
    Open,
    Closed,
}





namespace InventoryModule.Iterfaces
{
    public interface IContainerIdentifier
    {

    }

    public interface IPriorityQueue
    {

    }

    public interface IIgnoreContainer
    {
        void IgnoreContainer(bool ignore);
        bool IsIgnoreContainer(bool ignore);
    }

    public interface IPriority
    {
        
        void Priority(int priority);
        int PriorityRank();
        
    }

    public interface ICurrentStatus
    {
        void SetCurrentStatus(ContainerStatus status);
        ContainerStatus GetContainerStatus();
    }
}
