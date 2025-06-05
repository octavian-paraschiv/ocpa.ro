using System;

namespace ocpa.ro.api.Exceptions
{
    public class ExtendedException : Exception
    {
        public ExtendedException(string message) : base(message)
        {
        }
    }
}
