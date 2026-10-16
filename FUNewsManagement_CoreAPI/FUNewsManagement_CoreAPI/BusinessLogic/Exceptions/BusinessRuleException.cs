using System;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
