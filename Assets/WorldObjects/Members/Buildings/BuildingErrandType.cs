using Assets.Behaviors.Errands.Scripts;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings
{
    [CreateAssetMenu(fileName = "BuildingErrandType", menuName = "Behaviors/BuildingErrandType", order = 1)]
    public class BuildingErrandType : ErrandType
    {
        public BuildingErrand CreateErrand(BuildingController builder)
        {
            return new BuildingErrand(this, builder);
        }
    }
}
