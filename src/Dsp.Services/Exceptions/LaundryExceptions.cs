using System;

namespace Dsp.Services.Exceptions
{
    public class LaundrySignupsExceededException : Exception
    {
        public LaundrySignupsExceededException(string message) : base(message)
        {

        }
    }

    public class LaundrySignupAlreadyExistsException : Exception
    {
        public LaundrySignupAlreadyExistsException(string message) : base(message)
        {

        }
    }

    public class LaundrySignupNotFoundException : Exception
    {
        public LaundrySignupNotFoundException(string message) : base(message)
        {

        }
    }

    public class LaundrySignupPermissionException : Exception
    {
        public LaundrySignupPermissionException(string message) : base(message)
        {

        }
    }
}
