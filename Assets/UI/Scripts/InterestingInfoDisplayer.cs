using Assets.Scripts.Core;
using Assets.WorldObjects.Members;
using System.Linq;
using TMPro;
using UnityEngine;

public class InterestingInfoDisplayer : MonoBehaviour
{
    public GameObjectVariable objectToDisplay;

    public TextMeshProUGUI text;


    public void DisplayInfoForObject(GameObject obj)
    {
        if(!obj || obj == null)
        {
            text.text = "Nothing selected";
            return;
        }
        var interestingBits = obj.GetComponentsInChildren<IInterestingInfo>();

        var info = interestingBits.Select(x => x.GetCurrentInfo()).Aggregate((a, b) => a + "\n-------\n" + b);
        text.text = info;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        DisplayInfoForObject(objectToDisplay.CurrentValue);
    }
}
