using BehaviorTree.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;

namespace Assets.Libraries.BehaviorTree.Editor.GraphEditor
{
    public class BehaviorGraphViewNode: Node
    {
        public string GUID;

        public NodeFactory nodeFactory;

        public bool EntryPoint = false;
    }
}
