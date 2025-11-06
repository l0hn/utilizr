using System;
using System.Collections.Generic;

namespace Utilizr.Validation
{
    public class ValidationResult : EventArgs
    {
        public bool IsValid { get; set; }
        public List<string> Messages { get; set; }

        public ValidationResult()
        {
            Messages = new List<string>();
        }

        public ValidationResult MergeResult(ValidationResult resultToMerge)
        {
            IsValid = IsValid && resultToMerge.IsValid;
            Messages.AddRange(resultToMerge.Messages);
            return this;
        }

        public override string ToString()
        {
            return String.Join(Environment.NewLine, Messages.ToArray());
        }
    }
}
