using UnityEngine;

namespace Assets.UI.Manipulators
{
    public abstract class MapManipulator : ScriptableObject
    {
        public abstract void OnOpen(ManipulatorController controler);
        public abstract void OnUpdate();
        public abstract void OnClose();
    }
}
