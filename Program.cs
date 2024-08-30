using System.IO.Compression;
using Newtonsoft.Json.Linq;

namespace Program
{
    class MainInit
    {
        static async Task Main(string[] args)
        {
            Init init = new Init();

            await init.Start();
        }
    }

    internal class Init
    {
        public async Task Start()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (!new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                   + @"\mrpack_extractor\").Exists)
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                          + @"\mrpack_extractor\");
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
                
                string top_folder = Extract(args[1]);
                //string top_folder = Extract(@"C:\\Users\\thega\\RiderProjects\\mrpack extractor\\bin\\Debug\\net8.0\\Server moddato 1.2.2.mrpack");
                (string[] links, string[] paths) = Parser(top_folder);
                await Download(links, paths, top_folder);
                Console.WriteLine("\n All done! Press any key to exit...");
                Console.ReadLine();
            }


        }

        public string Extract(string path)
        {
            try
            {
                Console.WriteLine("What will be the name of the folder the assets will be stored to?:");
                string name = Console.ReadLine();
                string extraction = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                    + @"\mrpack_extractor" + @"\" + name;
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    ZipArchive archive = new ZipArchive(stream);
                    
                    archive.ExtractToDirectory(extraction, true);
                    Console.WriteLine("you will find your pack extracted at " + extraction);
                    Console.WriteLine("Remember to check the overrides folder!!!!");
                    Thread.Sleep(5000);
                    return name;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                throw;
            }

        }

        public (string[], string[]) Parser(string top_folder)
        {
            TextReader txt_reader = new StreamReader(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                + @"\mrpack_extractor\" + top_folder + @"\modrinth.index.json");
            string json = txt_reader.ReadToEnd();

            var jobject = JObject.Parse(json);

            string[] links = jobject.SelectTokens("$.files[*].downloads[0]").Values<string>().ToArray();
            string[] paths = jobject.SelectTokens("$.files[*].path").Values<string>().ToArray();

            return (links, paths);

        }


        public async Task Download(string[] links, string[] paths, string top_folder)
        {
            int i = 0;
            try
            {
                using (var client = new HttpClient())
                {
                    foreach (var link in links)
                    {
                        //so we dont flood the modrinth cdn
                        Thread.Sleep(500);
                        
                        Console.WriteLine("\n Download: " + link);
                        Uri uri = new Uri(link);
                        
                        string path_to_folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                      + @"\mrpack_extractor\"+ top_folder + @"\" + paths[i][0..paths[i].LastIndexOf("/")];
                        
                        string path_to_file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                              + @"\mrpack_extractor\"+ top_folder + @"\" + paths[i];
                        
                        if (!Directory.Exists(path_to_folder))
                        {
                            Directory.CreateDirectory(path_to_folder);
                        }
                    
                        var response = await client.GetAsync(uri);
                        if (response.IsSuccessStatusCode)
                        {   
                            var totalBytes = response.Content.Headers.ContentLength.Value;
                            
                            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                   stream = new FileStream(path_to_file, FileMode.Create))
                            {
                                var buffer = new byte[8192];
                                long totalRead = 0;
                                int bytesRead;
                                do
                                {
                                    bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                                    if (bytesRead > 0)
                                    {
                                        await stream.WriteAsync(buffer, 0, bytesRead);
                                        totalRead += bytesRead;

                                        if (totalBytes != -1)
                                        {
                                            Console.Write(
                                                $"\rDownload progress: {totalRead}/{totalBytes} bytes ({(totalRead * 100 / totalBytes):0.00}%)");
                                        }
                                        else
                                        {
                                            Console.Write($"\rDownload progress: {totalRead} bytes");
                                        }
                                    }
                                } while (bytesRead > 0);

                            }
                        }
                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                throw;
            }

        }
    }
}

