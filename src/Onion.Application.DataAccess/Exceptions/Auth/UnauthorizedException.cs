﻿using System;

namespace Onion.Application.DataAccess.Exceptions.Auth
{
    public class UnauthorizedException : Exception
    {

        private const string MESSAGE_KEY = "Unauthorized";
        public string MessageKey { get; private set; }

        public UnauthorizedException() : base()
        {
            MessageKey = MESSAGE_KEY;
        }
    }
}
