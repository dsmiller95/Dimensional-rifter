using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts
{
    public class SelfObjectAssigner : MonoBehaviour
    {
        public GameObjectVariable variableToSet;

        public void SetSelfToVariable()
        {
            variableToSet.SetValue(gameObject);
        }
    }
}
