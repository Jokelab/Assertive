using MediatR;
using OmniSharp.Extensions.JsonRpc;

namespace Assertive.LanguageServer.Models
{
    [Method("assertive/interpreterRequest")]
    public class InterpretationRequest : IRequest
    {
        public required string FilePath { get; set; }
    }
}
