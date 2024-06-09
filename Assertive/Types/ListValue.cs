namespace Assertive.Types
{
    internal class ListValue : Value
    {
        private List<Value> _values;
        public ListValue(List<Value> values)
        {
            _values = values;
        }

        public List<Value> ListValues => _values;

        public override string ToString()
        {
            return $"[{string.Join(',', _values)}]";
        }
    }
}
