using System;

namespace Group3.Semester3.WebApp.Helpers.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException()
        {
        }

        public ConcurrencyException(string message)
            : base(message)
        {
        }

        public ConcurrencyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}