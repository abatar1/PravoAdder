using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
    public class Task : DatabaseEntityItem, ICreatable
    {
        public Project Project { get; set; }
        public TaskName TaskName { get; set; }
        public Responsible ResponseUser { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public TaskState TaskState { get; set; }
	    public List<string> TimeLogs { get; set; }
		[JsonIgnore]
        public bool IsArchive { get; set; }
    }
}
