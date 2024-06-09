namespace Assertive.Types
{
    internal class NumericValue : Value
    {
        private readonly int _value;
        public NumericValue(int value)
        {
            _value = value;
        }

        public NumericValue(string value)
        {
            _value = Convert.ToInt32(value);
        }

        public int Value { get { return _value; } }

        public override string ToString()
        {
            return _value.ToString();
        }

    }
}
