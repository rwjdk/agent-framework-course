using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Containers;
using OpenAI.Responses;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text;
using OpenAI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Samples.Section05;

public static class CodeInterpreter
{
    public static async Task RunSample()
    {
#pragma warning disable OPENAI001
        //Create Raw Connection
        string apiKey = SecretManager.GetOpenAIApiKey();
        OpenAIClient client = new OpenAIClient(apiKey);

        //Create Agent
        ChatClientAgent agent = client
            .GetResponsesClient("gpt-5-nano")
            .CreateAIAgent(
                tools:
                [
                    new HostedCodeInterpreterTool()
                ],
                instructions: "You can make charts using you 'code_interpreter' tool");

        AgentThread thread = agent.GetNewThread();

        Console.OutputEncoding = Encoding.UTF8;
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            AgentRunResponse response = await agent.RunAsync(input, thread);
            {
                Console.WriteLine(response);
                foreach (ChatMessage message in response.Messages)
                {
                    foreach (AIContent content in message.Contents)
                    {
                        foreach (AIAnnotation annotation in content.Annotations ?? [])
                        {
                            if (annotation.RawRepresentation is ContainerFileCitationMessageAnnotation containerFileCitation)
                            {
                                ContainerClient containerClient = client.GetContainerClient();
                                ClientResult<BinaryData> fileContent = await containerClient.DownloadContainerFileAsync(containerFileCitation.ContainerId, containerFileCitation.FileId);
                                string path = Path.Combine(Path.GetTempPath(), containerFileCitation.Filename);
                                await File.WriteAllBytesAsync(path, fileContent.Value.ToArray());
                                await Task.Factory.StartNew(() =>
                                {
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                    {
                                        FileName = path,
                                        UseShellExecute = true
                                    });
                                });
                            }
                        }
                    }
                }
            }

            Output.Separator();
        }
    }


    //Note:
    //Code Interpreter code $0.03 / session
}