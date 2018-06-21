using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Server
{

    struct User
    {
        int id;

    }
    class server
    {
        private HttpListener listener;
        List<User> mas_connect;

        public server(){}

        public void NewUser(object obj)
        {
            HttpListenerContext context = (HttpListenerContext)obj;
            HttpListenerRequest request;
            HttpListenerResponse response;
            Stream output;
            byte[] buffer;
            string inputRead = null;

            request = context.Request;
            // получаем объект ответа 
            response = context.Response;
            using (StreamReader input = new StreamReader(
            request.InputStream, Encoding.UTF8))
            {
                inputRead = input.ReadToEnd();
                switch (inputRead)
                {
                    case "setID":
                        Random rand = new Random();
                        int id_tmp = rand.Next(100, 10000);

                        buffer = Encoding.UTF8.GetBytes(id_tmp.ToString());
                        response.ContentLength64 = buffer.Length;
                        output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);

                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("ID " + id_tmp + " подключен");
                        Console.WriteLine();
                        Console.WriteLine();
                        return;
                    default:
                        Console.WriteLine(inputRead);
                        break;
                }
            }
           
            // создаем ответ клиенту 
            buffer = Encoding.UTF8.GetBytes("Сообщение отправлено");
            // получаем поток ответа и пишем в него ответ 
            response.ContentLength64 = buffer.Length;
            output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

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
    }
}
