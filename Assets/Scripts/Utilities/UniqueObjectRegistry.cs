using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects.Members
{
    [Serializable]
    public abstract class IDableObject : ScriptableObject
    {
        public abstract void AssignId(int myNewID);
    }

    public abstract class UniqueObjectRegistry: ScriptableObject
    {
        public abstract IDableObject[] AllObjects { get; }

        public void AssignAllIDs()
        {
            for (var i = 0; i < AllObjects.Length; i++)
            {
                AllObjects[i].AssignId(i);
            }
        }
    }

    public abstract class UniqueObjectRegistryWithAccess<T> : UniqueObjectRegistry where T: IDableObject
    {
        public T[] allObjects;
        public override IDableObject[] AllObjects => allObjects;

        public T GetUniqueObjectFromID(int id)
        {
            return allObjects[id];
        }
    }
}
