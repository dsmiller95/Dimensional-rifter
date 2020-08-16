using System;
using System.Collections.Generic;

namespace Assets.WorldObjects.SaveObjects
{
    [Serializable]
    public class WorldSaveObject
    {
        public IList<TileRegionSaveObject> regions;
    }
}
