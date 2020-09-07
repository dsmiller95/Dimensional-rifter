using System.Collections.Generic;

namespace BehaviorTree.Nodes
{
    public class Blackboard
    {
        private IDictionary<string, object> data = new Dictionary<string, object>();

        public bool TryGetValueOfType<T>(string key, out T value)
        {
            if (key != null && data.TryGetValue(key, out var objectValue) && objectValue is T casted)
            {
                value = casted;
                return true;
            }
            value = default;
            return false;
        }
        public bool TryGetValue(string key, out object value)
        {
            if (key != null && data.TryGetValue(key, out var objectValue))
            {
                value = objectValue;
                return true;
            }
            value = default;
            return false;
        }

        public void SetValue<T>(string key, T value)
        {
            data[key] = value;
        }
        public void ClearValue(string key)
        {
            data.Remove(key);
        }
    }
}
