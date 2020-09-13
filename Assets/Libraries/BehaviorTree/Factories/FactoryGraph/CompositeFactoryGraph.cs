using BehaviorTree.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        public IDictionary<string, FactoryNodeSavedNode> SavedNodesByGuid
        {
            get
            {
                if (_savedNodesByGuid == null)
                {
                    _savedNodesByGuid = SavedNodes.ToDictionary(x => x.Guid);
                }
                return _savedNodesByGuid;
            }
        }


        public NodeFactory entryFactory;

        public override void SetChildFactories(IEnumerable<NodeFactory> children)
        {
            throw new NotImplementedException("oh this would be hard to do");
        }

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return entryFactory?.CreateNode(target);
        }

        public NodeFactory[] factoriesSavedWithAsset = new NodeFactory[0];
        public NodeFactory RegisterNewFactoryInsideAsset(NodeFactory instance)
        {
            if (instance == this)
            {
                return instance;
            }
            AssetDatabase.AddObjectToAsset(instance, this);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(instance));
            factoriesSavedWithAsset = factoriesSavedWithAsset.Append(instance).ToArray();
            return instance;
        }

        public void DestroyUnlinkedNodeFactories()
        {
            var factoriesReferencedInSaveData = new HashSet<int>(
                    SavedNodes
                    .Select(saved => saved.factory?.GetInstanceID())
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                );

            factoriesSavedWithAsset = factoriesSavedWithAsset
                .Where(factory => factory != this)
                .Select(factory =>
                {
                    if (factoriesReferencedInSaveData.Contains(factory.GetInstanceID()))
                    {
                        return factory;
                    }
                    DestroyImmediate(factory, true);
                    return null;
                })
                .Where(x => x != null)
                .ToArray();
        }
    }
}
