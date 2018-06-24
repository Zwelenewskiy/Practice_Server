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
        //приватный ключ
        //публичный ключ
    }
    class Server
    {
        private HttpListener listener;
        List<User> mas_connect;

        public Server() { }
        public void NewUser(object obj)
        {
            HttpListenerContext context = (HttpListenerContext)obj;
            HttpListenerRequest request;
            HttpListenerResponse response;
            string inputRead = null;

            request = context.Request;
            // получаем объект ответа 
            response = context.Response;
            using (StreamReader input = new StreamReader(
            request.InputStream, Encoding.UTF8))
            {
                inputRead = input.ReadToEnd();

                char key = inputRead[0];
                switch (key)
                {
                    case '0':
                        Random rand = new Random();
                        int id_tmp = rand.Next(100, 10000);

                        SendMessage(response, id_tmp.ToString());
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("ID " + id_tmp + " подключен");
                        Console.WriteLine();
                        Console.WriteLine();
                        return;

                    case '1':
                        int i;
                        for (i = 2; i < inputRead.Length; i++)
                            Console.Write(inputRead[i]);
                        Console.WriteLine();
                        SendMessage(response, "Сообщение отправлено");
                        break;

                    case '2':
                        i = 2;
                        int count = 0;
                        while (i < inputRead.Length)
                        {
                            if (inputRead[i] == '\n')
                                count++;

                            Console.Write(inputRead[i]);
                            i++;
                        }
                        Console.WriteLine();
                        SendMessage(response, "Отчет отправлен");
                        break;

                    case '3':
                        for (i = 2; i < inputRead.Length; i++)
                            Console.Write(inputRead[i]);

                        Console.Write(" отключился");
                        Console.WriteLine();
                        break;
                }
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
            byte[] buff;
            Stream output;

            buff = Encoding.UTF8.GetBytes(message);
            response.ContentLength64 = buff.Length;
            output = response.OutputStream;
            output.Write(buff, 0, buff.Length);
        }
    }
}
