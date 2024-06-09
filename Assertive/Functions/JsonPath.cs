using Assertive.Exceptions;
using Assertive.Types;
using Newtonsoft.Json.Linq;

namespace Assertive.Functions
{
    public class JsonPath : IFunction
    {

        public int ParameterCount => 2;

        public string Name => nameof(JsonPath);

        public async Task<Value> Execute(List<Value> values, FunctionContext context)
        {
            if (values[0] is not HttpRequestValue httpResponse)
            {
                throw new FunctionExecutionException("No http request value provided", this);
            }
            if (values[1] is not StringValue stringValue)
            {
                throw new FunctionExecutionException("No string path provided", this);
            }
            var body = await httpResponse.GetResponseBody();
            JToken token = JObject.Parse(body!);
            var selectedToken = token.SelectToken(stringValue.ToString());
            if (selectedToken == null)
            {
                throw new FunctionExecutionException(stringValue.ToString() + " did not get a valid json token", this);
            }
            return new JsonValue(selectedToken);
        }


    }
}
