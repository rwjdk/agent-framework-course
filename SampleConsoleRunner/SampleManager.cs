using System.ComponentModel;
using System.Reflection;
using Samples.SampleUtilities;
using Samples.Section03;
using Samples.Section04;
using Samples.Section05;
using Samples.Section07;

namespace SampleConsoleRunner;

//This class deal with sample-selection and have nothing to do with the course as such
public static class SampleManager
{
    public static async Task RunSample(Sample sample)
    {
        Console.Clear();
        Console.ResetColor();

        if (sample == Sample.Interactive)
        {
            //Choose sample via interactivity
            Console.WriteLine("Available Samples");
            Output.Separator(false);
            Sample[] samplesToChooseFrom = Enum.GetValues<Sample>().Except([Sample.Interactive]).ToArray();
            IEnumerable<IGrouping<string, SampleDetails>> groups = samplesToChooseFrom.Select(x => x.GetDetails()).GroupBy(x => x.Section);
            foreach (IGrouping<string, SampleDetails> group in groups)
            {
                List<SampleDetails> values = group.ToList();
                Output.Title(group.Key);
                string samplesInSection = string.Join(" ", values);
                Console.WriteLine("- " + samplesInSection);
                Output.Separator(false);
            }

            Console.WriteLine("Enter the number of the sample you wish to run");
            Console.Write("> ");
            string input = Console.ReadLine() ?? string.Empty;
            int number = Convert.ToInt32(input); //todo - better
            sample = (Sample)number;
        }

        Console.Clear();

        Output.Gray("Running sample: " + sample);
        switch (sample)
        {
            case Sample.TokenUsage:
                await TokenUsage.RunSample();
                break;
            case Sample.NormalVsStreamingResponse:
                await NormalVsStreamingResponse.RunSample();
                break;
            case Sample.Chatloop:
                await Chatloop.RunSample();
                break;
            case Sample.Instructions:
                await Instructions.RunSample();
                break;
            case Sample.CreatingTools:
                await CreatingTools.RunSample();
                break;
            case Sample.ConsumingMcp:
                await ConsumingMcpTools.RunSample();
                break;
            case Sample.ToolCallingMiddleware:
                await ToolCallingMiddleware.RunSample();
                break;
            case Sample.OtherAgentsAsTools:
                await OtherAgentsAsTools.RunSample();
                break;
            case Sample.WebSearch:
                await WebSearch.RunSample();
                break;
            case Sample.StructuredOutputManual:
                await StructuredOutput.RunSample();
                break;
            case Sample.Interactive:
            default:
                Console.WriteLine("No sample with that number :-(");
                break;
        }

        Output.Gray("--- Done ---");
        Console.ReadLine();
    }
}

public enum Sample
{
    Interactive = 0,

    [SampleDetails("Token Usage", SampleSection.Section3)]
    TokenUsage = 300,

    [SampleDetails("Streaming Response", SampleSection.Section4)]
    NormalVsStreamingResponse = 400,

    [SampleDetails("Chat-loop (AgentThread)", SampleSection.Section4)]
    Chatloop = 401,

    [SampleDetails("Instructions (Prompt Engineering)", SampleSection.Section4)]
    Instructions = 402,

    [SampleDetails("Creating Tools", SampleSection.Section5)]
    CreatingTools = 600,

    [SampleDetails("Consuming MCP Servers", SampleSection.Section5)]
    ConsumingMcp = 601,

    [SampleDetails("CodeInterpreter Tool", SampleSection.Section5)]
    CodeInterpreter = 602,

    [SampleDetails("Web Search Tool", SampleSection.Section5)]
    WebSearch = 603,

    [SampleDetails("Other Agents as Tools", SampleSection.Section5)]
    OtherAgentsAsTools = 604,

    [SampleDetails("Tool Calling Middleware", SampleSection.Section5)]
    ToolCallingMiddleware = 605,

    [SampleDetails("Structured Output", SampleSection.Section6)]
    StructuredOutputBasic = 700,

    [SampleDetails("Structured Output (More manual way)", SampleSection.Section6)]
    StructuredOutputManual = 701,
}

public enum SampleSection
{
    [Description("Introduction to the course")]
    Section1,

    [Description("Hello World (Zero to first Prompt)")]
    Section2,

    [Description("Before we dive deeper...")]
    Section3,

    [Description("Chat")]
    Section4,

    [Description("Tool Calling")]
    Section5,

    [Description("Structured Output")]
    Section6,

    [Description("Intermission: The Life of an LLM Call")]
    Section7,

    [Description("RAG (Retrieval Augmented Generation)")]
    Section8,

    [Description("Advanced Agent Topics")]
    Section9,

    [Description("Agent Framework Toolkit")]
    Section10,

    [Description("Introduction to the Workflow Part of Microsoft Agent Framework")]
    Section11,

    [Description("Bonus Topics")]
    Section12,
}

public class SampleDetailsAttribute(string name, SampleSection section) : Attribute
{
    public string Name { get; } = name;
    public SampleSection Section { get; set; } = section;
}

public class SampleDetails
{
    public required int Number { get; set; }
    public required string Name { get; set; }
    public required string Section { get; set; }

    public override string ToString()
    {
        return $"[{Number}: {Name}]";
    }
}

public static class EnumExtensions
{
    public static SampleDetails GetDetails(this Sample sample)
    {
        Type enumType = sample.GetType();
        string name = Enum.GetName(enumType, sample) ?? throw new InvalidOperationException($"Name is null for {sample} ({enumType})");
        FieldInfo field = enumType.GetField(name) ?? throw new InvalidOperationException($"Field is null for {sample} ({enumType})");
        SampleDetailsAttribute attribute = field.GetCustomAttribute<SampleDetailsAttribute>()!;
        return new SampleDetails
        {
            Number = Convert.ToInt32(sample),
            Name = attribute.Name,
            Section = attribute.Section.ToString() + ": " + attribute.Section.Description()!
        };
    }

    public static string? Description(this Enum enumValue)
    {
        Type enumType = enumValue.GetType();
        string name = Enum.GetName(enumType, enumValue) ?? throw new InvalidOperationException($"Name is null for {enumValue} ({enumType})");
        FieldInfo field = enumType.GetField(name) ?? throw new InvalidOperationException($"Field is null for {enumValue} ({enumType})");
        return field.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }
}