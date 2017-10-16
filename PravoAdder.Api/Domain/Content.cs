using System.Collections.Generic;
using System.Dynamic;

namespace PravoAdder.Api.Domain
{
    public class Content
    {
        public Content(IDictionary<string, string> parameters = null, IDictionary<string, string> json = null)
        {
            Parameters = parameters;
            Json = json;
        }

        private IDictionary<string, string> Json { get; }
        private IDictionary<string, string> Parameters { get; }

	    public IDictionary<string, object> Get(int count)
	    {		
		    var result = new ExpandoObject() as IDictionary<string, object>;
		    result.Add("PageSize", ApiRouter.PageSize);
		    result.Add("Page", count);

		    if (Parameters != null)
		    {
			    foreach (var pair in Parameters)
			    {
				    result.Add(pair.Key, pair.Value);
			    }
		    }
		    if (Json != null)
		    {
			    foreach (var pair in Json)
			    {
				    result.Add(pair.Key, pair.Value);
			    }
		    }
		    return result;
	    }
    }
}
