﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace PravoAdder.DatabaseEnviroment
{
    public class DatabaseGetter
    {
        private static readonly HttpAuthenticator HttpAuthenticator;

        static DatabaseGetter()
        {
            HttpAuthenticator = new HttpAuthenticator();
        }

        public void Authentication(string login, string password)
        {
            HttpAuthenticator.Authentication(login, password);
        }

        private static IEnumerable<dynamic> GetPages(object content, string name, string uri)
        {          
            var request = HttpHelper.CreateRequest(content, $"api/{uri}", HttpMethod.Post, HttpAuthenticator.UserCookie);

            var response = HttpAuthenticator.Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            var jsonMessages = HttpHelper.GetMessageFromResponce(response).Result;
            foreach (var message in jsonMessages.Result)
            {
                yield return message;
            }
        }

        private static object GetSimplePage(string name, string uri)
        {
            var content = new
            {
                Name = name,
                PageSize = 50,
                Page = 1
            };

            var message = GetPages(content, name, uri).FirstOrDefault();

            return new
            {
                Id = (string)message?["Id"],
                Name = (string)message?["Name"],
            };
        }

        public dynamic GetProjectGroup(string projectName, int pageSize)
        {
            var content = new
            {
                Name = "",
                PageSize = pageSize,
                Page = 1
            };

            var pages = GetPages(content, "", "ProjectGroups/PostProjectGroups");

            foreach (var page in pages)
            {

                var name = (string) page?["Name"];
                if (name == projectName)
                    return new
                    {
                        Name = name,
                        Id = (string)page?["Id"]
                    };          
            }
            return null;
        }

        public dynamic GetProject(string projectName, string projectGroupid, string folderName, int pageSize)
        {
            var content = new
            {
                FullSearchString = "",
                PageSize = pageSize,
                Page = 1,
                FolderId = GetProjectFolder(folderName).Id
            };

            var projectGroup = GetPages(content, "", "Projects/GetGroupedProjects")
                .FirstOrDefault(pf => pf?["ProjectGroupResponse"]?["Id"] == projectGroupid);

            foreach (var project in projectGroup?["Projects"])
            {
                var name = (string) project?["Name"];
                if (projectName == name)
                {
                    return new
                    {
                        Name = (string) project?["Name"],
                        Id = (string) project?["Id"]
                    };
                }              
            }
            return null;
        }

        public dynamic GetProjectType(string projectTypeName)
        {
            return GetSimplePage(projectTypeName, "ProjectTypes/GetProjectTypes");
        }

        public dynamic GetResponsible(string resposibleName)
        {
            return GetSimplePage(resposibleName.Replace(".", ""), "CompanyUsersSuggest");
        }

        public dynamic GetProjectFolder(string folderName)
        {
            return GetSimplePage(folderName, "ProjectFolders/GetProjectFoldersForEdit");
        }

        public dynamic GetParticipant(string participantName)
        {
            return GetSimplePage(participantName, "ParticipantsSuggest/GetParticipants");
        }

        public dynamic GetCalculationFormulas(string formulaName)
        {
            return GetSimplePage(formulaName, "CalculationFormulasSuggest/GetCalculationFormulas");
        }
    }
}
