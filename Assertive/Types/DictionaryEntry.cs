namespace Assertive.Types
{

    internal class DictionaryEntry : Value
    {
        public required Value Key { get; set; }
        public required Value Value { get; set; }

        public override string ToString()
        {
            return Key.ToString() + ":" + Value.ToString();
        }
    }

}
