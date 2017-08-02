using System.Linq;
using PravoAdder.DatabaseEnviroment;

namespace PravoAdder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var excel = ExcelReader.ReadDataFromTable("test.xlsx");
            var blocks = BlockReader.Read("blocksInfo.json");
            var generalBlock = BlockReader.GetBlockByName(blocks, "Общая информация");

            var filler = new DatabaseFiller();

            filler.Authentication(
                login: "admin@pravo.ru",
                password: "123123");

            filler.AddProjectGroup(
                projectGroupName: "test_new",
                folderName: "Тест (админ)");

            var projectId = filler.AddProject(
                projectName: "test-nproject",
                folderName: "Тест (админ)",
                projectTypeName: "1-я инстанция",
                responsibleName: "Casepro Admin",
                projectGroupName: "test_new");

            filler.AddGeneralInformation(
                projectId: projectId,
                generalBlock: generalBlock,
                excel: excel.First());
        }
    }
}
