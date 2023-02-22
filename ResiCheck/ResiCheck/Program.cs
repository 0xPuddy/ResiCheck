using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResiCheck
{
    internal class Program
    {
        public static int checkedcount = 0;
        public static int timeout = 5000;
        public static int proxiesCount = 0;
        //List<Thread> threads = new List<Thread>();

        static void Main(string[] args)
        {
            Console.WindowWidth = 100;
            string lol = @"
                        
                           __           _   ___ _               _    
                          /__\ ___  ___(_) / __\ |__   ___  ___| | __
                         / \/// _ \/ __| |/ /  | '_ \ / _ \/ __| |/ /
                        / _  \ P__/\__U\ / /___|D| |D|  __/ (__|Y  < 
                        \/ \_/\___||___/_\____/|_| |_|\___|\___|_|\_\
        
                                   V0.1 | Cyber-request.com

";
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(lol);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Path to proxylist (drag into console): ");
            string path = Console.ReadLine();
            Console.Write("Enter the number of threads to use: ");
            int numThreads = int.Parse(Console.ReadLine());
            Console.Write("Set (timeout default 5000): ");
            timeout = int.Parse(Console.ReadLine());
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(lol);
            Console.ForegroundColor = ConsoleColor.Gray;
            // Read the list of proxies from the file
            List<string> proxies = new List<string>();
            proxiesCount = proxies.Count();
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    proxies.Add(line);
                }
            }
            Console.Title = "ResiCheck v0.1 | "+checkedcount+" checked | Created by Puddy | cyber-request.com";
            // Create the output file name based on the current date and time
            string outputFileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-proxies.txt";

            // Create a StreamWriter to write the output to the file
            StreamWriter writer = new StreamWriter(outputFileName);

            // Create a list to hold the threads
            int chunkSize = proxies.Count / numThreads;

            List<Thread> threads = new List<Thread>();
            List<ManualResetEvent> resetEvents = new List<ManualResetEvent>();

            for (int i = 0; i < numThreads; i++)
            {
                int startIndex = i * chunkSize;
                int endIndex = (i == numThreads - 1) ? proxies.Count : (i + 1) * chunkSize;

                List<string> proxyChunk = proxies.GetRange(startIndex, endIndex - startIndex);

                ManualResetEvent resetEvent = new ManualResetEvent(false);
                resetEvents.Add(resetEvent);

                Thread thread = new Thread(() =>
                {
                    try
                    {
                        IsResidentialProxy(proxyChunk, writer);
                    }
                    finally
                    {
                        resetEvent.Set();
                    }
                });

                thread.Start();
                threads.Add(thread);
            }

            WaitHandle.WaitAll(resetEvents.ToArray());

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            writer.Close();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                  Proxy check complete. Results saved to " + outputFileName);
            Console.ReadLine();
        }

        static void IsResidentialProxy(List<string> proxies, StreamWriter writer)
        {
            foreach (string proxy in proxies)
            {
                try
                {
                    // Create a new WebRequest object with the proxy address
                    WebRequest request = WebRequest.Create("https://www.google.com/");
                    request.Proxy = new WebProxy(proxy);

                    // Set the timeout to 10 seconds
                    request.Timeout = timeout;

                    // Send a request to Google and check if the response contains the string "google"
                    using (WebResponse response = request.GetResponse())
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            byte[] buffer = new byte[8192];
                            StringBuilder sb = new StringBuilder();
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                sb.Append(Encoding.UTF8.GetString(buffer, 0, read));
                                if (sb.ToString().Contains("google"))
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("                             ╠ ✔ Working residental : " + proxy);
                                    writer.WriteLine(proxy);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("                          ╠ ✣ Not working : " + proxy);
                    // An exception occurred, which means the proxy is either not working or not residential
                }
                checkedcount++;
                Console.Title = "ResiCheck v0.1 | " + checkedcount + " checked | Created by Puddy | cyber-request.com";
            }

        }
    }
}
