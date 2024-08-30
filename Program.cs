using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Program
{
    class MainInit
    {
        public static void Main(string[] args)
        {

            Init init = new Init();
            
            init.Start();
        }

        
       
    }

    internal class Init
    {
        public void Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            
            if (!new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) 
                                   + "/mrpack_extractor/" ).Exists)
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                          + "/mrpack_extractor/");
            }
            if (!new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) 
                                   + "/mrpack_extractor/" + "downloaded" ).Exists)
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                          + "/mrpack_extractor/" + "downloaded");
            }
            
            if (args.Length == 0)
            {
                Console.WriteLine("Drop a file on the exe!");
                Console.WriteLine("Press any button to close...");
                Console.ReadLine();
                Environment.Exit(1);
                
            }
            else
            {
                //Extract(args[1]);
                Extract(
                    "C:\\Users\\thega\\RiderProjects\\mrpack extractor\\bin\\Debug\\net8.0\\Server moddato 1.2.2.mrpack");
                string[] links = Parser();
                Download(links);
            }
            
            
        }
        public bool Extract(string path)
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    ZipArchive archive = new ZipArchive(stream);
                    
                    archive.ExtractToDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                               + "/mrpack_extractor",true);
                    return true;
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                throw;
            }
            
        }

        public string[] Parser()
        {
            TextReader txt_reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                                 + "/mrpack_extractor/" + "modrinth.index.json");
            string json = txt_reader.ReadToEnd();
            
            var jobject = JObject.Parse(json);

            string[] links = jobject.SelectTokens("$.files[*].downloads[0]").Values<string>().ToArray();

            return links;

        }

        public void Download(string[] links)
        {
            using (var client = new WebClient())
            {
                foreach (var link in links)
                {
                    Uri uri = new Uri(link);
                    
                    string filename = link;
                    int last_slash = filename.LastIndexOf("/");
                    string sanitized = filename[last_slash..filename.Length];
                    
                    client.DownloadFile(uri, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) 
                                             + "/mrpack_extractor/" + "downloaded" + sanitized);
                }
            }
        }
        
        
        
    }
}

