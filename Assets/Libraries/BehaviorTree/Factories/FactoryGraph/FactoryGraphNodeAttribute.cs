using System;

namespace BehaviorTree.Factories.FactoryGraph
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FactoryGraphNodeAttribute : Attribute
    {
        public string menuName;
        public string name;
        public int childCountClassification;
        public FactoryGraphNodeAttribute(
            string menuName,
            string name,
            int childCountClassification
            )
        {
            this.menuName = menuName;
            this.name = name;
            this.childCountClassification = childCountClassification;
        }
    }
}
