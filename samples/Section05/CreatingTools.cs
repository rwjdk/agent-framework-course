using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text;

namespace Samples.Section05;

public static class CreatingTools
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        PersonTools personTools = new PersonTools();

        //Create Agent
        ChatClientAgent agent = client
            .GetChatClient("gpt-4.1-nano")
            .AsAIAgent(
                instructions: "You know people and can change colors of the Console ('change_console_color')",
                tools:
                [
                    AIFunctionFactory.Create(personTools.GetPersons, "get_persons", "Get all the persons you know"),
                    AIFunctionFactory.Create(personTools.GetPerson, "get_person", "Get a specific person by name"),
                    AIFunctionFactory.Create(ChangeConsoleColor, "change_console_color", "Change the color of the console")
                ]);

        AgentSession session = await agent.GetNewSessionAsync();;

        Console.OutputEncoding = Encoding.UTF8;
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            AgentResponse response = await agent.RunAsync(input, session);
            {
                Console.WriteLine(response);
            }

            Output.Separator();
        }
    }

    public record PersonInfo(string Name, string FavoriteColor);

    //Information Tool
    public class PersonTools
    {
        public PersonInfo[] GetPersons()
        {
            Output.Gray("(GetPersons was called)");
            return GetData();
        }

        public PersonInfo? GetPerson(string name)
        {
            Output.Gray($"(GetPerson was called with '{name}')");
            PersonInfo[] data = GetData();
            return data.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        private static PersonInfo[] GetData()
        {
            return
            [
                new PersonInfo("Rasmus", "Blue"),
                new PersonInfo("John", "Red"),
                new PersonInfo("Ben", "Green"),
                new PersonInfo("Jenny", "Red"),
                new PersonInfo("Mona", "Yellow"),
            ];
        }
    }

    //Action Tool
    public static void ChangeConsoleColor(ConsoleColor color)
    {
        Output.Gray($"(ChangeConsoleColor was called with '{color}')");
        Console.ForegroundColor = color;
    }
}