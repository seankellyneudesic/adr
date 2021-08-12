using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace adr
{
    class InputItem
    {
        public string QuestionText { get; set; }
        public string Answer { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
    }

    class PathStatuses
    {
        public bool SubPath { get; set; }
        public bool AdrCounterPath { get; set; }
        public bool AdrDocPath { get; set; }
    }

    class Program
    {

        const string MarkdownIndexTemplate = @"NEXTADB=0001";

        const string MarkdownTemplate =
@"# {NUMBER}. {TITLE}

Date: {DATE}

## Status

{STATUS}

## Context

{CONTEXT}

## Decision

{DECISION}

## Consequences

{CONSEQUENCES}";

        static Dictionary<string, InputItem> InputItems = new Dictionary<string, InputItem> {
            { "TITLE", new InputItem { QuestionText = "Please enter the title:", IsRequired = true }},
            { "DATE", new InputItem { QuestionText = "Please enter the date:", IsRequired = true, DefaultValue = DateTime.Now.ToString("yyyy-MM-dd") }},
            { "STATUS", new InputItem { QuestionText = "Please enter the status:", IsRequired = true }},
            { "CONTEXT", new InputItem { QuestionText = "Please enter the context:", IsRequired = true }},
            { "DECISION", new InputItem { QuestionText = "Please enter the decision:", IsRequired = true }},
            { "CONSEQUENCES", new InputItem { QuestionText = "Please enter the consequences:", IsRequired = true }}
        };
        const string TitleTemplate = @"{NUMBER}-{TITLE}.md";
        const string TITLE_KEY = "TITLE";
        static string SubPath = AppContext.BaseDirectory + ".adr";

        static string AdrCounterPath = SubPath + @"\adrcounter.txt";
        static string AdrDocPath = AppContext.BaseDirectory + @"docs\adr";

        static bool ValidateFilePath(string file)
        {
            return System.IO.File.Exists(file);
        }

        static bool ValidatePath(string directory)
        {
            return System.IO.Directory.Exists(directory);
        }

        static bool InitializeAdrForRepo(bool createIfDoesNotExist = true)
        {
            var statuses = new PathStatuses
            {
                SubPath = ValidatePath(SubPath),
                AdrCounterPath = ValidateFilePath(AdrCounterPath),
                AdrDocPath = ValidatePath(AdrDocPath)
            };

            if (statuses.SubPath)
                Console.WriteLine(@"ADR data directory \.adr already exists, skipping...");
            else
            {
                if (createIfDoesNotExist)
                {
                    System.IO.Directory.CreateDirectory(SubPath);
                    Console.WriteLine(@"Successfully created ADR data directory \.adr");
                }
                else
                    Console.WriteLine(@"ERROR: ADR data directory does not exist");
            }

            if (statuses.AdrCounterPath)
                Console.WriteLine(@"ADR Counter File already exists, skipping...");
            else
            {
                if (createIfDoesNotExist)
                {
                    System.IO.File.WriteAllText(AdrCounterPath, MarkdownIndexTemplate);
                    Console.WriteLine(@"Successfully created ADR Counter File in \.adr");
                }
                else
                    Console.WriteLine(@"ERROR: ADR counter file does not exist");
            }

            var docExists = ValidatePath(AdrDocPath);
            if (docExists)
            {
                Console.WriteLine(@"ADR Doc directory already exists, skipping...");
            }
            else
            {
                if (createIfDoesNotExist)
                {
                    System.IO.Directory.CreateDirectory(AdrDocPath);
                    Console.WriteLine(@"Successfully created ADR Doc directory. in docs\adr");
                }
                else
                    Console.WriteLine(@"ERROR: ADR Doc directory does not exist");
            }

            //check again after any creation
            statuses = new PathStatuses
            {
                SubPath = ValidatePath(SubPath),
                AdrCounterPath = ValidateFilePath(AdrCounterPath),
                AdrDocPath = ValidatePath(AdrDocPath)
            };
            return statuses.AdrCounterPath && statuses.AdrDocPath && statuses.SubPath;
        }

        static string GetAdrSequenceNumber()
        {
            return System.IO.File.ReadAllText(AdrCounterPath).Split("=")[1];
        }

        static string UpdateAdrToNextInSequence()
        {
            var data = System.IO.File.ReadAllText(AdrCounterPath).Split("=");
            var nextNumString = (Int16.Parse(data[1]) + 1).ToString("0000");
            var nextSequence = $"{data[0]}={nextNumString}";
            System.IO.File.WriteAllText(AdrCounterPath, nextSequence);
            return nextNumString;
        }


        static void WriteAdrRecord(Dictionary<string, InputItem> items)
        {
            var nextAdrNumber = GetAdrSequenceNumber();
            var output = MarkdownTemplate.Replace("{NUMBER}", Int32.Parse(nextAdrNumber).ToString());

            foreach (var item in items)
            {
                output = output.Replace($"{{{item.Key}}}", item.Value.Answer);
            }

            var normalizedTitle = items.First(x => x.Key == TITLE_KEY).Value.Answer.ToLower().Replace(" ", "-").Replace("'", "").Replace(@"""", @"");
            var fileName = TitleTemplate.Replace("{NUMBER}", nextAdrNumber).Replace("{TITLE}", normalizedTitle);

            System.IO.File.WriteAllText(AdrDocPath + $"\\{fileName}", output);

            var nextNum = UpdateAdrToNextInSequence();

            Console.WriteLine($"Successfully created ADR file {AdrDocPath}\\{fileName}.");
            Console.WriteLine($"Next number in sequence will be {nextNum}.");
        }
        static void CreateAdrRecord()
        {
            var fileCheck = InitializeAdrForRepo(false);
            if (!fileCheck) return;

            foreach (var topic in InputItems)
            {
                do
                {
                    Console.WriteLine(topic.Value.QuestionText);
                    var consoleRead = Console.ReadLine();
                    topic.Value.Answer = string.IsNullOrEmpty(consoleRead) ? topic.Value.DefaultValue : consoleRead;
                } while (string.IsNullOrEmpty(topic.Value.Answer) && topic.Value.IsRequired);
            }
            WriteAdrRecord(InputItems);

            // create the adr record
        }

        static void OutputNoCommandInfo()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("adr {command}");
            Console.WriteLine("    where command is [create, supercede]");
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                OutputNoCommandInfo();
                return;
            }

            switch (args[0].ToUpper())
            {
                case "INIT":
                    InitializeAdrForRepo();
                    break;
                case "CREATE":
                    CreateAdrRecord();
                    break;
                default:
                    OutputNoCommandInfo();
                    break;
            }
        }
    }
}
