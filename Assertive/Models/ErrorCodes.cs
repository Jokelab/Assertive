using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assertive.Models
{
    internal class ErrorCodes
    {
        public const string FunctionNotFound = "F0001";
        public const string FunctionAlreadyDefined= "F0002";
        public const string FunctionParamsMismatch = "F0003";

        public const string ImportFileNotFound = "I0001";
        public const string ImportFileAlreadyImported = "I0002";
        public const string ImportFileSyntaxErrors = "I0003";

        public const string VariableNotFound = "V0001";

        public const string RuntimeException = "R0001";
    }
}
