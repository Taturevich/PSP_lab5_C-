using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;
using System.Net;
using System.Net.Sockets;

namespace Lab5_psp_server
{
    public struct cell          // Структура ячейки
    {
        public float lbx; //левый нижний угол ячейки (координата по х)
        public float lby; //левый нижний угол ячейки(координата по y)
        public int hline; //линии-подсветки потенциальных ходов
        public int condition; //состояние ячейки  (текущий цвет ячейки; по дефолту все клетки черного цвета) 
        public bool locker; //блокирование ячейки от нажатия мыши (по дефолту все клетки свободны)
    }

    class OpenGL
    {
        cell[,] cells = new cell[11, 11];                // Матрица всего игрового поля
        int cnt1, cnt2, cnt3, cnt4;			//переменные для подсчета очков игроков 
        //bool checkMouse(float mx,float my);
        public int mx = 0, my = 0;		//переменные для координат мыши
        public int mx1 = 0, my1 = 0;		//переменные для координат мыши другого игрока
        public int count = 1;            //счетчик ходов игроков
        public int number=1;
        public bool pp=false;
        bool start = false;         // Переменная, позволяющая задавать начальные значения 
        bool[] keys = new bool[256];         // Массив для манипуляций с клавиатурой
        int i, j;					//переменные для обхода циклов
        bool down = false;             //клавиша не нажата
        byte[] buff = new byte[1024];


        void ReSizeGLScene()
        {
            //Gl.glViewport(0, 0, width, height);  // Сброс текущей области просмотра
            Gl.glMatrixMode(Gl.GL_PROJECTION);   // Выбор матрицы проектирования
            Gl.glLoadIdentity();              // Сброс матрицы проектирования
            //Gl.glOrtho(0.0f, width, height, 0.0f, -1.0f, 1.0f); // Создание ортог. вида 640x480 (0,0 – верх лево)
            Gl.glMatrixMode(Gl.GL_MODELVIEW);    // Выбор матрицы просмотра вида
            Gl.glLoadIdentity();              // Сброс матрицы просмотра вида
        }

