using Assets.Scripts.ObjectVariables;
using Assets.WorldObjects;
using Assets.WorldObjects.Members.Food;
using UnityEngine;

public class ItemGatherable : MonoBehaviour, IGatherable
{
    public Resource type;
    public Resource GatherableType => type;
    public InventoryReference InventoryGatheredFrom;

    public bool CanGather()
    {
        return InventoryGatheredFrom.CurrentValue.Get(type) > 0;
    }

    public void OnGathered()
    {
        if (InventoryGatheredFrom.CurrentValue.Get(type) > 0)
        {
            Destroy(gameObject);
        }
    }
}
