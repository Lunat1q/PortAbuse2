using System.Collections.Generic;

namespace PortAbuse2.Core.Common
{
    public class ValidationResult
    {
        public ValidationResult(ResultType result, IEnumerable<string> messages) : this(result)
        {
            this.Messages = messages;
        }
        public ValidationResult(ResultType result)
        {
            this.Result = result;
        }

        public ResultType Result { get; }

        public IEnumerable<string> Messages { get; }
    }
}