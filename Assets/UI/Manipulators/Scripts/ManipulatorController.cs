using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UI.Manipulators
{
    public class ManipulatorController: MonoBehaviour
    {
        public MapManipulator activeManipulator;

        private void Start()
        {
            activeManipulator?.OnOpen(this);
        }

        public void SetActiveManipulator(MapManipulator nextActive)
        {
            activeManipulator?.OnClose();
            this.activeManipulator = nextActive;
        }

        public void OnManipulatorClosed(MapManipulator manipulator)
        {
            if(manipulator == activeManipulator)
            {
                activeManipulator = null;
            }
        }

        private void Update()
        {
            activeManipulator?.OnUpdate();
        }
    }
}
