namespace Assets.WorldObjects.SaveObjects
{
    public interface ISaveable<T>
    {
        T GetSaveObject();
        void SetupFromSaveObject(T save);
    }
}
