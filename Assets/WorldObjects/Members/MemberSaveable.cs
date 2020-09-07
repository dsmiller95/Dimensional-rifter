using Assets.WorldObjects.SaveObjects;

namespace Assets.WorldObjects.Members
{
    public interface IMemberSaveable : ISaveable<object>
    {
        string IdentifierInsideMember();
    }
}
