using System;

namespace ocpa.ro.domain.Exceptions
{
    public class ExtendedException : Exception
    {
        public ExtendedException(string message) : base(message)
        {
        }
    }
}
