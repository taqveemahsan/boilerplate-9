using System;
using System.Collections.Generic;

namespace Audit.Services
{
    public sealed class AdjustmentValidationException : Exception
    {
        public AdjustmentValidationException(IReadOnlyList<string> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }

        public IReadOnlyList<string> Errors { get; }
    }
}
