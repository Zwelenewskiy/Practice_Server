using System;
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
        public System.Drawing.Imaging.ImageFormat Format;
        public byte[] Image;

        public Report(int com, string id, string p, string p1, string p2,
            System.Drawing.Imaging.ImageFormat format, string name, byte[] img)
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

    class Server
    {
        private HttpListener listener;
        List<int> mas_connect;

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

                int key = UserReport.Command;

                var connect = Connection("pavel6520.hopto.org", 25565, "project", "root", "6520");
                connect.Open();
                if (connect.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Ошибка подключения к базе данных");
                    return;
                }

                switch (key)
                {
                    case 0:
                        Random rand = new Random();
                        int id_tmp = rand.Next(100, 10000);

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
                            "now() )");

                        Console.WriteLine();
                        SendMessage(response, "Отчет отправлен");
                        break;

                    case 3:
                        try
                        {
                            using (var reader = Query(connect, "select P, P1, P2 from project.test order by Date desc limit 1"))
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
                        Console.Write(curDate + " " + UserReport.Id + " отключился");
                        Console.WriteLine();
                        Console.WriteLine();
                        break;

                    case 5:
                        SendMessage(response, JsonConvert.SerializeObject(new Report(0, UserReport.Id, "", "", "", null, "", null)));
                        Console.WriteLine(curDate + " Проверка соединения от " + UserReport.Id);
                        break;

                    case 6:
                        Console.WriteLine();
                        Console.WriteLine("Format: " + UserReport.Format.ToString());
                        Console.WriteLine(curDate + " Принято изображение от " + UserReport.Id);

                        File.WriteAllBytes(@"D:\Alexander\Programming\C#\Клиент-сервер\Server\Images\t.png", UserReport.Image);

                        SendMessage(response, "Изображение отправлено");
                        Console.WriteLine();
                        break;
                }

                connect.Close();
                connect.Dispose();
            }
        }
        public void Start(string host)
        {
            listener = new HttpListener();
            // установка адресов прослушки 
            listener.Prefixes.Add(host);
            listener.Start();

            Console.WriteLine("Сервер запущен. Ожидание подключений...");
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
            /*String connString = "server=" + host + ";database=" + database
               + ";port=" + port + ";user=" + username + ";password=" + password;
            MySqlConnection conn = new MySqlConnection(connString);
            return conn;*/

            return new MySqlConnection("server=" + host + ";database=" + database
               + ";port=" + port + ";user=" + username + ";password=" + password); ;

        }
        public MySqlDataReader Query(MySqlConnection connect, string query)
        {
            return new MySqlCommand(query, connect).ExecuteReader();
        }
    }
}
