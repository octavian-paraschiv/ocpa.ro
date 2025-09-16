using System;

namespace ocpa.ro.common.Exceptions
{
    public class ExtendedException : Exception
    {
        public ExtendedException(string message) : base(message)
        {
        }
    }
}
