using System;

namespace PravoAdder.Domain
{
    public class SettingsIgnoreAttribute : Attribute
    {
        public SettingsIgnoreAttribute(bool needToIgnore)
        {
            Ignore = needToIgnore;
        }

        public bool Ignore { get; set; }
    }
}