﻿using System;

namespace PravoAdder.Domain
{
    public class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute(bool needToIgnore)
        {
            Ignore = needToIgnore;
        }

        public bool Ignore { get; set; }
    }
}