using System;
using System.Collections.Generic;
using System.IO;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder.Controllers
{
    public class MigrationProcessController
    {
	    public MigrationProcessController(TextWriter writer)
	    {
		    Console.SetOut(writer);
	    }

        public HttpAuthenticator Authenticate(Settings settings)
        {
            while (true)
            {              
                Console.WriteLine($"Login as {settings.Login} to {settings.BaseUri}...");
                try
                {
                    var authentication = new HttpAuthenticator(settings.BaseUri);
                    authentication.Authentication(settings.Login, settings.Password);
                    return authentication;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }    	

        public string AddProjectGroup(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings)
        {
	        try
	        {
		        var projectGroupSender = filler.AddProjectGroupAsync(settings, headerBlock).Result;
		        return projectGroupSender.Content;
			}
	        catch (Exception e)
	        {
		        Console.WriteLine(e);
		        throw;
	        }                  
        }

        public string AddProject(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings, string projectGroupId)
        {
	        try
	        {
		        var projectSender = filler.AddProjectAsync(settings, headerBlock, projectGroupId).Result;
		        return projectSender.Content;
			}
	        catch (Exception e)
	        {
		        Console.WriteLine(e);
		        throw;
	        }           
        }

        public void AddInformationAsync(BlockInfo blockInfo, DatabaseFiller filler, IDictionary<int, string> excelRow, string projectId)
        {
	        try
	        {
		        filler.AddInformationAsync(projectId, blockInfo, excelRow).Wait();
			}
	        catch (Exception e)
	        {
		        Console.WriteLine(e);
		        throw;
	        }
            
        }

	    public void ProcessCount(int current, int total)
	    {
			Console.WriteLine($"\t{current}/{total} processing.");
		}
    }
}
