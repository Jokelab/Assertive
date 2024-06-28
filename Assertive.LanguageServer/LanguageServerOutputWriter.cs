using Assertive.Models;
using Assertive.Requests;
using Newtonsoft.Json;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Assertive.LanguageServer
{
    internal class LanguageServerOutputWriter : IOutputWriter
    {
        private ILanguageServerFacade _languageServer;
        public LanguageServerOutputWriter(ILanguageServer languageServer)
        {
            _languageServer = languageServer;
        }
        public Task RequestEnd(Request response)
        {
            var json = JsonConvert.SerializeObject(response);
            _languageServer.SendNotification($"assertive/{nameof(RequestEnd)}", json);
            return Task.CompletedTask;
        }

        public Task RequestStart(Request request)
        {
            var json = JsonConvert.SerializeObject(request);
            _languageServer.SendNotification($"assertive/{nameof(RequestStart)}", json);
            return Task.CompletedTask;
        }

        public Task Write(string text)
        {
            var json = JsonConvert.SerializeObject(text);
            _languageServer.SendNotification($"assertive/output", json);
            return Task.CompletedTask;
        }

        public Task AnnotatedFunctionStart(AnnotatedFunction annotatedFunction)
        {
            var json = JsonConvert.SerializeObject(annotatedFunction);
            _languageServer.SendNotification($"assertive/{nameof(AnnotatedFunctionStart)}", json);
            return Task.CompletedTask;
        }

        public Task AnnotatedFunctionEnd(AnnotatedFunction annotatedFunction)
        {
            var json = JsonConvert.SerializeObject(annotatedFunction);
            _languageServer.SendNotification($"assertive/{nameof(AnnotatedFunctionEnd)}", json);
            return Task.CompletedTask;

        }

        public Task Assertion(AssertResult assertResult)
        {
            var json = JsonConvert.SerializeObject(assertResult);
            _languageServer.SendNotification($"assertive/{nameof(Assertion)}", json);
            return Task.CompletedTask;
        }
    }
}
