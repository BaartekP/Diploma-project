using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;           //FtpWebRequest
using System.IO;            //InputOutput    
using System.Diagnostics;   //Stopwatch
using System.Threading;

namespace ConsoleApp1
{
    public class Program
    {
        public static string ftpAddress = null;
        public static string port = null;
        public static string hostname = null;
        public static string password = null;
        public static double time = 0;
        //public static List<string> times = new List<string>();
        public static Dictionary<string, DateTime> timeDict = new Dictionary<string,DateTime>();
        //public static Dictionary<string, double> correctTimes = new Dictionary<string, double>();
        public static Dictionary<int, Dictionary<string, DateTime>> all = new Dictionary<int, Dictionary<string, DateTime>>();

        static void Main(string[] args)
        {
            List<string> nameList = new List<string> { };
            List<int> sizeList = new List<int> { };
            int nThreads = 1;

            SetFtpServerInfo();
            PrintFilesInfo(ref nameList, ref sizeList);

            Console.Write("\nSet the number of threads : ");
            nThreads = Convert.ToInt32(Console.ReadLine());
            //for (int j = 1; j < 9; j++)
            //{
            //Console.WriteLine($"\nThreads number : {j} ");

                ThreadPool.SetMinThreads(nThreads, nThreads);
                ThreadPool.SetMaxThreads(nThreads, nThreads);

                timeDict.Clear();

                timeDict.Add("Initial", DateTime.Now);
                foreach (string name in nameList)
                    ThreadPool.QueueUserWorkItem(DownloadFile, name);


                Console.ReadLine();
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter($@"E:/Appoutput/{j}.txt"))
                //{
                    
                for (int i = 1; i < timeDict.Count; i++)
                {
                    double temp = timeDict.ElementAt(i).Value.TimeOfDay.TotalSeconds - timeDict.ElementAt(i - 1).Value.TimeOfDay.TotalSeconds;
                    Console.WriteLine($"{timeDict.ElementAt(i).Key} {temp}");
                        //file.WriteLine($"{timeDict.ElementAt(i).Key} {temp}");
                }

                double time = timeDict.ElementAt(timeDict.Count - 1).Value.TimeOfDay.TotalSeconds - timeDict.ElementAt(0).Value.TimeOfDay.TotalSeconds;

                Console.WriteLine($"Time needed to download data : {time} s");

            //      file.WriteLine($"Time needed to download data : {time} s\n");
            // }

            //}

            Console.ReadLine();
        }

        public static void SetFtpServerInfo() {

            //Console.Write("ftpAddress : ");
            ftpAddress = "127.0.0.1";//Console.ReadLine();

            //Console.Write("port : ");
            port = "1209";//Console.ReadLine();

            //Console.Write("hostname : ");
            hostname = "bprasels";//Console.ReadLine();

            //Console.Write("password : ");
            password = "qwerty";//Console.ReadLine();

            //Console.WriteLine("");

        }

        public static void PrintFilesInfo(ref List<string> nameList, ref List<int> sizeList) {

            try
            {
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

                nameList = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

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

            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }

        }

        public static void DownloadFile(object objName)
        {
            
            string name = (string)objName;
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                FtpWebRequest ftpWebRequest_3 = (FtpWebRequest)WebRequest.Create($"ftp://{ftpAddress}:{port}/{name}");
                ftpWebRequest_3.Credentials = new NetworkCredential($"{hostname}", $"{password}");

                ftpWebRequest_3.Method = WebRequestMethods.Ftp.DownloadFile;
                using (FtpWebResponse response = (FtpWebResponse)ftpWebRequest_3.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (Stream fileStream = File.Create($@"E:/Appoutput/{name}"))
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
                            timeDict.Add(name, DateTime.Now);
                            Console.WriteLine($"Done {name}");
                            //Console.WriteLine($"File {name} \nSize {fileStream.Position} bytes \nTime {stopwatch.ElapsedMilliseconds} ms");
                            //double bps = (double)fileStream.Position / ((double)stopwatch.ElapsedMilliseconds / 1000);
                            //Console.WriteLine($"Speed {Math.Round((bps / 1000 / 1000), 2)} Mbps");
                            //time += (double)stopwatch.ElapsedMilliseconds;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
            //Console.WriteLine($"Using thread nr {Thread.CurrentThread.ManagedThreadId}\n");

        }

    }
}
