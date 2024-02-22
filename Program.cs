using Microsoft.VisualBasic.FileIO;

using NLog;
string path = Directory.GetCurrentDirectory() + "\\nlog.config";
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();

string file = "temp.csv"; // change to movies.csv for production
string choice;

do
{
    Console.WriteLine("1) Read from file.");
    Console.WriteLine("2) Write to file.");
    Console.WriteLine("Any other key to exit.");
    choice = Console.ReadLine() ?? "";

    if (choice == "1")
    {
        if (File.Exists(file))
        {
            Console.WriteLine($"{"Movie ID",-10}{"Movie Title",-80}{"Movie Genres",-30}");
            Console.WriteLine($"{"--------",-10}{"-----------",-80}{"------------",-30}");

            StreamReader sr = new StreamReader(file);
            sr.ReadLine();

            int lineNumber = 1;

            while (!sr.EndOfStream)
            {
                lineNumber++;
                try
                {
                    string line = sr.ReadLine() ?? "";
                    TextFieldParser parser = new TextFieldParser(new StringReader(line));
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.SetDelimiters(",");

                    string[] fields = parser.ReadFields() ?? Array.Empty<string>();

                    Console.WriteLine($"{fields[0],-10}{fields[1],-80}{fields[2],-30}");

                    parser.Close();
                }
                catch
                {
                    Console.WriteLine($"Failed to parse line number {lineNumber}.");
                    logger.Error($"Failed to parse line number {lineNumber}.");
                }
            }

            sr.Close();
        }
        else
        {
            Console.WriteLine("File not found.");
            logger.Warn("File not found.");
        }
    }
    else if (choice == "2")
    {
        string resp = "Y";
        while (resp == "Y")
        {
            Console.WriteLine("Add a movie (Y/N)?");
            resp = Console.ReadLine().ToUpper();
            if (resp != "Y") { break; }

            Console.WriteLine("Movie ID:");
            string movieID = Console.ReadLine();
            Console.WriteLine("Movie Title:");
            string movieTitle = Console.ReadLine();
            Console.WriteLine("Movie Genres:");
            string movieGenres = Console.ReadLine();

            StreamReader sr = new StreamReader(file);
            string fileContents = sr.ReadToEnd();
            sr.Close();
            if (fileContents.EndsWith("\n"))
            {
                StreamWriter sw = new StreamWriter(file, true);
                sw.WriteLine("{0},{1},{2}", movieID, movieTitle, movieGenres);
                sw.Close();
            }
            else
            {
                StreamWriter sw = new StreamWriter(file, true);
                sw.WriteLine();
                sw.WriteLine("{0},{1},{2}", movieID, movieTitle, movieGenres);
                sw.Close();
            }
        }
    }

} while (choice == "1" || choice == "2");
