namespace Assets.WorldObjects.Members.Food
{

    public interface IGatherable
    {
        bool CanGather();
        void OnGathered();
        Resource GatherableType { get; }
    }
}
