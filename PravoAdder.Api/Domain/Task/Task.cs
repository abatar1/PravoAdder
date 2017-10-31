using System;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
    public class Task : ICreatable 
    {
        public Project Project { get; set; }
        public TaskName TaskName { get; set; }
        public Responsible ResponseUser { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public TaskState TaskState { get; set; }
        public string Id { get; set; }
        [JsonIgnore]
        public bool IsArchive { get; set; }
    }
}
