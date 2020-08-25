using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.VariableOperators
{
    public class BooleanToggle : MonoBehaviour
    {
        public BooleanReference variableToToggle;

        public void Toggle()
        {
            variableToToggle.SetValue(!variableToToggle.CurrentValue);
        }
    }
}
