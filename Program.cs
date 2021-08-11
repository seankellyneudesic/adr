using System;
using System.Collections;
using System.Collections.Generic;

namespace adr
{
    class InputItem
    {
        public string QuestionText { get; set; }
        public string Answer { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
    }
    class Program
    {

        const string MarkdownIndexTemplate = @"
            NEXTADB=0001
        ";

        const string MarkdownTemplate = @"
            # {NUMBER}. {TITLE}

            Date: {DATE}

            ## Status

            {STATUS}

            ## Context

            {CONTEXT}

            ## Decision

            {DECISION}

            ## Consequences

            {CONSEQUENCES}
        ";

        static Dictionary<string, InputItem> InputItems = new Dictionary<string, InputItem> {
            { "TITLE", new InputItem { QuestionText = "Please enter the title:", IsRequired = true }},
            { "DATE", new InputItem { QuestionText = "Please enter the date:", IsRequired = true, DefaultValue = DateTime.Now.ToString("yyyy-MM-dd") }},
            { "STATUS", new InputItem { QuestionText = "Please enter the status:", IsRequired = true }},
            { "CONTEXT", new InputItem { QuestionText = "Please enter the context:", IsRequired = true }},
            { "DECISION", new InputItem { QuestionText = "Please enter the decision:", IsRequired = true }},
            { "CONSEQUENCES", new InputItem { QuestionText = "Please enter the consequences:", IsRequired = true }}
        };
        const string TitleTemplate = @"{0000}-{title}.md";

        static void CreateDirectoryIfDoesNotExist()
        {
            var exists = ValidateInit();
            if (!exists.Item1)
            {
                System.IO.Directory.CreateDirectory(exists.Item2);
                Console.WriteLine(@"Successfully created ADB data directory. in \.adb");
            }
        }

        static void CreateMarkdownCounterFile()
        {
            var path = ValidateInit();
            string indexFilePath = path.Item2 + @"\record.txt";
            var doesIndexFileExist = System.IO.File.Exists(indexFilePath);

            if (doesIndexFileExist)
            {
                Console.WriteLine($"{ indexFilePath } exists, skipping...");
                return;
            }

            Console.WriteLine($"{ indexFilePath } does not exist, creating...");
            System.IO.File.WriteAllText(indexFilePath, MarkdownIndexTemplate);
            Console.WriteLine("Successfully created index file.");
        }
        static void InitializeAdrForRepo()
        {
            CreateDirectoryIfDoesNotExist();
            CreateMarkdownCounterFile();
        }

        static Tuple<bool, string> ValidateInit(bool reportErrorIfDoesNotExist = false)
        {
            string subPath = AppContext.BaseDirectory + ".adr";
            bool exists = System.IO.Directory.Exists(subPath);
            if (!exists && reportErrorIfDoesNotExist)
                Console.WriteLine("ERROR: ADR RECORDS MUST BE INITIALZIED, use 'adr init' first.");
            return new Tuple<bool, string>(exists, subPath);
        }
        static void CreateAdrRecord()
        {
            foreach (var topic in InputItems)
            {
                do
                {
                    Console.WriteLine(topic.Value.QuestionText);
                    topic.Value.Answer = Console.ReadLine();
                } while (string.IsNullOrEmpty(topic.Value.Answer) && topic.Value.IsRequired)
            }
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
                    bool valid = ValidateInit(true).Item1;
                    if (valid)
                        CreateAdrRecord();
                    break;
                default:
                    OutputNoCommandInfo();
                    break;
            }
        }
    }
}
