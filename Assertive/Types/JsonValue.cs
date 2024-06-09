using Newtonsoft.Json.Linq;

namespace Assertive.Types
{
    internal class JsonValue : Value
    {
        JToken _token;

        public JsonValue(JToken token)
        {
            _token = token;
        }

        public override string ToString()
        {
            return _token.ToString();
        }
    }
}
