using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

class CurrentDirectory
{
    public string path;
    public List<CurrentDirectory> SubdirectoryInstances { get; }
    public string Filename { get; }
    public Dictionary<string, object> Result { get; }
    public List<string> Files => GetWorkingDirectoryFiles().FindAll(item => File.Exists(Path.Combine(this.path, item)));
    public List<string> Directories => GetWorkingDirectoryFiles().FindAll(item => Directory.Exists(Path.Combine(this.path, item)));

    public CurrentDirectory(string path, string filename)
    {
        this.path = path;
        SubdirectoryInstances = new List<CurrentDirectory>();
        Filename = filename;
        GetSubdirectories();
        Result = ObjToDictionary();
    }

    public override string ToString()
    {
        return path;
    }

    public List<string> GetWorkingDirectoryFiles()
    {
        string[] fileArray = Directory.GetFiles(path);
        List<string> fileList = new List<string>(fileArray);
        return fileList;
    }

    public string GetFileExtension(string filename)
    {
        return Path.GetExtension(filename);
    }

    public void GetSubdirectories()
    {
        foreach (string subdir in Directories)
        {
            try
            {
                var subdirInstance = new CurrentDirectory(path + subdir, Filename);
                subdirInstance.GetSubdirectories();
                SubdirectoryInstances.Add(subdirInstance);
            }
            catch (UnauthorizedAccessException err)
            {
                Console.WriteLine(err);
            }
        }
    }

    public List<string> GetAllFileExtensions()
    {
        var extensions = new List<string>();
        foreach (string file in Files)
        {
            var fileExtension = GetFileExtension(path + file);
            if (!extensions.Contains(fileExtension))
            {
                extensions.Add(fileExtension);
            }
        }
        return extensions;
    }

    public void PrintFileExtensions()
    {
        Console.WriteLine($"Path of your directory is: {path}");
        Console.WriteLine("In this directory were found files with suffixes: ");
        var extensions = GetAllFileExtensions();
        foreach (var extension in extensions)
        {
            Console.WriteLine($"\t- {extension}");
        }
    }

    public Dictionary<string, object> ObjToDictionary()
    {
        var result = new Dictionary<string, object>
        {
            {"path", path},
            {"files", Files},
            {"subdirectories", SubdirectoryInstances.ConvertAll(subdir => subdir.ObjToDictionary())}
        };
        return result;
    }

    public void ToJson()
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        string json = JsonConvert.SerializeObject(Result, settings);
        File.WriteAllText(Filename, json);
    }

    public static CurrentDirectory FromJson(string jsonFilename, string output)
    {
        string json = File.ReadAllText(jsonFilename);
        var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        string path = (string)jsonData["path"];
        var instance = new CurrentDirectory(path, output);

        // Recursively create instances for subdirectories
        foreach (var subdirData in (List<object>)jsonData["subdirectories"])
        {
            var subdirInstance = FromDictionary(subdirData, jsonFilename);
            instance.SubdirectoryInstances.Add(subdirInstance);
        }

        return instance;
    }

    public static CurrentDirectory FromDictionary(object data, string jsonFilename)
    {
        var dataDict = (Dictionary<string, object>)data;
        string path = (string)dataDict["path"];
        var instance = new CurrentDirectory(path, jsonFilename);

        // Recursively create instances for subdirectories
        foreach (var subdirData in (List<object>)dataDict["subdirectories"])
        {
            var subdirInstance = FromDictionary(subdirData, jsonFilename);
            instance.SubdirectoryInstances.Add(subdirInstance);
        }

        return instance;
    }
}

class Program
{
    static void Main(string[] args)
    {
        string startingPath = GetValidFilePath();
        string outputJsonFile = "directory_structure.json";

        CurrentDirectory rootDirectory = new CurrentDirectory(startingPath, outputJsonFile);

        GetAction(rootDirectory);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static string GetValidFilePath()
    {
        string path;
        do
        {
            Console.WriteLine("Enter a valid path: ");
            path = Console.ReadLine().Trim();
        } while (!Directory.Exists(path));

        return path;
    }

    static void GetAction(CurrentDirectory instance)
    {
        while (true)
        {
            Console.WriteLine("You can:");
            Console.WriteLine("  1. print all file extensions in directory tree");
            Console.WriteLine("  2. create a JSON file representing directory tree");
            Console.WriteLine("  3. exit");

            Console.Write("Which one do you want to choose? ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                instance.PrintFileExtensions();
            }
            else if (choice == "2")
            {
                instance.ToJson();
            }
            else if (choice == "3")
            {
                return;
            }
        }
    }
}