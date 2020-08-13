using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.WorldObjects
{
    [RequireComponent(typeof(TileMapMemeber))]
    public class Hungry: MonoBehaviour
    {

        private void Update()
        {
            var tileMember = this.GetComponent<TileMapMemeber>();
            if(tileMember.SeekClosestOfType(member => member.gameObject.GetComponent<Food>() != null))
            {
                var foundFood = tileMember.currentTarget.gameObject.GetComponent<Food>();
                Destroy(foundFood.gameObject);
            }
        }
    }
}
