namespace Assertive.Types
{
    internal class StringValue : Value
    {
        private readonly string _value;
    

        public StringValue(string value)
        {
            _value = value;
        }

        public string Value { get { return _value; } }

        public override string ToString()
        {
            return _value;
        }
    }
}