        // Все настройки для OpenGL делаются здесь
        int InitGL()
        {
            Gl.glShadeModel(Gl.GL_SMOOTH);    // Разрешить плавное сглаживание
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.5f);   // Черный фон
            Gl.glClearDepth(1.0f);         // Настройка буфера глубины
            Gl.glHint(Gl.GL_LINE_SMOOTH_HINT, Gl.GL_NICEST); // Сглаживание линий
            Gl.glEnable(Gl.GL_BLEND);         // Разрешить смешивание
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA); // Тип смешивания
            return 0;                // Инициализация окончена успешно
        }

        //ФУНКЦИЯ ЗАКРАШИВАНИЯ ЯЧЕЙКИ
        void drawcell(int i, int j)
        {
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex2d(20 + (i * 60), 70 + (j * 60));
            Gl.glVertex2d(80 + (i * 60), 70 + (j * 60));
            Gl.glVertex2d(80 + (i * 60), 130 + (j * 60));
            Gl.glVertex2d(20 + (i * 60), 130 + (j * 60));
            Gl.glEnd();
        }

        //подсветка возможных ходов
        void drawhline(int i, int j, int condition)
        {
            if (j < 10 && i < 10)
            {
                Gl.glLineWidth(3.0f);               // Ширина линий 3.0f  
                switch (condition)
                {
                    case 1: Gl.glColor3f(1.0f, 0.0f, 0.0f); break;
                    case 2: Gl.glColor3f(0.0f, 1.0f, 0.0f); break;
                    case 3: Gl.glColor3f(0.0f, 0.0f, 1.0f); break;
                    case 4: Gl.glColor3f(1.0f, 1.0f, 0.0f); break;
                    default: break;
                }


                Gl.glBegin(Gl.GL_LINES);           // Начало рисования горизонтального бордюра ячейки
                Gl.glVertex2d(20 + (i * 60), 70 + (j * 60)); // Левая сторона горизонтальной линии
                Gl.glVertex2d(80 + (i * 60), 70 + (j * 60)); // Правая сторона горизонтальной линии
                Gl.glVertex2d(20 + (i * 60), 70 + ((j + 1) * 60)); // Нижняя грань
                Gl.glVertex2d(80 + (i * 60), 70 + ((j + 1) * 60)); // 
                Gl.glEnd();                     // Конец рисования горизонтального бордюра ячейки
            }
            if (i < 10 && j < 10)
            {
                Gl.glLineWidth(3.0f);               // Ширина линий 3.0f
                switch (condition)
                {
                    case 1: Gl.glColor3f(1.0f, 0.0f, 0.0f); break;
                    case 2: Gl.glColor3f(0.0f, 1.0f, 0.0f); break;
                    case 3: Gl.glColor3f(0.0f, 0.0f, 1.0f); break;
                    case 4: Gl.glColor3f(1.0f, 1.0f, 0.0f); break;
                    default: break;
                }
                Gl.glBegin(Gl.GL_LINES);           // Начало рисования вертикального бордюра ячейки
                Gl.glVertex2d(20 + (i * 60), 70 + (j * 60));  // Верхняя сторона вертикальной линии
                Gl.glVertex2d(20 + (i * 60), 130 + (j * 60)); // Нижняя сторона вертикальной линии
                Gl.glVertex2d(20 + ((i + 1) * 60), 70 + (j * 60));  // Верхняя сторона вертикальной линии
                Gl.glVertex2d(20 + ((i + 1) * 60), 130 + (j * 60)); // Нижняя сторона вертикальной линии
                Gl.glEnd();                     // Конец рисования вертикального бордюра ячейки*/
            }
        }

        //функция рисования линии
        void drawline(int i, int j)
        {
            if (j < 10 && i < 10)
            {
                Gl.glLineWidth(3.0f);               // Ширина линий 3.0f  
                Gl.glColor3f(1.0f, 1.0f, 1.0f);
                Gl.glBegin(Gl.GL_LINES);           // Начало рисования горизонтального бордюра ячейки
                Gl.glVertex2d(20 + (i * 60), 70 + (j * 60)); // Левая сторона горизонтальной линии
                Gl.glVertex2d(80 + (i * 60), 70 + (j * 60)); // Правая сторона горизонтальной линии
                Gl.glVertex2d(20 + (i * 60), 70 + ((j + 1) * 60)); // Нижняя грань
                Gl.glVertex2d(80 + (i * 60), 70 + ((j + 1) * 60)); // 
                Gl.glEnd();                     // Конец рисования горизонтального бордюра ячейки
            }
            if (i < 10 && j < 10)
            {
                Gl.glLineWidth(3.0f);               // Ширина линий 3.0f
                Gl.glColor3f(1.0f, 1.0f, 1.0f);
                Gl.glBegin(Gl.GL_LINES);           // Начало рисования вертикального бордюра ячейки
                Gl.glVertex2d(20 + (i * 60), 70 + (j * 60));  // Верхняя сторона вертикальной линии
                Gl.glVertex2d(20 + (i * 60), 130 + (j * 60)); // Нижняя сторона вертикальной линии
                Gl.glVertex2d(20 + ((i + 1) * 60), 70 + (j * 60));  // Верхняя сторона вертикальной линии
                Gl.glVertex2d(20 + ((i + 1) * 60), 130 + (j * 60)); // Нижняя сторона вертикальной линии
                Gl.glEnd();                     // Конец рисования вертикального бордюра ячейки*/
            }
        }

        //функция захвата одной точки
        public bool checkneighbors(int i, int j, int parity)
        {
            bool accept = true;
            if (i > 0 && cells[i - 1, j].condition != 0)
            {
                if ((cells[i - 1, j].condition) == parity)
                {
                    accept = false;
                }
            }
            if (cells[i + 1, j].condition != 0 && i < 10)
            {
                if ((cells[i + 1, j].condition) == parity)
                {
                    accept = false;
                }
            }
            if (j > 0 && cells[i, j - 1].condition != 0)
            {
                if ((cells[i, j - 1].condition) == parity)
                {
                    accept = false;
                }
            }
            if (j < 10 && cells[i, j + 1].condition != 0)
            {
                if ((cells[i, j + 1].condition) == parity)
                {
                    accept = false;
                }
            }
            //диагональные соседи
            if (i > 0 && j > 0 && cells[i - 1, j - 1].condition != 0)
            {
                if ((cells[i - 1, j - 1].condition) == parity)
                {
                    accept = false;
                }
            }
            if (i < 10 && j < 10 && cells[i + 1, j + 1].condition != 0)
            {
                if ((cells[i + 1, j + 1].condition) == parity)
                {
                    accept = false;
                }
            }
            if (i < 10 && j > 0 && cells[i + 1, j - 1].condition != 0)
            {
                if ((cells[i + 1, j - 1].condition) == parity)
                {
                    accept = false;
                }
            }
            if (i > 0 && j < 10 && cells[i - 1, j + 1].condition != 0)
            {
                if ((cells[i - 1, j + 1].condition) == parity)
                {
                    accept = false;
                }
            }
            if (accept == false)
                return true;
            else
                return false;
        }

        //функция доминирования (захват определенной части поля)
        public void fillneighbors(int i, int j)
        {
            //проверка соседних ячеек (согласно конфигурации)
            if (i > 0 && cells[i - 1, j].condition != 0)
            {
                if (cells[i, j].condition != cells[i - 1, j].condition && i > 0)
                    cells[i - 1, j].condition = cells[i, j].condition;
            }
            if (cells[i + 1, j].condition != 0)
            {
                if (cells[i, j].condition != cells[i + 1, j].condition && i < 10)
                    cells[i + 1, j].condition = cells[i, j].condition;
            }
            if (j > 0 && cells[i, j - 1].condition != 0)
            {
                if (cells[i, j].condition != cells[i, j - 1].condition && j > 0)
                    cells[i, j - 1].condition = cells[i, j].condition;
            }
            if (cells[i, j + 1].condition != 0)
            {
                if (cells[i, j].condition != cells[i, j + 1].condition && j > 0)
                    cells[i, j + 1].condition = cells[i, j].condition;
            }
            //диагональные соседи
            if (i > 0 && j > 0 && cells[i - 1, j - 1].condition != 0)
            {
                if (cells[i, j].condition != cells[i - 1, j - 1].condition && i > 0 && j > 0)
                    cells[i - 1, j - 1].condition = cells[i, j].condition;
            }
            if (i < 10 && j < 10 && cells[i + 1, j + 1].condition != 0)
            {
                if (cells[i, j].condition != cells[i + 1, j + 1].condition && i < 10 && j < 10)
                    cells[i + 1, j + 1].condition = cells[i, j].condition;
            }
            if (i < 10 && j > 0 && cells[i + 1, j - 1].condition != 0)
            {
                if (cells[i, j].condition != cells[i + 1, j - 1].condition && i < 10 && j > 0)
                    cells[i + 1, j - 1].condition = cells[i, j].condition;
            }
            if (i > 0 && j < 10 && cells[i - 1, j + 1].condition != 0)
            {
                if (cells[i, j].condition != cells[i - 1, j + 1].condition && i > 0 && j < 10)
                    cells[i - 1, j + 1].condition = cells[i, j].condition;
            }
        }

        //функция проверки значения координат с клика мыши
        public bool checkMouse(int mx, int my)
        {
            for (i = 0; i < 11; i++)   // Цикл слева направо
            {
                for (j = 0; j < 11; j++) // Цикл сверху вниз
                {
                    if (i < 10 && j < 10)
                    {
                        cells[i, j].lbx = 20 + (i * 60);
                        cells[i, j].lby = 50 + (j * 45);
                    }
                    //ПРОВЕРКА НА ПОПАДАНИЕ КЛИКА КУРСОРА В ПОДХОДЯЩУЮ КЛЕТКУ
                    if (/*mx>20 && my>50 && mx<620 && my<500*/mx > cells[i, j].lbx && mx < cells[i, j].lbx + 60 && my > cells[i, j].lby && my < cells[i, j].lby + 45 && cells[i, j].locker != true
                        && checkneighbors(i, j, count) == true)
                    {
                        
                        switch (count)
                        {
                            case 1:
                                cells[i, j].condition = 1; //первое состояние (цвет) клетки
                                fillneighbors(i, j); //закрашиваем вражеские соседние клетки; 
                                break;
                            case 2:
                                cells[i, j].condition = 2; //первое состояние (цвет) клетки
                                fillneighbors(i, j); //закрашиваем вражеские соседние клетки
                                break;
                            case 3:
                                cells[i, j].condition = 3; //первое состояние (цвет) клетки
                                fillneighbors(i, j); //закрашиваем вражеские соседние клетки
                                break;
                            case 4:
                                cells[i, j].condition = 4; //первое состояние (цвет) клетки
                                fillneighbors(i, j); //закрашиваем вражеские соседние клетки
                                break;
                            default: break;
                        }
                        Glut.glutPostRedisplay();
                        return true;
                    }
                }
            }
            return false;

        }

        //функция, обрабатывающая значения с клика мыши
        public void MousePressed(int button, int state, int ax, int ay)
        {
            down = button == Glut.GLUT_LEFT_BUTTON && state == Glut.GLUT_LEFT; //ЕСЛИ НАЖАТА (И НЕ ЗАЖАТА) ЛЕВАЯ КЛАВИША МЫШИ
            if (down && ((count) == 1))
            {
                Glut.glutPostRedisplay();
                start = true; //начинаем игру
                mx = ax; //назначаем переменной координату мыши по х
                my = ay; //назначаем переменной координату мыши по у
                if (checkMouse(mx, my))
                {
                    pp = true;
                    if (count != 4)
                        count++;
                    else
                        count = 1;
                }
            }
        }

        //ОСНОВНАЯ ФУНКЦИЯ ПРОРИСОВКИ
        public void display()
        {
            for (i = 0; i < 11; i++)   // Цикл слева направо
            {
                for (j = 0; j < 11; j++) // Цикл сверху вниз
                {
                    //НАЗНАЧАЕМ НАЧАЛЬНЫЕ КЛЕТКИ ИГРОКОВ
                    if (start == false)
                    {
                        Gl.glColor3f(1.0f, 0.0f, 0.0f);   //Цвет линии красный
                        cells[0, 0].condition = 1;
                        Gl.glColor3f(0.0f, 1.0f, 0.0f);   //Цвет линии зеленый
                        cells[0, 9].condition = 2;
                        Gl.glColor3f(0.0f, 0.0f, 1.0f);   //Цвет линии синий
                        cells[9, 0].condition = 3;
                        Gl.glColor3f(1.0f, 1.0f, 0.0f);   //Цвет линии желтый
                        cells[9, 9].condition = 4;
                        //if (i < 10 && j < 2)
                        //{
                        //    cells[i,j].condition = 1;
                        //    cells[i,j+1].hline = 1;
                        //}
                        //if (i < 10 && j<10 && j>7)
                        //{
                        //    cells[i,j].condition = 2;
                        //    cells[i,j - 1].hline = 1;
                        //}
                    }
                    //ЗАКРАШИВАЕМ НУЖНЫЕ КЛЕТКИ
                    if (i < 10 && j < 10) //не рисуем за пределами поля
                    {
                        //Gl.glColor3f(0.0f, 0.0f, 0.0f); //ЧЕРНЫЙ ЦВЕТ
                        switch (cells[i, j].condition)
                        {
                            case 1:
                                Gl.glColor3f(1.0f, 0.0f, 0.0f);   //Цвет линии красный
                                drawcell(i, j);
                                cnt1++;
                                cells[i, j].locker = true; //блокируем клетку от нажатия мыши
                                cells[i, j].hline = 0; //убираем подсветку
                                break;
                            case 2:
                                Gl.glColor3f(0.0f, 1.0f, 0.0f);
                                drawcell(i, j);
                                cnt2++;
                                cells[i, j].locker = true; //блокируем клетку от нажатия мыши
                                cells[i, j].hline = 0; //убираем подсветку
                                break;
                            case 3:
                                Gl.glColor3f(0.0f, 0.0f, 1.0f);
                                drawcell(i, j);
                                cnt3++;
                                cells[i, j].locker = true; //блокируем клетку от нажатия мыши
                                cells[i, j].hline = 0; //убираем подсветку
                                break;
                            case 4:
                                Gl.glColor3f(1.0f, 1.0f, 0.0f);
                                drawcell(i, j);
                                cnt4++;
                                cells[i, j].locker = true; //блокируем клетку от нажатия мыши
                                cells[i, j].hline = 0; //убираем подсветку
                                break;
                            default: break;
                        }
                    }
                    //РИСУЕМ СЕТКУ

                    drawline(i, j);
                }
            }
            //ИНФОРМАЦИЯ ДЛЯ ИГРОКОВ
            switch (count)
            {
                case 1:
                    Gl.glColor3f(1.0f, 0.0f, 0.0f);   //Цвет линии зеленый
                    drawcell(14, 1);
                    break;
                case 2:
                    Gl.glColor3f(0.0f, 1.0f, 0.0f);   //Цвет линии красный
                    drawcell(14, 1);
                    break;
                case 3:
                    Gl.glColor3f(0.0f, 0.0f, 1.0f);   //Цвет линии красный
                    drawcell(14, 1);
                    break;
                case 4:
                    Gl.glColor3f(1.0f, 1.0f, 0.0f);   //Цвет линии красный
                    drawcell(14, 1);
                    break;
                default: break;
            }

            switch (number)
            {
                case 1:
                    Gl.glColor3f(1.0f, 0.0f, 0.0f);   //Цвет линии красный
                    drawcell(14, 3);
                    break;
                case 2:
                    Gl.glColor3f(0.0f, 1.0f, 0.0f);   //Цвет линии зеленый
                    drawcell(14, 3);
                    break;
                case 3:
                    Gl.glColor3f(0.0f, 0.0f, 1.0f);   //Цвет линии синий
                    drawcell(14, 3);
                    break;
                case 4:
                    Gl.glColor3f(1.0f, 1.0f, 0.0f);   //Цвет линии желтый
                    drawcell(14, 3);
                    break;
                default: break;
            }


            if (cnt1 + cnt2 == 100) //если конец игры
            {
                start = false;
                if (cnt1 > cnt2)
                    Glut.glutSetWindowTitle("Красные победили");
                if (cnt1 < cnt2)
                    Glut.glutSetWindowTitle("Зеленые победили");
                if (cnt1 == cnt2)
                    Glut.glutSetWindowTitle("Ничья");
            }

            if (cnt1 + cnt2 != 100)
                cnt1 = cnt2 = 0; //обнуляем счетчики
            Glut.glutSwapBuffers();
        }

        //параметры окна OpenGL
        public void init()
        {
            //Gl.glutInit();
            /* установим черный фон */
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_RGB);
            Glut.glutInitWindowSize(1000, 1000);
            Glut.glutInitWindowPosition(100, 200);
            Glut.glutCreateWindow("SERVER");
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            /* инициализация viewing values */
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glOrtho(0, 1000, 1000, 0, -1, 1);
        }

        //функция, задающая анимацию
        //void Timer()
        //{
        //    Glut.glutPostRedisplay();
        //    Glut.glutTimerFunc(10, Timer, 0);
        //}

        //вызов функций рисования поля
        public void Opengl()
        {
            //glutTimerFunc(10, Timer, 1);
            init();
            Gl.glRasterPos2i(300, 300);
            Glut.glutMouseFunc(new Glut.MouseCallback(MousePressed));
            Glut.glutDisplayFunc(display);
            Glut.glutMainLoop();
            //return 0; /* ISO C requires main to return int. */
        }
    /// <summary>
    /// Посылаем координаты сервера
    /// </summary>
    /// <param name="my_sock"></param>
    public void Sender(Socket my_sock)
    {
        byte[] buff = new byte[1024];
        string s = mx + " " + my;
        Console.WriteLine(s);
        string request = GET_HTTP(s);
        Console.WriteLine(request);
        buff = Encoding.ASCII.GetBytes(request);
        my_sock.Send(buff);
        Array.Clear(buff, 0, buff.Length);
        //pp = false;
        //count = 2;
    }
    /// <summary>
    /// Посылаем координаты других игроков
    /// </summary>
    /// <param name="my_sock"></param>
    /// <param name="s"></param>
    public void Sender(Socket my_sock, string s)
    {
        byte[] buff = new byte[1024];
        Console.WriteLine(s);
        string request = GET_HTTP(s);
        Console.WriteLine(request);
        buff = Encoding.ASCII.GetBytes(request);
        my_sock.Send(buff);
        Array.Clear(buff, 0, buff.Length);
        //pp = false;
        //count = 2;
    }

    public string Receiver(Socket my_sock)
    {
        byte[] buff = new byte[1024];
        int bytesRec;
        bytesRec = my_sock.Receive(buff);
        string s = Encoding.ASCII.GetString(buff, 0, bytesRec);
        Console.WriteLine(s);
        string[] s1 = s.Split(' ');
        mx1 = Convert.ToInt32(s1[1]);
        my1 = Convert.ToInt32(s1[2]);
        if (checkMouse(mx1, my1))
        {
            Console.WriteLine(mx1 + " " + my1);
            if (count != 4)
                count++;
            else
                count = 1;
        }
        pp = false;
        Array.Clear(buff, 0, buff.Length);
        return s;
    }

    public static string GET_HTTP(string data)
    {
        string result = "GET " + data + " HTTP/1.1\r\n";
        result = result + "Host: 127.0.0.1\r\n";
        result = result + "Accept: */*\r\n";
        result = result + "Connection: keep-alive\r\n";
        result = result + "Accept-Language:ru\r\n";
        result = result + "Content-Length:" + data.Length + "\r\n";
        result = result + "Content: " + data;
        return result;
    }

    public string Answer(string dan)
    {
        string result = "";
        result = result + "HTTP/1.0 200 OK\r\n";
        result = result + "Date: " + DateTime.Now + "\r\n";
        result = result + "Server:My Server\r\n";
        result = result + "X-Powered-By:C#\r\n";
        result = result + "Content-Language:ru\r\n";
        result = result + "Content-type: text/html\n";
        result = result + "Content-Length:" + dan.Length + "\n\n";
        result = result + dan + "\r\n\r\n";
        return result;
    }

    }

}

