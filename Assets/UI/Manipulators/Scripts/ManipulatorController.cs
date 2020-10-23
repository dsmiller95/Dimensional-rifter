using Assets.Scripts.Core;
using UniRx;
using UnityEngine;

namespace Assets.UI.Manipulators
{
    public class ManipulatorController : MonoBehaviour
    {
        public ScriptableObjectVariable manipulatorVariable;
        public MapManipulator activeManipulator;

        private void Awake()
        {
            manipulatorVariable.Value
                .TakeUntilDestroy(this)
                .Subscribe((nextValue) =>
                {
                    activeManipulator?.OnClose();
                    activeManipulator = nextValue as MapManipulator;
                    activeManipulator?.OnOpen(this);
                })
                .AddTo(this);
        }

        private void Update()
        {
            activeManipulator?.OnUpdate();
        }
    }
}
