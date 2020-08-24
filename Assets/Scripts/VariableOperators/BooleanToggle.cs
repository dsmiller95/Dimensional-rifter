using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.VariableOperators
{
    public class BooleanToggle : MonoBehaviour
    {
        public BooleanVariable variableToToggle;

        public void Toggle()
        {
            variableToToggle.SetValue(!variableToToggle.CurrentValue);
        }
    }
}
