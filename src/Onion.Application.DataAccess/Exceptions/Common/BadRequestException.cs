﻿namespace Onion.Application.DataAccess.Exceptions.Common;

public class BadRequestException : Exception
{
    public string MessageKey { get; private set; }
    public string Details { get; set; }

    public BadRequestException(string messageKey, string details = null) : base()
    {
        MessageKey = messageKey;
        Details = details;
    }
}
