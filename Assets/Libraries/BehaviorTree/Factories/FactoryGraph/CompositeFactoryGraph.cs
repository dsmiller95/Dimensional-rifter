using BehaviorTree.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories.FactoryGraph
{
    [CreateAssetMenu(fileName = "FactoryGraph", menuName = "Behaviors/FactoryGraph", order = 1)]
    public class CompositeFactoryGraph : NodeFactory
    {
        public FactoryNodeSavedNode[] _savedNodes;
        public FactoryNodeSavedNode[] SavedNodes
        {
            get => _savedNodes;
            set
            {
                _savedNodesByGuid = null;
                _savedNodes = value;
            }
        }

        private IDictionary<string, FactoryNodeSavedNode> _savedNodesByGuid;
        public IDictionary<string, FactoryNodeSavedNode> SavedNodesByGuid {
            get
            {
                if(_savedNodesByGuid == null)
                {
                    _savedNodesByGuid = SavedNodes.ToDictionary(x => x.Guid);
                }
                return _savedNodesByGuid;
            }
        }


        public NodeFactory entryFactory;

        public override int GetValidChildCount()
        {
            return 0;
        }

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return null;
        }
    }
}
