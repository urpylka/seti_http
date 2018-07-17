using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace seti_http
{
    class Program
    {
        private static void GETstringHead(string url, string bUri, ref Types[] types)
        {
            /*
                Достает строку ответа на get-запрос
            */
            try
            {
                System.Net.WebRequest reqGET = System.Net.WebRequest.Create(url);
                reqGET.Method = "HEAD";
                reqGET.Timeout = 500;
                System.Net.WebResponse resp = reqGET.GetResponse();
                bool repeat = false;
                foreach (Types type in types)
                    if (type.type == resp.ContentType)
                    {
                        //если такой тип уже есть, то добавляем в суммарный размер
                        repeat = true;
                        type.size += resp.ContentLength;
                        //Console.WriteLine("Add type: " + resp.ContentType);
                    }
                if (!repeat)
                {
                    //если тип не найден, то создаем новый элемент
                    Array.Resize<Types>(ref types, types.Length + 1);
                    types[types.Length - 1] = new Types(resp.ContentType, resp.ContentLength);
                    //Console.WriteLine("Add type: " + resp.ContentType);
                }
                resp.Close();
                //foreach (Types linkFrArr in types) Console.WriteLine(linkFrArr.type + linkFrArr.size);
            }
            catch (Exception e)
            {
                Console.WriteLine("В загрузке {1} ответа сервера произошло исключение: {0}", e.Message.ToString(), url);
                try
                {

                    //Pass the filepath and filename to the StreamWriter Constructor
                    StreamWriter sw = new StreamWriter("log.txt", true);

                    //Write a line of text
                    sw.WriteLine(bUri + " " + url);

                    //Close the file
                    sw.Close();
                }
                catch (Exception e2)
                {
                    Console.WriteLine("Exception: " + e2.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
        }
        private static string GETstring(string url)
        {
            /*
                Достает строку ответа на get-запрос
            */
            try
            {
                System.Net.WebRequest reqGET = System.Net.WebRequest.Create(url);
                reqGET.Timeout = 500;
                System.Net.WebResponse resp = reqGET.GetResponse();
                System.IO.Stream stream = resp.GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                return sr.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine("В загрузке {1} ответа сервера произошло исключение: {0}", e.Message.ToString(), url);
                return "В загрузке ответа сервера произошло исключение: " + e.Message.ToString(); //ответ сервера, на этапе парсинга отбросится
            }
        }
        private static void DumpHRefs(string inputString, string bUri, string bUriForSave, ref links[] arrLinks)
        {
            Match m;
            string HRefPattern = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";
            try
            {
                m = Regex.Match(inputString, HRefPattern,
                                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                TimeSpan.FromSeconds(1));
                while (m.Success)
                {
                    //Console.WriteLine("Found href " + m.Groups[1] + " at " + m.Groups[1].Index);
                    var baseUri = new Uri(bUri, UriKind.Absolute);
                    try
                    {
                        var absUri = new Uri(baseUri, m.Groups[1].ToString());
                        //saveLinkToArray(absUri.ToString(), bUri, ref arrLinks);
                        if (absUri.ToString().IndexOf('#') < 0)
                            if (absUri.ToString().IndexOf('?') < 0)
                                saveLinkToArray(absUri.ToString(), bUri, bUriForSave, ref arrLinks);
                            else
                                //дополнительно убираем все что начинается '?' в конце строки
                                saveLinkToArray(absUri.ToString().Remove(absUri.ToString().IndexOf('?')), bUri, bUriForSave, ref arrLinks);
                        else
                            //дополнительно убираем все что начинается '#' в конце строки
                            saveLinkToArray(absUri.ToString().Remove(absUri.ToString().IndexOf('#')), bUri, bUriForSave, ref arrLinks);
                        //Console.WriteLine(absUri);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("В преобразовании адреса {1} сработало исключение: {0}", e.Message.ToString(), m.Groups[1].ToString());
                    }
                    m = m.NextMatch();
                }
            }
            catch (RegexMatchTimeoutException)
            {
                Console.WriteLine("The matching operation timed out.");
            }
        }
        private static void saveLinkToArray(string link, string baseUri, string bUriForSave, ref links[] arrLinks)
        {
            /*
               проверяет есть ли нормализованная (абсолютная) ссылка в массиве, и если нет, то записывает ее в массив по адресу
            */
            if (link.StartsWith(baseUri)) //если ссылки находятся на корневом домене
            {
                bool repeat = false;
                foreach (links linkFrArr in arrLinks) if (linkFrArr.link == link) repeat = true;
                if (!repeat)
                {
                    Array.Resize<links>(ref arrLinks, arrLinks.Length + 1);
                    arrLinks[arrLinks.Length - 1] = new links(link, bUriForSave, false);
                    //Console.WriteLine("Add link: " + link);
                }
            }
        }
        private static void saveTypeLinkToArray(string link, ref Types[] arrTypeLinks)
        {
            /*
               проверяет есть ли расширение файла в массиве, если нет, то записывает в массив и добавляет размер
            */
            string type = link; //исправить
            bool repeat = false;
            foreach (Types typeFrArr in arrTypeLinks) if (typeFrArr.type == type) repeat = true;
            if (!repeat)
            {
                Array.Resize<Types>(ref arrTypeLinks, arrTypeLinks.Length + 1);
                //arrTypeLinks[arrTypeLinks.Length - 1] = new Types(type, getSize(link));
            }
        }
        private static void machiene(string baseUri, ref links[] arrLinks, ref Types[] types)
        {
            for(int i = 0;i<arrLinks.Length;i++)
            {
                links linkFrArr = arrLinks[i];
                if (!linkFrArr.visited)
                {
                    linkFrArr.visited = true;
                    string pattern = @"(.php|.html|.htm|/(|[a-zA-Z0-9\-_]*))(\?[a-zA-Z0-9\-_\=\&]*|\#[a-zA-Z0-9\-_]*|)$";
                    Regex newReg = new Regex(pattern);
                    MatchCollection matches = newReg.Matches(linkFrArr.link);
                    if(matches.Count>0)
                    {
                        DumpHRefs(GETstring(linkFrArr.link), baseUri, linkFrArr.link, ref arrLinks);
                    }
                    else
                    {
                        //добавляем размер в Types
                        //GETstringHead(linkFrArr.link, ref types);
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            string baseUri = "http://war.ssau.ru/";
            links[] arrLinks = new links[1];
            arrLinks[0] = new links(baseUri, baseUri, false);
            Types[] types = new Types[1];
            types[0] = new Types("text/css", 0);
            machiene(baseUri, ref arrLinks, ref types); //основной цикл отвечающий за рекурсивный перебор
            foreach (links linkFrArr in arrLinks) GETstringHead(linkFrArr.link, linkFrArr.bUri, ref types);
            Console.WriteLine("=====================================");
            foreach (Types linkFrArr in types) Console.WriteLine(linkFrArr.type +" - "+ linkFrArr.size);
            //foreach (links linkFrArr in arrLinks) Console.WriteLine(linkFrArr.link);
            Console.WriteLine("=====================================");
            Console.WriteLine("Total links: " + arrLinks.Length);
            Console.ReadLine();
        }
    }
}
