using System;
using System.Text.RegularExpressions;
using Utilizr.Globalisation;

namespace Utilizr.WPF.Validation
{
    public delegate ValidationResult CustomValidationDelegate(string? input);

    /// <summary>
    /// Opportunity to tweak input before validation applied.
    /// E.g. may wish to trim spaces at the start/end to save user having to do it
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public delegate string PreValidateCoercion(string? input);


    public class Validater
    {
        public EventHandler? ClearErrorRequest;
        public EventHandler<ValidationResult>? Validated;

        public CustomValidationDelegate ValidationDelegate { get; set; }
        public PreValidateCoercion? PreValidateCoercion { get; set; }

        public Validater(CustomValidationDelegate validationDelegate)
        {
            ValidationDelegate = validationDelegate;
        }

        public Validater(CustomValidationDelegate validationDelegate, PreValidateCoercion? preValidateCoercion)
            : this(validationDelegate)
        {
            PreValidateCoercion = preValidateCoercion;
        }

        public ValidationResult Validate(string? input)
        {
            var result = new ValidationResult
            {
                IsValid = true
            };

            if (ValidationDelegate == null)
                return result;

            if (PreValidateCoercion == null)
            {
                result.MergeResult(ValidationDelegate(input));
            }
            else
            {
                result.MergeResult(ValidationDelegate(PreValidateCoercion(input)));
            }

            OnValidationResult(result);
            return result;
        }

        public void ClearErrors()
        {
            OnClearErrorRequest();
        }

        protected virtual void OnClearErrorRequest()
        {
            ClearErrorRequest?.Invoke(this, new EventArgs());
        }

        protected virtual void OnValidationResult(ValidationResult result)
        {
            Validated?.Invoke(this, result);
        }

        #region DefaultValidaters

        public static Validater CreateEmailValidater(PreValidateCoercion? preValidateCoercion = null)
        {
            return new Validater(ValidateEmail, preValidateCoercion);
        }

        public static Validater CreateNameValidater(int minLength, PreValidateCoercion? preValidateCoercion = null)
        {
            return new Validater((input) => ValidateName(input, minLength), preValidateCoercion);
        }

        //TODO: expand check symbols / caps
        public static Validater CreatePasswordValidater(int minLength, PreValidateCoercion? preValidateCoercion = null)
        {
            return new Validater((input) => ValidatePassword(input, minLength), preValidateCoercion);
        }

        public static Validater CreateIntegerValidater(int? minimum = null, int? maximum = null, PreValidateCoercion? preValidateCoercion = null)
        {
            return new Validater((input) => ValidateInteger(input, minimum, maximum), preValidateCoercion);
        }

        static ValidationResult ValidateEmail(string? input)
        {
            var result = new ValidationResult() { IsValid = true };
            if (string.IsNullOrEmpty(input))
            {
                result.IsValid = false;
                result.Messages.Add(L._I("Please enter an email address."));
                return result;
            }

            var regexPattern = @"^[_a-zA-Z0-9-+.!#$%&'*+-/=?^_`{|}~]+(\.[_a-zA-Z0-9-+]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            if (!regex.IsMatch(input))
            {
                result.IsValid = false;
                result.Messages.Add(L._I("Please enter a valid email address."));
            }
            return result;
        }

        static ValidationResult ValidateName(string? input, int minLength)
        {
            return ValidateMinimumLength(
                input,
                minLength,
                L._I("Please enter a name."),
                L._I("Please enter a name (minimum {0} characters).", () => L.Args(minLength)));
        }

        static ValidationResult ValidatePassword(string? input, int minLength)
        {
            return ValidateMinimumLength(
                input,
                minLength,
                L._I("Please enter a password."),
                L._I("Please enter a password (minimum {0} characters).", () => L.Args(minLength)));
        }

        static ValidationResult ValidateMinimumLength(string? input, int minLength, ITranslatable noInputMessage, ITranslatable underMinLengthMessage)
        {
            var result = new ValidationResult() { IsValid = true };
            if (string.IsNullOrEmpty(input))
            {
                result.IsValid = false;
                result.Messages.Add(noInputMessage);
                return result;
            }

            if (input.Length < minLength)
            {
                result.IsValid = false;
                result.Messages.Add(underMinLengthMessage);
            }

            return result;
        }

        static ValidationResult ValidateInteger(string? input, int? minimum, int? maximum)
        {
            var result = new ValidationResult() { IsValid = true };
            if (string.IsNullOrEmpty(input))
            {
                result.IsValid = false;
                //## Data validation on user inputted text
                result.Messages.Add(L._I("Please enter a number."));
                return result;
            }

            // Better than using Char.IsDigit. This handles negative numbers, need to parse anyway if min / max specified...
            if (!Int64.TryParse(input, out long parsedNumber))
            {
                result.IsValid = false;
                result.Messages.Add(L._I("Please enter digits only."));
                return result;
            }

            if (minimum.HasValue && parsedNumber < minimum.Value)
            {
                result.IsValid = false;
                result.Messages.Add(L._I("Please enter a number equal to or greater than {0}.", () => L.Args(minimum.Value)));
                return result;
            }

            if (maximum.HasValue && parsedNumber > maximum.Value)
            {
                result.IsValid = false;
                result.Messages.Add(L._I("Please enter a number equal to or less than {0}.", () => L.Args(maximum.Value)));
                return result;
            }

            return result;
        }
        #endregion
    }
}
