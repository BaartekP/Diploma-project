using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;           //FtpWebRequest
using System.IO;            //InputOutput    
using System.Diagnostics;   //Stopwatch

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string ftpAddress,port,hostname,password;

            Console.Write("ftpAddress : ");
            ftpAddress = Console.ReadLine();

            Console.Write("port : ");
            port = Console.ReadLine();

            Console.Write("hostname : ");
            hostname = Console.ReadLine();

            Console.Write("password : ");
            password = Console.ReadLine();

            Console.WriteLine("");

            try
            {   //Connection to the server
                FtpWebRequest ftpWebRequest_1 = (FtpWebRequest)WebRequest.Create($"ftp://{ftpAddress}:{port}");
                ftpWebRequest_1.Credentials = new NetworkCredential($"{hostname}", $"{password}");
                
                //Getting all file names
                string names = "";
                ftpWebRequest_1.Method = WebRequestMethods.Ftp.ListDirectory;
                using (FtpWebResponse response = (FtpWebResponse)ftpWebRequest_1.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            names = reader.ReadToEnd();
                        }
                    }
                }

                List<string> nameList = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                
                List<int> sizeList = new List<int> { };

                foreach (string name in nameList)
                {   //Connection to the specific file on the server
                    FtpWebRequest ftpWebRequest_2 = (FtpWebRequest)WebRequest.Create($"ftp://{ftpAddress}:{port}/{name}");
                    ftpWebRequest_2.Credentials = new NetworkCredential($"{hostname}", $"{password}");

                    ftpWebRequest_2.Method = WebRequestMethods.Ftp.GetFileSize;
                    using (FtpWebResponse response = (FtpWebResponse)ftpWebRequest_2.GetResponse())
                    {
                        sizeList.Add((int)response.ContentLength);
                    }
                }

                //If sizes of lists are equal, program prints the informations about the files
                if (nameList.Count == sizeList.Count)
                {
                    Console.WriteLine("Nr\tName\tSize[B]");
                    Console.WriteLine("-----------------------");
                    for (int i = 0; i < nameList.Count; i++)
                    {
                        Console.WriteLine($"[{i}]\t{nameList[i]}\t{sizeList[i]}");
                    }
                }

                Console.WriteLine("\nPress Key to start downloading!");
                Console.ReadLine();

                //Object which calculate the time
                Stopwatch stopwatch = new Stopwatch();

                foreach (string name in nameList)
                {   //Connection to the specific file on the server
                    FtpWebRequest ftpWebRequest_3 = (FtpWebRequest)WebRequest.Create($"ftp://{ftpAddress}:{port}/{name}");
                    ftpWebRequest_3.Credentials = new NetworkCredential($"{hostname}", $"{password}");

                    ftpWebRequest_3.Method = WebRequestMethods.Ftp.DownloadFile;
                    using (FtpWebResponse response = (FtpWebResponse)ftpWebRequest_3.GetResponse())
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (Stream fileStream = File.Create($@"C://Users/Bartek/Desktop/Appoutput/{name}"))
                            {
                                byte[] buffer = new byte[10240];
                                int read;

                                Console.WriteLine($"Downloading file {name} started!");
                                stopwatch.Reset();
                                stopwatch.Start();
                                while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fileStream.Write(buffer, 0, read);
                                }
                                stopwatch.Stop();
                                Console.WriteLine($"Size {fileStream.Position} bytes \nTime {stopwatch.ElapsedMilliseconds} ms");
                                double bps = (double)fileStream.Position / ((double)stopwatch.ElapsedMilliseconds / 1000);
                                Console.WriteLine($"Speed {Math.Round((bps / 1000 / 1000), 2)} Mbps\n");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
            Console.ReadLine();
        }


    }
}
