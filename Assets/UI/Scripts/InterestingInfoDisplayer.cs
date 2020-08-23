using Assets.Scripts.Core;
using Assets.WorldObjects.Members;
using System.Linq;
using TMPro;
using UnityEngine;

public class InterestingInfoDisplayer : MonoBehaviour
{
    public GameObject defaultObject;
    public GameObjectVariable objectToDisplay;

    public TextMeshProUGUI text;


    public void DisplayInfoForObject(GameObject obj)
    {
        if(!obj || obj == null)
        {
            text.text = "Nothing selected";
            return;
        }
        var interestingBits = obj.GetComponentsInChildren<IInterestingInfo>()
            .Select(x => x.GetCurrentInfo());
        if (!interestingBits.Any())
        {
            text.text = "No info";
            return;
        }

        var info = interestingBits.Aggregate((a, b) => a + "-------\n" + b);
        text.text = info;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(objectToDisplay.CurrentValue == null)
        {
            DisplayInfoForObject(defaultObject);
        }
        else
        {
            DisplayInfoForObject(objectToDisplay.CurrentValue);
        }
    }
}
