using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SetSpriteColor: MonoBehaviour
    {
        public Color colorToSet;
        public void SetColor()
        {
            GetComponent<SpriteRenderer>().color = colorToSet;
        }
    }
}
