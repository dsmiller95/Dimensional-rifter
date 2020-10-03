using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    public class ErrandBoardOwner : MonoBehaviour
    {
        public ErrandBoard myBoard;
        private void Awake()
        {
            myBoard.Init();
        }
    }
}
