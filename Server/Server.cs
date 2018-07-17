using System;
using System.Drawing.Imaging;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json; 

namespace Server
{
    struct Report
    {
        public int Command;
        public string Id, P, P1, P2, ImgName;
        public ImageFormat Format;
        public byte[] Image;

        public Report(int com, string id, string p, string p1, string p2, ImageFormat format, string name, byte[] img)
        {
            Command = com;
            Id = id;
            P = p;
            P1 = p1;
            P2 = p2;
            ImgName = name;
            Image = img;
            Format = format;
        }
    }

    struct Image
    {
        public string name;
        public long size;
        public byte[] image;

        public Image(string n, long s, byte[] img)
        {
            name = n;
            size = s;
            image = img;
        }
    }    

    struct Images
    {
        public List<Image> images;
    }

    class Server
    {
        const string ImagesDirectory = "D:\\\\Alexander\\\\Programming\\\\C#\\\\Клиент-сервер\\\\Server\\\\Images\\\\";
        const string ForUpload = "D:\\\\Alexander\\\\Programming\\\\C#\\\\Клиент-сервер\\\\Server\\\\Images\\\\ForUpload";
        

        private HttpListener listener;
        List<int> UserIdArray = new List<int>();//Id активных пользователей
        Images ImagesArray;

        public Server() { }
        public void NewUser(object obj)
        {
            HttpListenerContext context = (HttpListenerContext)obj;
            HttpListenerRequest request;
            HttpListenerResponse response;
            DateTime curDate = DateTime.Now;

            request = context.Request;
            // получаем объект ответа 
            response = context.Response;
            using (StreamReader input = new StreamReader(
            request.InputStream, Encoding.UTF8))
            {
                Report UserReport = JsonConvert.DeserializeObject<Report>(input.ReadToEnd());

                var connect = Connection("pavel6520.hopto.org", 25565, "project", "root", "6520");
                connect.Open();
                if (connect.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Ошибка подключения к базе данных");
                    return;
                }

                switch (UserReport.Command)
                {
                    case 0:
                        Random rand = new Random();
                        
                        int id_tmp;
                        do
                        {
                            id_tmp = rand.Next(100, 10000);
                        } while (UserIdArray.IndexOf(id_tmp) > 0);

                        UserIdArray.Add(id_tmp);

                        SendMessage(response, id_tmp.ToString());
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine(curDate + " ID " + id_tmp + " подключен");
                        Console.WriteLine();
                        Console.WriteLine();
                        break;

                    case 1:
                        Console.WriteLine();
                        Console.WriteLine(curDate + " " + UserReport.Id + " > " + UserReport.P);
                        SendMessage(response, "Сообщение отправлено");
                        Console.WriteLine();
                        break;

                    case 2:
                        Console.WriteLine();
                        Console.WriteLine(curDate + " Отчет от Id: " + UserReport.Id);
                        Console.WriteLine("1: " + UserReport.P);
                        Console.WriteLine("2: " + UserReport.P1);
                        Console.WriteLine("3: " + UserReport.P2);

                        Query(connect, "insert into project.test values( '" +
                            UserReport.Id + "', '" +
                            UserReport.P + "', '" +
                            UserReport.P1 + "', '" +
                            UserReport.P2 + "'," +
                            "now() )", false);

                        Console.WriteLine();
                        SendMessage(response, "Отчет отправлен");
                        break;

                    case 3:
                        try
                        {
                            using (var reader = Query(connect, "select P, P1, P2 from project.test order by Date desc limit 1", true))
                            {
                                while (reader.Read())
                                    SendMessage(response, JsonConvert.SerializeObject(new Report(0, "", reader[0].ToString(),
                                        reader[1].ToString(), reader[2].ToString(), null, "", null)));
                            }                 
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                            throw;
                        }
                        break;

                    case 4:
                        Console.WriteLine();

                        Query(connect, "delete from project.test1 where Id = '" + UserReport.Id + "' ", false);

                        string[] files = Directory.GetFiles(ImagesDirectory);
                        for (int i = 0; i < files.Length; i++)
                            if (files[i].IndexOf(UserReport.Id) > 0)
                                File.Delete(files[i]);

                        UserIdArray.Remove(Convert.ToInt32(UserReport.Id));

                        Console.WriteLine(curDate + " " + UserReport.Id + " отключился");
                        Console.WriteLine();
                        Console.WriteLine();
                        break;

                    case 5:
                        SendMessage(response, JsonConvert.SerializeObject(new Report(0, UserReport.Id, "", "", "", null, "", null)));
                        Console.WriteLine(curDate + " Проверка соединения от " + UserReport.Id);
                        break;

                    case 6:
                        Console.WriteLine();
                        Console.WriteLine(curDate + " Принято изображение от " + UserReport.Id);

                        string path = ImagesDirectory + UserReport.Id + '.' + UserReport.Format;
                        File.WriteAllBytes(path, UserReport.Image);

                         Query(connect, "insert into project.test1 values( '" +
                             UserReport.Id + "', '" +
                             path + "', " +
                             "now() )", false);
                        
                        SendMessage(response, "Изображение отправлено");
                        Console.WriteLine();
                        break;

                    case 7:
                        Console.WriteLine();
                        SendMessage(response, JsonConvert.SerializeObject(ImagesArray));
                        Console.WriteLine(curDate + " Изображения отправлены к " + UserReport.Id);
                        Console.WriteLine();

                        break;
                }

                connect.Close();
                connect.Dispose();
            }
        }
        public void Start(string host)
        {
            DateTime curDate = DateTime.Now;

            listener = new HttpListener();
            // установка адресов прослушки 
            listener.Prefixes.Add(host);
            listener.Start();

            ImagesArray.images = new List<Image>();
            string[] imgs = Directory.GetFiles(ForUpload);

            if(imgs.Length > 0)
            {
                for (int i = 0; i < imgs.Length; i++)
                {
                    ImagesArray.images.Add(new Image(imgs[i].Substring(imgs[i].LastIndexOf(@"\") + 1),
                        new FileInfo(imgs[i]).Length / 1024, File.ReadAllBytes(imgs[i])));
                }
            }          

            Console.WriteLine(curDate +  " Сервер запущен. Ожидание подключений...");
            Console.WriteLine();
        }
        public void Stop()
        {
            // останавливаем прослушивание подключений 
            listener.Stop();
        }
        public void NewConnection()
        {
            HttpListenerContext context;
            // метод GetContext блокирует текущий поток, ожидая получение запроса 
            context = listener.GetContext();

            Thread new_user = new Thread(new ParameterizedThreadStart(NewUser));
            new_user.Start(context);
        }
        public void SendMessage(HttpListenerResponse response, string message)
        {
            response.ContentLength64 = Encoding.UTF8.GetBytes(message).Length;
            response.OutputStream.Write(Encoding.UTF8.GetBytes(message), 0, Encoding.UTF8.GetBytes(message).Length);
        }
        public MySqlConnection Connection(string host, int port, string database, string username, string password)
        {
            return new MySqlConnection("server=" + host + ";database=" + database
               + ";port=" + port + ";user=" + username + ";password=" + password); ;

        }
        public MySqlDataReader Query(MySqlConnection connect, string query, bool need)
        {
            if (need)
                return new MySqlCommand(query, connect).ExecuteReader();
            else
            {
                new MySqlCommand(query, connect).ExecuteReader();
                return null;
            }                
        }        
    }
}
