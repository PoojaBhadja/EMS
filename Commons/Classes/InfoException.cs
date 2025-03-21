﻿using System.Runtime.Serialization;

namespace Commons.Classes
{
    [Serializable]
    public class InfoException : Exception
    {
        public InfoException()
        {
        }

        public InfoException(string message) : base(message)
        {
        }

        public InfoException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InfoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}
