using UnityEngine;

namespace Assets.Scripts.Core
{
    [CreateAssetMenu(fileName = "GameObjectVariable", menuName = "Variables/GameObjectVariable", order = 1)]
    public class GameObjectVariable : GenericVariable<GameObject>
    {
    }
}
