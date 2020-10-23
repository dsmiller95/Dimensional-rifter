using UnityEngine;

namespace Assets.Scripts.Core
{
    [CreateAssetMenu(fileName = "ScriptableObjectVariable", menuName = "State/ScriptableObjectVariable", order = 1)]
    public class ScriptableObjectVariable : GenericVariable<ScriptableObject>
    {
    }
}
