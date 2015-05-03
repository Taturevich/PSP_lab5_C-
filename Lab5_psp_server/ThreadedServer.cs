using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace Lab5_psp_server
{
    class ThreadedServer
    {
        private Socket _serverSocket;
        private int _port;
        private OpenGL Gl=new OpenGL();
        //private StreamWriter writer = new StreamWriter("www\\coordinates.txt");
        private int count = 2; //счетчик ходов
        private int sender = 0; //счетчик для отправки
        private int number = 1;
        private Object thisLock = new Object();
        private bool accept = false;

        public ThreadedServer(int port)
        { _port = port; }

        private class ConnectionInfo
        {
            public Socket Socket;
            public Thread Thread;
        }

        private Thread _acceptThread;
        private List<ConnectionInfo> _connections = new List<ConnectionInfo>();
        private List<int> locker = new List<int>();

        public void Start()
        {
            SetupServerSocket();
            //_acceptThread = new Thread(Gl.Opengl);
            //_acceptThread.IsBackground = true;
            //_acceptThread.Start();
            AcceptConnections();
        }

        private void SetupServerSocket()
        {
            // Получаем информацию о локальном компьютере
            IPHostEntry localMachineInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint myEndpoint = new IPEndPoint(localMachineInfo.AddressList[1], _port);
            IPEndPoint myEndpointlocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            //Console.WriteLine("Host address: " + localMachineInfo.AddressList[1]);
            Console.WriteLine("Host address: " + myEndpointlocal);
            Console.WriteLine("Host address: " + myEndpoint);
            // Создаем сокет, привязываем его к адресу
            // и начинаем прослушивание
            _serverSocket = new Socket(myEndpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(myEndpoint);
            //_serverSocket.Bind(myEndpointlocal);
            _serverSocket.Listen((int)SocketOptionName.MaxConnections);
        }

        private void AcceptConnections()
        {
            while (true)
            {
                
                //Console.WriteLine(Request);
                // Принимаем соединение
                Socket socket = _serverSocket.Accept();
                //socket.Receive(buffer);
                //string s1 = Encoding.ASCII.GetString(buffer);
                //string[] s2 = s1.Split(' ');
                // Создаем поток для получения данных
                ConnectionInfo connection = new ConnectionInfo();
                connection.Socket = socket;
                connection.Thread = new Thread(RequestHandler);
                connection.Thread.IsBackground = true;
                connection.Thread.Start(connection);



                    //string Html = "<html><body><h1>It works!</h1></body></html>";
                    //string s3 = Gl.Answer(Html);
                    //Console.WriteLine(s3);
                    //buffer = Encoding.ASCII.GetBytes(s3);
                    //socket.Send(buffer);
                    //Console.WriteLine("New client");
                    //Console.WriteLine(s1);
  
                    //Console.WriteLine(s2[1]);
                   
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessConnection), connection);
                    //ThreadPool.SetMaxThreads(4, 4);
                    //// Сохраняем сокет
                    //lock (_connections) _connections.Add(connection);
               
                
                
                

                //ThreadPool.SetMinThreads(2, 4);

                
            }
        }

        public void RequestHandler(object state)
        {
            ConnectionInfo conn = (ConnectionInfo)state;
            Socket socket = conn.Socket;
            byte[] buffer = new byte[1024];
            string Request;
            // Переменная для хранения количества байт, принятых от клиента
            int Count;
            while (true)
            {
                
                Request = "";
                do
                {
                    Count = socket.Receive(buffer);
                    Console.WriteLine(Count);
                    // Преобразуем эти данные в строку и добавим ее к переменной Request
                    Request += Encoding.ASCII.GetString(buffer, 0, Count);
                    //string Request1 = Encoding.ASCII.GetString(buffer, 0, Count);
                    Array.Clear(buffer, 0, buffer.Length);
                    //Console.WriteLine(Request1);
                    if (Count>0)
                        Count = 0;
                    // Запрос должен обрываться последовательностью \r\n\r\n
                    // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
                    // Нам не нужно получать данные из POST-запроса (и т. п.), а обычный запрос
                    // по идее не должен быть больше 4 килобайт
                    //if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
                    //{
                    //    break;
                    //}
                } while ( Count != 0);

                //Console.WriteLine("\n\n1111");
                Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");
                //Console.WriteLine(ReqMatch.Groups[1].Value);
                if (ReqMatch == Match.Empty)
                {
                    // Передаем клиенту ошибку 400 - неверный запрос
                    Console.WriteLine("Error 400");
                    return;
                }
                string RequestUri = ReqMatch.Groups[1].Value;
                Console.WriteLine(RequestUri);
                RequestUri = Uri.UnescapeDataString(RequestUri);
                // Если строка запроса оканчивается на "/", то добавим к ней index.html

                switch (RequestUri)
                {
                    case "/" :
                        BrowserFunction(socket,RequestUri);break;
                    case "/number":
                        SendNumber(socket, Request);break;
                    case "/send":
                        ReceiveDate(socket, Request);break;
                    case "/recv":
                        SendDate(socket, Request);break;
                    default: break;

                }

                //Console.WriteLine(RequestUri);

                
                //socket.Close();
 
            }
              
        }

        /// <summary>
        /// Работа с дефолтным запросом браузера
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="request"></param>
        private void BrowserFunction(Socket socket, string RequestUri)
        {
            byte[] buffer = new byte[1024];
            RequestUri += "coordinates.txt";
            string FilePath = "www/" + RequestUri;
            // Если в папке www не существует данного файла, посылаем ошибку 404
            if (!File.Exists(FilePath))
            {
                Console.WriteLine("Error 404");
                return;
            }
            //Console.WriteLine(FilePath);
            // Получаем расширение файла из строки запроса
            string Extension = RequestUri.Substring(RequestUri.LastIndexOf('.'));

            // Тип содержимого
            string ContentType = "";
            // Пытаемся определить тип содержимого по расширению файла
            switch (Extension)
            {
                case ".htm":
                case ".html":
                case ".txt":
                    ContentType = "text/html";
                    break;
                case ".css":
                    ContentType = "text/stylesheet";
                    break;
                case ".js":
                    ContentType = "text/javascript";
                    break;
                case ".jpg":
                case ".ico":
                    ContentType = "image/jpeg";
                    break;
                case ".jpeg":
                case ".png":
                case ".gif":
                    ContentType = "image/" + Extension.Substring(1);
                    break;
                default:
                    if (Extension.Length > 1)
                    {
                        ContentType = "application/" + Extension.Substring(1);
                    }
                    else
                    {
                        ContentType = "application/unknown";
                    }
                    break;
            }
            // Открываем файл, страхуясь на случай ошибки
            FileStream FS;
            try
            {
                FS = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                // Если случилась ошибка, посылаем клиенту ошибку 500
                Console.WriteLine("Ошибка 500");
                return;
            }
            // Посылаем заголовки
            string Headers = "HTTP/1.1 200 OK\nContent-Type: " + ContentType + "\nContent-Length: " + FS.Length + "\n\n";
            byte[] HeadersBuffer = Encoding.ASCII.GetBytes(Headers);
            socket.Send(HeadersBuffer);

            // Пока не достигнут конец файла
            while (FS.Position < FS.Length)
            {
                // Читаем данные из файла
                FS.Read(buffer, 0, buffer.Length);
                // И передаем их клиенту
                socket.Send(buffer);
            }
            FS.Close();
        }



        /// <summary>
        /// Отсылаем номер подключившемуся игроку 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="request"></param>
        private void SendNumber(Socket socket, string request)
        {
            byte[] buffer = new byte[1024];
            string Msg = "<html><body><h1>"+number+"</h1></body></html>";
            string Headers = "HTTP/1.1 200 OK\nContent-Type: text/html\nContent-Length: " + Msg.Length + "\n\n" + Msg;
            buffer = Encoding.ASCII.GetBytes(Headers);
            socket.Send(buffer);
            Array.Clear(buffer,0,buffer.Length);
            Console.WriteLine(Headers);
            number++;
        }

        /// <summary>
        /// Извлекает из текста запроса нужную нам информацию
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string Parser(string request)
        {
            int i;
            i = request.IndexOf("Content:");
            request=request.Substring(i+9, request.Length - i-9);
            Console.WriteLine(request);
            return request;
        }


        /// <summary>
        /// Принимаем данные от игрока 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="request"></param>
        private void ReceiveDate(Socket socket, string request)
        {
            byte[] buffer = new byte[1024];
            int bytesrecv;
            int newcount;
            Console.WriteLine("\n\n"+request+"\n\n");
            lock (thisLock)
            {
                StreamWriter Writer = new StreamWriter("www\\coordinates.txt", true, Encoding.UTF8);
                string value = Parser(request);
                string[] s1 = value.Split(' ');
                newcount = Convert.ToInt32(s1[2]);
                //Console.WriteLine(value);
                Writer.WriteLine(value);
                Writer.Close();
            }
            //string[] readText = System.IO.File.ReadAllLines("coordinates.txt", Encoding.Default);
            //Date = readText[readText.Length - 1];
            string Msg = "<html><body><h1>STOP</h1></body></html>";
            string Headers = "HTTP/1.1 200 OK\nContent-Type: text/html\nContent-Length: " + Msg.Length + "\n\n" + Msg + "\r\n\r\n";
            buffer = Encoding.ASCII.GetBytes(Headers);
            socket.Send(buffer);
            Array.Clear(buffer, 0, buffer.Length);
            //if (newcount==count)
            //{
                locker.Clear();
                accept = true;
                sender = 0;
            //}
            //Console.WriteLine(Headers);
 
        }

        /// <summary>
        /// Отсылаем изменения остальным игрокам и делаем переход хода
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="request"></param>
        private void SendDate(Socket socket, string request)
        {
            Thread.Sleep(1000);
            byte[] buffer = new byte[1024];
            string Date;
            string value = Parser(request);
            int newcount = int.Parse(value);
            if (accept)
                Console.WriteLine("assssssssssssssss" + newcount);
            if (accept == true && !locker.Contains(newcount))
            {
                lock (thisLock)
                {
                    string[] readText = System.IO.File.ReadAllLines("www\\coordinates.txt", Encoding.Default);
                    Date = readText[readText.Length - 1];
                }
                string Msg = "<html><body><h1>" + Date + "</h1></body></html>";
                string Headers = "HTTP/1.1 200 OK\nContent-Type: text/html\nContent-Length: " + Msg.Length + "\n\n" + Msg + "\r\n\r\n";
                buffer = Encoding.ASCII.GetBytes(Headers);
                socket.Send(buffer);
                Array.Clear(buffer, 0, buffer.Length);
                sender++;
                locker.Add(newcount);
                //Console.WriteLine("Элемент списка: " + locker[0]);
                if (sender == 3)
                {
                    accept = false;
                    if (count != 4)
                        count++;
                    else
                        count = 1;
                }
                    
            }
            else
            {
                string Msg = "<html><body><h1>NO</h1></body></html>";
                string Headers = "HTTP/1.1 200 OK\nContent-Type: text/html\nContent-Length: " + Msg.Length + "\n\n" + Msg + "\r\n\r\n";
                buffer = Encoding.ASCII.GetBytes(Headers);
                socket.Send(buffer);
                Array.Clear(buffer, 0, buffer.Length);
                Console.WriteLine("Отправил");
            }
            
 
        }

        private void ProcessConnection(object state)
        {
            ConnectionInfo connection = (ConnectionInfo)state;
            byte[] buffer = new byte[255];
            //Посылаем игроку его порядковый номер 
            Console.WriteLine(_connections.IndexOf(connection));
            Gl.Sender(connection.Socket, Convert.ToString(_connections.IndexOf(connection)));
            try
            {
                Gl.pp = false;
                while (true)
                {
                    if (Gl.pp == true)
                    {
                        Console.WriteLine("Отправляю");
                        lock (_connections)
                        {
                            foreach (ConnectionInfo conn in _connections)
                            {
                                    Console.WriteLine("Посылаю всем");
                                    Gl.Sender(conn.Socket);
                                    //conn.Socket.Send(buffer, bytesRead, SocketFlags.None);
                            }
                        }
                        Gl.pp = false;
                    }

                    if (Gl.pp == false && Gl.count!=1)
                    {
                        string s1=Gl.Receiver(connection.Socket);
                        Console.WriteLine(s1);
                        lock (_connections)
                        {
                            foreach (ConnectionInfo conn in _connections)
                            {
                                if (conn != connection)
                                {
                                    Console.WriteLine("Посылаю всем");
                                    Gl.Sender(conn.Socket, s1);
                                    //conn.Socket.Send(buffer, bytesRead, SocketFlags.None);
                                }
                            }
                        }
                    }
                    //else if (buffer.) return;
                }
            }
            catch (SocketException exc)
            {
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc);
            }
            finally
            {
                connection.Socket.Close();
                lock (_connections) _connections.Remove(connection);
            }
        }



    }
}
