using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class TaskCreator : ICreator
	{
	    private IList<DictionaryItem> _taskStates;
	    private IList<ProjectFolder> _projectFolders;
	    private readonly Dictionary<string, IList<Project>> _projects;
	    private IList<Responsible> _responsibles;

		public TaskCreator(HttpAuthenticator authenticator)
		{
			HttpAuthenticator = authenticator;
            _projects = new Dictionary<string, IList<Project>>();
		}

	    private TaskState GetState(string name)
	    {
	        var item = _taskStates.FirstOrDefault(t => t.Name == name);
	        return new TaskState(item?.Name, item?.Id, item?.SysName);
        }

		public HttpAuthenticator HttpAuthenticator { get; }

		public ICreatable Create(Row info, Row row)
		{
			var task = new Task {Id = null};

		    foreach (var valuePair in row.Content)
		    {
		        var fieldName = info[valuePair.Key].FieldName;
		        var value = valuePair.Value.Value?.Trim();
		        if (fieldName == "Case name")
		        {
		            if (_projectFolders == null) _projectFolders = ApiRouter.ProjectFolders.GetProjectFolders(HttpAuthenticator);
		            foreach (var folder in _projectFolders)
		            {
		                if (!_projects.ContainsKey(folder.Name))
		                {
		                    _projects.Add(folder.Name, new List<Project>());
			                _projects[folder.Name] = ApiRouter.Projects.GetProjects(HttpAuthenticator, folder.Name);
		                }
		                var project = _projects[folder.Name]
		                    .FirstOrDefault(s => s.Name == value);
		                if (project != null)
		                {
		                    if (ApiRouter.Projects.GetProject(HttpAuthenticator, project.Id).IsArchive)
		                    {
		                        ApiRouter.Projects.RestoreProject(HttpAuthenticator, project.Id);
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
		                _taskStates = ApiRouter.Dictionary.GetDefaultDictionaryItems(HttpAuthenticator, "Tasks.TaskState");

		            task.TaskState = GetState(bool.Parse(value) ? "Completed" : "In Progress");
		        }
                else if (fieldName == "Priority")
		        {
		            task.Priority = int.Parse(value);
		        }               
                else if (fieldName == "Responsible")
		        {
		            if (_responsibles == null) _responsibles = ApiRouter.Responsibles.GetResponsibles(HttpAuthenticator);
		            task.ResponseUser = _responsibles.First(r => r.Name == value);
		        }
		    }
			return task;
		}
	}
}
