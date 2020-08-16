using Assets.WorldObjects.Members;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InterestingInfoDisplayer : MonoBehaviour
{
    public static InterestingInfoDisplayer instance;

    public TextMeshProUGUI text;

    private void Awake()
    {
        instance = this;
    }

    public void DisplayInfoForObject(GameObject obj)
    {
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
        
    }
}
