using Assets.Scripts.Core;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Wall
{
    public interface ITileBlocking
    {
        IObservable<bool> OnBlockingChanges { get; }
        bool IsCurrentlyBlocking { get; }
    }

    [RequireComponent(typeof(TileMapMember))]
    public class Blocking : MonoBehaviour, ITileBlocking
    {
        public BooleanReference currentlyBlocking;

        public IObservable<bool> OnBlockingChanges => currentlyBlocking.ValueChanges;

        public bool IsCurrentlyBlocking => currentlyBlocking.CurrentValue;
    }
}
