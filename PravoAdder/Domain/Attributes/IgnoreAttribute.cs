using System;

namespace PravoAdder.Domain.Attributes
{
    public class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute(bool needToIgnore)
        {
            Ignore = needToIgnore;
        }

	    public IgnoreAttribute()
	    {
		    Ignore = true;
	    }

		public bool Ignore { get; set; }
    }
}