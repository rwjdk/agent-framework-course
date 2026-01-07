// ReSharper disable UnreachableSwitchCaseDueToIntegerAnalysis

using SampleConsoleRunner;

Console.Clear();
Console.ResetColor();

//Choose Interactive to choose on the fly or set the sample you wish to run
await SampleManager.RunSample(Sample.Interactive);