using Assets.WorldObjects.Members;
using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    [CreateAssetMenu(fileName = "ErrandType", menuName = "Behaviors/ErrandType", order = 1)]
    public class ErrandType : IDableObject
    {
        public int uniqueID = 0;
        public override void AssignId(int myNewID)
        {
            uniqueID = myNewID;
        }
    }
}
