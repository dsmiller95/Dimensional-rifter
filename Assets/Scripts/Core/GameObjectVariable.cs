using UnityEngine;

namespace Assets.Scripts.Core
{
    [CreateAssetMenu(fileName = "GameObjectVariable", menuName = "State/GameObjectVariable", order = 1)]
    public class GameObjectVariable : GenericVariable<GameObject>
    {
    }
}
