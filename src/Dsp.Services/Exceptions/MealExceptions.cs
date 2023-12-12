using System;

namespace Dsp.Services.Exceptions;

public class MealItemAlreadyExistsException : Exception
{
    public MealItemAlreadyExistsException(string message) : base(message)
    {

    }
}

public class MealItemAlreadyAssignedException : Exception
{
    public MealItemAlreadyAssignedException(string message) : base(message)
    {

    }
}
