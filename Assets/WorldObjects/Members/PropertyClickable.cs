using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.WorldObjects.Members
{
    [RequireComponent(typeof(Collider2D))]
    public class PropertyClickable: MonoBehaviour
    {
        private void OnMouseDown()
        {
            Debug.Log("me click");
            InterestingInfoDisplayer.instance.DisplayInfoForObject(gameObject);
        }
    }
}
