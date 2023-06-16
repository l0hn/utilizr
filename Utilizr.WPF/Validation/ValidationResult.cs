using System;
using System.Collections.Generic;
using System.Linq;
using Utilizr.Globalisation;

namespace Utilizr.WPF.Validation
{
    public class ValidationResult : EventArgs
    {
        public bool IsValid { get; set; }
        public List<ITranslatable> Messages { get; set; }

        public ValidationResult()
        {
            Messages = new List<ITranslatable>();
        }

        public ValidationResult MergeResult(ValidationResult resultToMerge)
        {
            IsValid = IsValid && resultToMerge.IsValid;
            Messages.AddRange(resultToMerge.Messages);
            return this;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Messages.Select(p => p.Translation).ToArray());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
