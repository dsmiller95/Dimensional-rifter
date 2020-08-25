using Assets.Scripts.Core;
using System;
using UniRx;
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

        private TileMapMember tilemapMember;

        public IObservable<bool> OnBlockingChanges => currentlyBlocking.ValueChanges;

        public bool IsCurrentlyBlocking => currentlyBlocking.CurrentValue;

        private void Awake()
        {
            tilemapMember = GetComponent<TileMapMember>();
        }

        private void Start()
        {
            SetBlocking(currentlyBlocking.CurrentValue);
            currentlyBlocking.ValueChanges
                .TakeUntilDisable(this)
                .Subscribe(newValue =>
                {
                    SetBlocking(newValue);
                });
        }


        private void SetBlocking(bool isBlocking)
        {
        }
    }
}
