using Assertive.Exceptions;
using Assertive.Types;
using System.Text;

namespace Assertive.Requests.Http
{
    public class RequestContentFactory
    {
        public static HttpContent Create(AssertiveParser.BodySectionContext context, Value expressionResult, string? currentFile)
        {
            if (context.contentType == null || context.contentType.Type == AssertiveLexer.STRING || context.contentType.Type == AssertiveLexer.STREAM)
            {
                if (expressionResult is StreamValue streamValue)
                {
                    return new StreamContent(streamValue.Value);
                }
                else
                {
                    return new StringContent(expressionResult.ToString()!, Encoding.UTF8, "application/json");
                }
            }
            else
            {
                switch (context.contentType.Type)
                {
                    case AssertiveLexer.FORMURLENCODED:
                        {
                            if (expressionResult is DictionaryValue dic)
                            {
                                return new FormUrlEncodedContent(dic.ToKeyValuePairs());
                            }
                            else
                            {
                                throw new InterpretationException("Body expression should be a dictionary when contenttype is formurlencoded", context, currentFile);
                            }
                        }
                    case AssertiveLexer.FORMDATA:
                        {
                            if (expressionResult is DictionaryValue dic)
                            {
                                var multipartContent = new MultipartFormDataContent();
                                foreach (var entry in dic.GetEntries())
                                {
                                    if (entry.Value is StreamValue streamVal)
                                    {
                                        streamVal.Value.Seek(0, SeekOrigin.Begin);
                                        using MemoryStream ms = new();
                                        streamVal.Value.CopyTo(ms);
                                        multipartContent.Add(new ByteArrayContent(ms.ToArray()), "file", Path.GetFileName(streamVal.Path));
                                    }
                                    else
                                    {
                                        multipartContent.Add(new StringContent(entry.Value.ToString()!), entry.Key.ToString()!);
                                    }
                                }
                                return multipartContent;
                            }
                            else
                            {
                                throw new InterpretationException("Body expression should be a dictionary when contenttype is formdata", context, currentFile);
                            }
                        }
              
                    default:
                        {
                            throw new InterpretationException("Unknown body content type", context, currentFile);
                        }
                }

            }

        }

    }
}
