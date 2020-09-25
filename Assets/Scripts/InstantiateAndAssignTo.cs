using Assets.Scripts.Core;
using UnityEngine;

public class InstantiateAndAssignTo : MonoBehaviour
{
    public GameObjectVariable objectToAssignTo;
    public GameObjectVariable parentObject;
    public GameObject prefabToInstantiate;

    public bool onlyIfNull = true;

    public void InstantiateAndAssign()
    {
        if (onlyIfNull && objectToAssignTo.CurrentValue != null)
        {
            return;
        }
        var instantiated = Instantiate(prefabToInstantiate, parentObject.CurrentValue.transform);
        objectToAssignTo.SetValue(instantiated);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
