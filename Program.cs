using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace GenshinMangaDownloader
{
    class Program
    {
        static void Main()
        {
            HttpClient httpClient = new HttpClient();
            string mainSite = httpClient.GetStringAsync("https://www.yuanshen.com/manga").Result;
            mainSite = mainSite.Substring(mainSite.LastIndexOf("风之歌"));
            int start = 0;
            int end = 0;
            List<string> chapterNameList = new List<string>();
            List<string> chapterIDList = new List<string>();
            List<string> coverList = new List<string>();
            chapterNameList.Add("00序章 风之歌");
            chapterIDList.Add("184");
            coverList.Add("https://uploadstatic.mihoyo.com/contentweb/20190211/2019021117254254750.jpg");
            chapterNameList.Add("01第一话 添酒");
            chapterIDList.Add("191");
            coverList.Add("https://uploadstatic.mihoyo.com/contentweb/20190222/2019022216011312601.jpg");
            while (start != -1 && end != -1)
            {
                start = mainSite.IndexOf("第", end);
                if (start != -1)
                {
                    end = mainSite.IndexOf("\"", start);
                    int idLocation = mainSite.LastIndexOf("jpg", start) + 6;
                    if (end != -1)
                    {
                        chapterNameList.Add(chapterNameList.Count.ToString("D2") + mainSite[start..end]);
                        chapterIDList.Add(mainSite.Substring(idLocation, start - idLocation - 3));
                        int coverStart = mainSite.IndexOf("http", start);
                        int coverEnd = mainSite.IndexOf("jpg", coverStart) + 3;
                        coverList.Add(mainSite[coverStart..coverEnd].Replace("\\u002F", "/"));
                    }
                    start = end;
                }
            }
            List<Task> taskList = new List<Task>();
            foreach (string chapterName in chapterNameList)
            {
                int num = chapterNameList.IndexOf(chapterName);
                string chapterID = chapterIDList[num];
                string cover = coverList[num];
                taskList.Add(Task.Run(() => DownloadChapter(num, chapterName, chapterID, cover)));
            }
            Task.WaitAll(taskList.ToArray());
            Console.SetCursorPosition(0, chapterNameList.Count + 1);
            Console.WriteLine("完成");
            Console.ReadKey();
        }
        static void DownloadChapter(int dirNum, string chapterName, string chapterID, string cover)
        {
            if (!Directory.Exists(chapterName))
            {
                Directory.CreateDirectory(chapterName);
            }
            HttpClient httpClient = new HttpClient();
            string chapterSite = httpClient.GetStringAsync("https://www.yuanshen.com/manga/detail/" + chapterID).Result;
            int siteStart = chapterSite.IndexOf("data-server-rendered");
            chapterSite = chapterSite[siteStart..chapterSite.LastIndexOf("bottombar__chapter")];
            int start = 0;
            int end = 0;
            int fileNum = 0;
            List<string> fileUrl = new List<string>();
            List<string> filePath = new List<string>();
            fileUrl.Add(cover);
            filePath.Add(chapterName + "\\" + chapterName.Substring(0, 2) + "-" + "00.jpg");
            while (start != -1 && end != -1)
            {
                start = chapterSite.IndexOf("https://uploadstatic.mihoyo.com", end);
                if (start != -1)
                {
                    end = chapterSite.IndexOf("jpg", start);
                    if (end != -1)
                    {
                        fileUrl.Add(chapterSite.Substring(start, end - start + 3));
                        fileNum++;
                        filePath.Add(chapterName + "\\" + chapterName.Substring(0, 2) + "-" + fileNum.ToString("D2") + ".jpg");
                    }
                    start = end;
                }
            }
            foreach (string fileName in filePath)
            {
                int num = filePath.IndexOf(fileName);
                string address = fileUrl[num];
                if (!File.Exists(fileName))
                {
                    FileStream fs = File.Create(fileName);
                    Byte[] fileByte = httpClient.GetByteArrayAsync(address).Result;
                    fs.Write(fileByte, 0, fileByte.Length);
                    fs.Close();
                }
                Consoler.Write(dirNum, $"{chapterName}：{num + 1}/{fileUrl.Count}");
            }
        }
        public class Consoler
        {
            private static readonly object _lock = new object();
            public static void Write(int n, string s)
            {
                lock (_lock)
                {
                    Console.SetCursorPosition(0, n);
                    Console.Write(s);
                }
            }
        }
    }
}
