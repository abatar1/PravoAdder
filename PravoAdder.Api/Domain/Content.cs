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

        public static IDictionary<string, object> Get(Content content, int count)
        {
            var result = new ExpandoObject() as IDictionary<string, object>;
            result.Add("PageSize", ApiRouter.PageSize);
            result.Add("Page", count);
            if (content == null) return result;
            
            if (content.Parameters != null)
            {
                foreach (var pair in content.Parameters)
                {
                    result.Add(pair.Key, pair.Value);
                }
            }
            if (content.Json != null)
            {
                foreach (var pair in content.Json)
                {
                    result.Add(pair.Key, pair.Value);
                }
            }
            return result;
        }
    }
}
