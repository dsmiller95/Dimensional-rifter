using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts
{
    public class ObjectAssigner : MonoBehaviour
    {
        public GameObjectVariable variableToSet;
        public GameObject objectToAssign;


        public void SetSelfToVariable()
        {
            variableToSet.SetValue(objectToAssign);
        }
    }
}
