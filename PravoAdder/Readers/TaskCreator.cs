using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class TaskCreator : Creator
	{
	    private IList<DictionaryItem> _taskStates;
		private readonly Dictionary<string, IEnumerable<Project>> _projects;

	    private TaskState GetState(string name)
	    {
	        var item = _taskStates.FirstOrDefault(t => t.Name == name);
	        return new TaskState(item?.Name, item?.Id, item?.SysName);
        }

		public override ICreatable Create(Table table, Row row, DatabaseEntityItem item = null)
		{
			var task = new Task {Id = null, TimeLogs = new List<string> {item?.Id}};

		    foreach (var valuePair in row.Content)
		    {
		        var fieldName = table.Header[valuePair.Key].FieldName;
		        var value = valuePair.Value.Value?.Trim();
		        if (fieldName == "Case name")
		        {
			        var projectFolders = ProjectFolderRepository.GetMany(HttpAuthenticator);
		            foreach (var folder in projectFolders)
		            {
		                if (!_projects.ContainsKey(folder.Name))
		                {
		                    _projects.Add(folder.Name, new List<Project>());
			                _projects[folder.Name] = ApiRouter.Projects.GetMany(HttpAuthenticator, folder.Name);
		                }
		                var project = _projects[folder.Name]
		                    .FirstOrDefault(s => s.Name == value);
		                if (project != null)
		                {
		                    if (ApiRouter.Projects.Get(HttpAuthenticator, project.Id).IsArchive)
		                    {
		                        ApiRouter.Projects.Restore(HttpAuthenticator, project.Id);
		                        task.IsArchive = true;
		                    }
		                    task.Project = project;
		                    break;
		                }
		            }
		            if (task.Project == null) return null;
		        }
                else if (fieldName == "Name")
		        {
		            task.TaskName = new TaskName(value);
		        }
                else if (fieldName == "Note")
		        {
		            task.Description = value;
		        }
                else if (fieldName == "Due date")
		        {
		            if (!DateTime.TryParse(value, out var date))
		            {
		                date = DateTime.Now;
		            }
		            task.EndDate = date;
		        }
                else if (fieldName == "Completed")
		        {
		            if (_taskStates == null)
		                _taskStates = ApiRouter.DefaultDictionaryItems.GetMany(HttpAuthenticator, "Tasks.TaskState");

		            task.TaskState = GetState(bool.Parse(value) ? "Completed" : "In Progress");
		        }
                else if (fieldName == "Priority")
		        {
		            task.Priority = int.Parse(value);
		        }               
                else if (fieldName == "Responsible")
		        {
			        task.ResponseUser = ResponsibleRepository.Get(HttpAuthenticator, value);
		        }
		    }
			return task;
		}

		public TaskCreator(HttpAuthenticator httpAuthenticator, Settings settings) : base(httpAuthenticator, settings)
		{
			_projects = new Dictionary<string, IEnumerable<Project>>();
		}
	}
}
