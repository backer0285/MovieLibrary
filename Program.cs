using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

using NLog;
string path = Directory.GetCurrentDirectory() + "\\nlog.config";
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();

string file = "movies.csv"; // TODO - change to movies.csv for production
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
        while (resp == "Y")
        {
            Console.WriteLine("Add a movie (Y/N)?");
            resp = (Console.ReadLine() ?? "").ToUpper();

            if (resp != "Y") { break; }

            // TODO - check for duplicates

            // if blocks check for embedded commas in fields and apply quotes around them to maintain reading ability
            Console.WriteLine("Movie ID:");
            string movieID = Console.ReadLine() ?? "";
            if (movieID.Contains(','))
            {
                movieID = "\"" + movieID + "\"";
            }

            Console.WriteLine("Movie Title:");
            string movieTitle = Console.ReadLine() ?? "";
            if (movieTitle.Contains(','))
            {
                movieTitle = "\"" + movieTitle + "\"";
            }

            string movieGenres = "";

            Console.WriteLine("Add a genre (Y/N)?");
            string genreResp = (Console.ReadLine() ?? "").ToUpper();

            while (genreResp == "Y")
            {
                Console.WriteLine("Movie Genre:");
                string movieGenre = Console.ReadLine() ?? "";

                if (genreResp != "Y") { break; }

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
