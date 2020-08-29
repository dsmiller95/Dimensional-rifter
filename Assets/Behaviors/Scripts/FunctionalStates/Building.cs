using Assets.Behaviors.Scripts.Utility_states;
using Assets.WorldObjects.Members.Building;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Building : ContinueableState<TileMapMember>
    {
        public Building()
        {
        }

        public override IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            var seekResult = tileMember.SeekClosestOfType(BuildingFilter);
            if (seekResult.status == NavigationStatus.ARRIVED)
            {
                var buildable = seekResult.reached.GetComponent<Buildable>();
                buildable.BuildIfPossible();
                return next;
            }
            if(seekResult.status == NavigationStatus.INVALID_TARGET)
            {
                return next;
            }
            return this;
        }

        private bool BuildingFilter(TileMapMember member)
        {
            var buildable = member.GetComponent<Buildable>();
            return buildable != null && buildable.CanBuild();
        }

        public override void TransitionIntoState(TileMapMember data)
        {
            base.TransitionIntoState(data);
        }
        public override void TransitionOutOfState(TileMapMember data)
        {
        }
        public override string ToString()
        {
            return $"Building";
        }
    }
}
