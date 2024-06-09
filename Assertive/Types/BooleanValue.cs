namespace Assertive.Types
{
    internal class BooleanValue : Value
    {
        private readonly bool _value;
        public BooleanValue(bool value)
        {
            _value = value;
        }

        public BooleanValue(string value)
        {
            _value = bool.Parse(value);
        }

        public bool Value { get { return _value; } }

        public override string ToString()
        {
            return _value.ToString().ToLower();
        }

    }
}
