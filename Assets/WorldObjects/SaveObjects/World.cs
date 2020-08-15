using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.WorldObjects.SaveObjects
{
    [Serializable]
    public class World
    {
        public IList<TileRegionSaveObject> regions;
    }
}
