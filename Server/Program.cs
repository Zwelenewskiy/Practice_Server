using System;
using MySql.Data.MySqlClient;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
namespace Server { 

    class Program
    {
        static void Main(string[] args){
            server server = new server();
            server.Start("http://localhost:8888/connection/");

            while (true){
                server.NewConnection();
            }
        }
    }
}