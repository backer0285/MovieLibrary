using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

using NLog;
string path = Directory.GetCurrentDirectory() + "\\nlog.config";
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();

string file = "movies.csv";
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

            // assumes first line of csv file is a header, rather than data
            int lineNumber = 1;

            while (!sr.EndOfStream)
            {
                lineNumber++;
                try
                {
                    string line = sr.ReadLine() ?? "";
                    TextFieldParser parser = new TextFieldParser(new StringReader(line));

                    // deals with commas embedded in fields when quotes surround the field, throws failed to parse line error if quotes are within quotes
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.SetDelimiters(",");

                    string[] fields = parser.ReadFields() ?? Array.Empty<string>();

                    fields[2] = fields[2].Replace("|", ", ");

                    Console.WriteLine($"{fields[0],-20}{fields[1],-100}{fields[2],-30}");

                    if (fields.Length > 3)
                    {
                        Console.WriteLine($"Row {lineNumber} in CSV file contained more than 3 fields. Some data was excluded.");
                        logger.Warn($"Row {lineNumber} in CSV file contained more than 3 fields. Some data was excluded.");
                    }

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

        // generates list of ID's and Titles to check against user entry for duplicate avoidance
        List<string> fileIDs = new List<string>();
        List<string> fileTitles = new List<string>();

        if (File.Exists(file))
        {
            StreamReader sr = new StreamReader(file);
            sr.ReadLine();

            int lineNumber = 0;

            while (!sr.EndOfStream)
            {
                lineNumber++;
                try
                {
                    string line = sr.ReadLine() ?? "";
                    TextFieldParser parser = new TextFieldParser(new StringReader(line));

                    // deals with commas embedded in fields when quotes surround the field, throws failed to parse line error if quotes are within quotes
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.SetDelimiters(",");

                    string[] fields = parser.ReadFields() ?? Array.Empty<string>();

                    fileIDs.Add(fields[0]);
                    fileTitles.Add(fields[1]);

                    parser.Close();
                }
                catch
                {
                    Console.WriteLine($"Check for duplicates failed to parse line number {lineNumber}.");
                    logger.Error($"Check for duplicates failed to parse line number {lineNumber}.");
                }
            }

            sr.Close();
        }
        else
        {
            Console.WriteLine("Check for duplicates file not found.");
            logger.Warn("Check for duplicates file not found.");
        }

        while (resp == "Y")
        {
            Console.WriteLine("Add a movie (Y/N)?");
            resp = (Console.ReadLine() ?? "").ToUpper();

            if (resp != "Y") { break; }

            // if blocks check for embedded commas in fields and apply quotes around them to maintain reading ability
            Console.WriteLine("Movie ID:");
            string movieID = Console.ReadLine() ?? "";
            if (movieID.Contains(','))
            {
                movieID = "\"" + movieID + "\"";
            }
            if (fileIDs.Contains(movieID))
            {
                Console.WriteLine("Movie ID already in database.");
                break;
            }

            Console.WriteLine("Movie Title:");
            string movieTitle = Console.ReadLine() ?? "";
            if (movieTitle.Contains(','))
            {
                movieTitle = "\"" + movieTitle + "\"";
            }
            if (fileTitles.Contains(movieTitle))
            {
                Console.WriteLine("Movie title already in database.");
                break;
            }

            string movieGenres = "";

            string genreResp = "Y";
            while (genreResp == "Y")
            {
                Console.WriteLine("Add a genre (Y/N)?");
                genreResp = (Console.ReadLine() ?? "").ToUpper();
                if (genreResp != "Y") { break; }

                Console.WriteLine("Movie Genre:");
                string movieGenre = Console.ReadLine() ?? "";

                if (movieGenres == "")
                {
                    movieGenres = movieGenre;
                }
                else
                {
                    movieGenres = movieGenres + "|" + movieGenre;
                }
            }

            if (movieGenres == "")
            {
                movieGenres = "(no genres listed)";
            }

            if (movieGenres.Contains(','))
            {
                movieGenres = "\"" + movieGenres + "\"";
            }

            if (File.Exists(file))
            {
                StreamReader sr = new StreamReader(file);
                string fileContents = sr.ReadToEnd();
                sr.Close();
                StreamWriter sw = new StreamWriter(file, true);

                // ensures no run-on fields regardless of whether csv file ends with blank line or not
                if (!fileContents.EndsWith("\n"))
                {
                    sw.WriteLine();
                }
                sw.WriteLine("{0},{1},{2}", movieID, movieTitle, movieGenres);
                sw.Close();
            }
            else
            {
                Console.WriteLine("File not found.");
                logger.Warn("File not found.");
            }
        }
    }
} while (choice == "1" || choice == "2");
