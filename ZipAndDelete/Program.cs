using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;
//using static System.Net.Mime.MediaTypeNames;

namespace ZipAndDelete
{
  internal class Program
  {
    static void Main(string[] arguments)
    {
      Action<string> Display = Console.WriteLine;
      Display($"Program ZipAndDelete.exe written by Freddy Juhel on the 21/11/2022, version {GetAssemblyVersion()}");
      Display(string.Empty);
      if (arguments.Length == 0 || arguments[0].ToLower().Contains("help") || arguments[0].Contains("?"))
      {
        DisplayUsage();
        return;
      }

      var argumentDictionary = new Dictionary<string, string>
      {
        // Initialization of the argument dictionary with default values
        {"directory", "."},
        {"includesubdirectories", "false"},
        {"extensionfilenamepattern", "txt" },
        {"exclusionextensionfilenamepattern", ".exe,.dll,.config" },
        {"compressionlevel", "maximum9" },
        {"deleteaftercompression", "false" },
        {"addextensionifnone", "false" },
        {"extensiontobeaddedifnone", "txt" },
        {"log", "false"}
      };

      // the variable numberOfInitialDictionaryItems is used for the log to list all non-standard arguments passed in.
      int numberOfInitialDictionaryItems = argumentDictionary.Count;
      int numberOfFilesZipped = 0;
      int numberOfFilesDeletedAfterBeingZipped = 0;
      bool hasExtraArguments = false;
      string datedLogFileName = $"ZipAndDeleteLogFile-{DateTime.Now.ToShortDateString().Replace('/', '-')}.log";
      bool deleteFileAfterBeingZipped = false;

      // we split arguments into the dictionary
      foreach (string argument in arguments)
      {
        string argumentKey = string.Empty;
        string argumentValue = string.Empty;
        if (argument.IndexOf('=') != -1)
        {
          argumentKey = argument.Substring(1, argument.IndexOf('=') - 1).ToLower();
          argumentValue = argument.Substring(argument.IndexOf('=') + 1, argument.Length - (argument.IndexOf('=') + 1));
        }
        else
        {
          // If we have an argument without the equal sign (=) then we add it to the dictionary
          argumentKey = argument;
          argumentValue = "The argument passed in does not have any value. The equal sign (=) is missing.";
        }

        if (argumentDictionary.ContainsKey(argumentKey))
        {
          // set the value of the argument
          argumentDictionary[argumentKey] = argumentValue;
        }
        else
        {
          // we add any other or new argument into the dictionary to look at them in the log
          argumentDictionary.Add(argumentKey, argumentValue);
          hasExtraArguments = true;
        }
      }

      // Add version of the program at the beginning of the log
      Log(datedLogFileName, argumentDictionary["log"], $"{Assembly.GetExecutingAssembly().GetName().Name} is in version {GetAssemblyVersion()}");

      // We log all arguments passed in.
      foreach (KeyValuePair<string, string> keyValuePair in argumentDictionary)
      {
        if (argumentDictionary["log"] == "true")
        {
          Log(datedLogFileName, argumentDictionary["log"], $"Argument requested: {keyValuePair.Key}");
          Log(datedLogFileName, argumentDictionary["log"], $"Value of the argument: {keyValuePair.Value}");
        }
      }

      //we log extra arguments
      if (hasExtraArguments && argumentDictionary["log"] == "true")
      {
        Log(datedLogFileName, argumentDictionary["log"], "Here are a list of argument passed in but not understood and thus not used (for debug purpose only).");
        for (int i = numberOfInitialDictionaryItems; i <= argumentDictionary.Count - 1; i++)
        {
          Log(datedLogFileName, argumentDictionary["log"], $"Extra argument requested: {argumentDictionary.Keys.ElementAt(i)}");
          Log(datedLogFileName, argumentDictionary["log"], $"Value of the extra argument: {argumentDictionary.Values.ElementAt(i)}");
        }
      }

      if (argumentDictionary["deleteaftercompression"].ToLower() == "true")
      {
        deleteFileAfterBeingZipped = true;
        Log(datedLogFileName, "true", "The variable deleteFileAfterBeingZipped has been set to true so files will be deleted after being zipped.");
      }

      // zipping files start here
      foreach (string filename in Directory.GetFiles(argumentDictionary["directory"], $"*.{argumentDictionary["extensionfilenamepattern"]}"))
      {
        try
        {
          // add !filename.EndsWith(argumentDictionary["exclusionextensionfilenamepattern"].Split(',').Any())
          if (filename.EndsWith(argumentDictionary["extensionfilenamepattern"]) && !IsFileBinary(filename, argumentDictionary["exclusionextensionfilenamepattern"]))
          {
            // zip the file
            ZipFiles(argumentDictionary["directory"], new List<string> { $"{filename}" }, $"{filename}.zip");
            Log(datedLogFileName, "true", $"The file {filename} has been zipped, its new name is {filename}.zip");
            Display($"{datedLogFileName} - the file {filename} has been zipped, its new name is {filename}.zip");
            numberOfFilesZipped++;

            if (deleteFileAfterBeingZipped)
            {
              File.Delete($"{Path.Combine(argumentDictionary["directory"], $"{filename}")}");
              Log(datedLogFileName, "true", $"The file {filename} has been deleted.");
              Display($"{datedLogFileName} - The file {filename} has been deleted.");
              numberOfFilesDeletedAfterBeingZipped++;
            }
          }
          else if (argumentDictionary["extensionfilenamepattern"] == "*")
          {
            // if no old extension is a star meaning it is for all files
            // We don't zip the application itself and its config file.
            if (filename.ToLower() != "ZipAndDelete.exe" || filename.ToLower() != "ZipAndDelete.exe.config" || filename.ToLower() != "icsharpcode.sharpziplib.dll" || !filename.StartsWith("ZipAndDeleteLog"))
            {
              try
              {
                // zip the file
                ZipFiles(argumentDictionary["directory"], new List<string> { $"{filename}" }, $"{filename}.zip");
                Log(datedLogFileName, "true", $"The file {filename} has been zipped, its new name is {filename}.zip");
                Display($"{datedLogFileName} - The file {filename} has been zipped, its new name is {filename}.zip");
                numberOfFilesZipped++;

                if (deleteFileAfterBeingZipped)
                {
                  File.Delete($"{Path.Combine(argumentDictionary["directory"], $"{filename}")}");
                  Log(datedLogFileName, "true", $"The file {filename} has been deleted.");
                  Display($"{datedLogFileName} - The file {filename} has been deleted.");
                  numberOfFilesDeletedAfterBeingZipped++;
                }
              }
              catch (Exception)
              {
                // catch nothing and continue
                Program.Display($"An error occured because the file: {filename} couldn't be zipped.");
              }
            }
          }
        }
        catch (Exception exception)
        {
          Program.Display("error while trying to zip files");
          Program.Display($"The exception is {exception.Message}");
        }
      }

      Display("Press any key to exit:");
      Console.ReadKey();
    }

