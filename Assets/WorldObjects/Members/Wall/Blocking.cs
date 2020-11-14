using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.WorldObjects.Members.Wall
{
    public interface ITileBlocking
    {
        bool IsCurrentlyBlocking { get; }
    }

    [RequireComponent(typeof(TileMapMember))]
    public class Blocking : MonoBehaviour, ITileBlocking
    {
        public BooleanReference currentlyBlocking;

        public bool IsCurrentlyBlocking => currentlyBlocking.CurrentValue;
    }
}
