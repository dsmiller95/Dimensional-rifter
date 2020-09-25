using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.VariableOperators
{
    public class ObjectAssigner : MonoBehaviour
    {
        public GameObjectVariable variableToSet;
        public GameObject objectToAssign;


        public bool AssignOnInit = false;

        private void Awake()
        {
            if (AssignOnInit)
            {
                SetToVariable();
            }
        }

        public void SetToVariable()
        {
            variableToSet.SetValue(objectToAssign);
        }
    }
}