    /// <summary>Zip all files in the same directory and delete them afterwards.</summary>
    /// <param name="folderPath">The path where to zip files.</param>
    /// <param name="listOfFiles">The list of all the files to be zipped.</param>
    /// <param name="zipFileName">The name of the zip file.</param>
    /// <param name="zipLevel">
    /// The level of compression, from 0 to 9, 0 is the lowest compression and 9 is the highest compression, 9 by default.
    /// </param>
    public static void ZipFiles(string folderPath, IEnumerable<string> listOfFiles, string zipFileName, int zipLevel = 9)
    {
      ZipOutputStream zipStream = null;
      try
      {
        if (File.Exists(Path.Combine(folderPath, zipFileName)))
        {
          zipStream = new ZipOutputStream(File.OpenWrite(Path.Combine(folderPath, zipFileName))); // open existing zip file
        }
        else
        {
          zipStream = new ZipOutputStream(File.Create(Path.Combine(folderPath, zipFileName))); // create new zip file
        }

        using (zipStream)
        {
          if (zipLevel < 0 || zipLevel > 9)
          {
            zipLevel = 9; // 9 is the maximum level of compression, we take it by default (level from 0 to 9)
          }

          zipStream.SetLevel(zipLevel);
          foreach (string fileName in listOfFiles)
          {
            string filenameCorrected = fileName;
            if (fileName.StartsWith("."))
            {
              filenameCorrected = fileName.Substring(2, fileName.Length - 2);
            }

            ZipEntry zipEntry = new ZipEntry(filenameCorrected);
            zipStream.PutNextEntry(zipEntry);
            using (FileStream fileStream = File.OpenRead(Path.Combine(folderPath, fileName)))
            {
              byte[] buffer = new byte[fileStream.Length];
              fileStream.Read(buffer, 0, buffer.Length);
              zipStream.Write(buffer, 0, buffer.Length);
            }
          }
        }
      }
      catch (Exception exception)
      {
        Console.WriteLine($"Exception found: {exception.Message}");
        Console.WriteLine("press any key to continue to next exception:");
        Console.ReadKey();
      }
      finally
      {
        if (zipStream != null)
        {
          zipStream.Finish();
          zipStream.Close();
          zipStream.Dispose();
        }
      }
    }

