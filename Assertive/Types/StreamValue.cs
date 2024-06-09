namespace Assertive.Types
{
    internal class StreamValue : Value
    {
        private readonly Stream _value;
        private string _path;


        public StreamValue(Stream stream, string path)
        {
            _value = stream;
            _path = path;
        }

        public Stream Value { get { return _value; } }
        public string Path { get { return _path; } }

        public override string ToString()
        {
            _value.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(_value);
            return reader.ReadToEnd();
        }
    }
}
