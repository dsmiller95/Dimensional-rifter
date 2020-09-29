using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    public class ErrandBoardOwner: MonoBehaviour
    {
        public ErrandBoard myBoard;
        private void Awake()
        {
            myBoard.Init();
        }
    }
}
