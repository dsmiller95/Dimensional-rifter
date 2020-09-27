using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    public class ErrandHandler
    {
        public readonly IErrand errand;
        public bool isClaimed = false;

        public bool IsComplete { get; private set; } = false;

        private ErrandBoard board;
        public ErrandHandler(IErrand errand, ErrandBoard board)
        {
            this.errand = errand;
            this.board = board;
        }

        public bool Available => !(isClaimed || IsComplete);

        public bool TryClaim(GameObject claimer)
        {
            if (isClaimed || IsComplete)
            {
                return false;
            }
            isClaimed = true;
            errand.ClaimedBy(claimer);
            return true;
        }

        public bool Complete()
        {
            if (IsComplete)
            {
                return false;
            }
            board.DeRegisterErrand(this);
            IsComplete = true;
            return true;
        }
    }
}