    private enum CompressionLevel
    {
      None = 10,
      CompressionLevel0 = 0,
      CompressionLevel1 = 1,
      CompressionLevel2 = 2,
      CompressionLevel3 = 3,
      CompressionLevel4 = 4,
      CompressionLevel5 = 5,
      CompressionLevel6 = 6,
      CompressionLevel7 = 7,
      CompressionLevel8 = 8,
      CompressionLevel9 = 9,
    }

    private static void Display(string message)
    {
      Console.WriteLine(message);
    }

    /// <summary>
    /// The log file to record all activities.
    /// </summary>
    /// <param name="filename">The name of the file.</param>
    /// <param name="logging">Do we log or not? if true yes otherwise no log.</param>
    /// <param name="message">The message to be logged.</param>
    private static void Log(string filename, string logging, string message)
    {
      if (logging.ToLower() != "true") return;
      if (filename.Trim() == string.Empty) return;
      try
      {
        StreamWriter sw = new StreamWriter(filename, true);
        sw.WriteLine($"{DateTime.Now} - {message}");
        sw.Close();
      }
      catch (Exception exception)
      {
        Console.WriteLine($"There was an error while writing the file: {filename}. The exception is:{exception}");
      }
    }

    private static void DisplayUsage()
    {
      Action<string> display = Console.WriteLine;
      display(string.Empty);
      display("ZipAndDelete is a console application written by Freddy Juhel on the 20th of November 2022.");
      display($"ZipAndDelete.exe is in version {GetAssemblyVersion()}");
      display("ZipAndDelete needs Microsoft .NET framework 4.7.2 to run, if you don't have it, download it from www.microsoft.com.");
      display("Copyright (c) MIT 2022 by Freddy Juhel.");
      display(string.Empty);
      display("Usage of this program:");
      display(string.Empty);
      display("List of arguments:");
      display(string.Empty);
      display("/help (this help)");
      display("/? (this help)");
      display(string.Empty);
      display(
        "You can write argument name (not its value) in uppercase or lowercase or a mixed of them (case insensitive)");
      display("/compressionlevel is the same as /Compressionlevel or /CompressionLevel or /COMPRESSIONLEVEL");
      display(string.Empty);
      display("/directory=<name of the directory where files will be zipped> default is where ZipAndDelete.exe is");
      display(string.Empty);
      display("/includesubdirectories=<true or false> false by default");
      display(string.Empty);
      display("/log=<true or false> false by default");
      display(string.Empty);
      display("/extensionfilenamepattern=<any kind of extension file name> default is txt");
      display(string.Empty);
      display("/exclusionextensionfilenamepattern=<any kind of extension file name> default is .exe,.dll,.config");
      display(string.Empty);
      display("/compressionlevel=<any number between 0 to 9>, default is 9");
      display(string.Empty);
      display("/deleteaftercompression=<true or false> default is false");
      display(string.Empty);
      display("/addextensionifnone=<true or false> default is false");
      display(string.Empty);
      display("/extensiontobeaddedifnone=<any kind of extension file name> default is txt");
      display(string.Empty);
      display("/log=<true or false> default is false");
      display(string.Empty);
      display("Examples:");
      display(string.Empty);
      display(@"ZipAndDelete /directory=. /extensionfilenamepattern=txt /log=true");
      display(string.Empty);
      display("ZipAndDelete /help (this help)");
      display("ZipAndDelete /? (this help)");
      display(string.Empty);
    }

    public static string GetAssemblyVersion()
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
      return string.Format("V{0}.{1}.{2}.{3}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
    }

    public static bool IsFileBinary(string filename, string commaSeparatedBinaryExtensions)
    {
      bool result = false;
      foreach (string extension in commaSeparatedBinaryExtensions.Split(','))
      {
        if (Path.GetExtension(filename) == extension)
        {
          return true;
        }
      }

      return result;
    }

    public static string Tabulate(int numberOfSpace = 0)
    {
      return new String(' ', numberOfSpace);
    }
  }
}
