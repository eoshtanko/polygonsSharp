using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace HW2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Количество деревьев.
            int N = 0;
            // Точка с минимальной координатой Y или, если таких несколько,
            // самая "левая" из них(т.е. с минимальным значением X).
            Point q;
            string clockwise = args[0];
            string format = args[1];
            // Коллекция со всеми поступившими координатами.
            List<Point> points = ReadFromFile(ref N, Path(args[2]));
            // Выводим строку, которая повторяет входные данные.
            if (format == "wkt")
            {
                File.WriteAllText(Path(args[3]), MultiPointString(points));
            }
            points = points.OrderBy(x => x.Y).ThenBy(x => x.X).ToList();
            q = points[0];
            points.Remove(q);
            // Точки, отсортированные в порядке возрастания полярного угла,
            // измеряемого против часовой стрелки относительно точки q.
            points = SortPoints(points, q);
            Stack<Point> stack = Graham(N, q, points);
            points.Insert(0, q);
            Out(Path(args[3]), format, clockwise, stack);
        }

        /// <summary>
        /// Данный метод позволит работать с файлами как через полный, так и через относительный пути.
        /// </summary>
        /// <param name="path">Введенный через командную строку путь.</param>
        /// <returns></returns>
        static string Path(string path)
        {
            if (File.Exists(path))
                return path;
            if (File.Exists("input\\" + path))
                return "input\\" + path;
            if (File.Exists("output\\" + path))
                return "output\\" + path;
            throw new FileNotFoundException("Ошибка. Такого файла не существует.");
        }

        /// <summary>
        /// Алгоритм Грэхема.
        /// </summary>
        /// <param name="N">Количество пар координат.</param>
        /// <param name="q">Точка с min Y(если таких несколько с min X из них).</param>
        /// <param name="points">Пары координат.</param>
        /// <returns></returns>
        static Stack<Point> Graham(int N, Point q, List<Point> points)
        {
            Stack<Point> stack = new Stack<Point>(N);
            stack.Push(q);
            stack.Push(points[0]);
            for (int i = 1; i < points.Count; i++)
            {
                while (stack.Size() >= 2 && !Left(stack.NextToTop(), stack.Top(), points[i]))
                {
                    stack.Pop();
                }
                stack.Push(points[i]);
            }
            return stack;
        }

        /// <summary>
        /// Вывод информации в файл.
        /// </summary>
        /// <param name="path">Путь к файлу для вывода.</param>
        /// <param name="format">Формат вывода.</param>
        /// <param name="clockwise">Требование к выводу: по часовой/противчасовой.</param>
        /// <param name="points">Все изначально введенные координаты.</param>
        /// <param name="stack">Выпуклая оболочка.</param>
        static void Out(string path, string format, string clockwise, Stack<Point> stack)
        {
            if (format == "plain")
            {
                File.WriteAllText(path, Plain(stack, clockwise));
            }
            else
            {
                File.AppendAllText(path, WKT(stack, clockwise));
            }
        }

        /// <summary>
        /// Формирует строку, содержащую полигон.
        /// </summary>
        /// <param name="stack">Выпуклая оболочка.</param>
        /// <param name="clock">Требование к выводу: по часовой/противчасовой.</param>
        /// <returns></returns>
        static string WKT(Stack<Point> stack, string clock)
        {
            string tempRes = "";
            string res;
            Point first;
            if (clock == "cw")
            {
                first = stack.First();
                foreach (var point in stack.Reverse())
                {
                    if (point != stack.First())
                    {
                        tempRes += $"{point.X} {point.Y}, ";
                    }
                }
            }
            else
            {
                first = stack.First();
                foreach (var point in stack)
                {
                    if (point != stack.First())
                    {
                        tempRes += $"{point.X} {point.Y}, ";
                    }
                }
            }
            res = $"\nPOLYGON (({first.X} {first.Y}, ";
            res += tempRes;
            res += $"{first.X} {first.Y}))";
            return res;
        }

        /// <summary>
        /// Приведение выходных данных к формату Plain
        /// </summary>
        /// <param name="stack">Выпуклая оболочка.</param>
        /// <param name="clock">Требование к выводу: по часовой/противчасовой.</param>
        /// <returns></returns>
        static string Plain(Stack<Point> stack, string clock)
        {
            string res = "";
            res += stack.Size() + "\n";

            if (clock == "cw")
            {
                res += stack.First() + "\n";
                foreach (var point in stack.Reverse())
                {
                    if (point != stack.First())
                    {
                        res += point + "\n";
                    }
                }
            }
            else
            {
                foreach (var point in stack)
                {
                    res += point + "\n";
                }
            }
            return res;
        }

        /// <summary>
        /// Формирует строку, содержащую изначально введенные координаты.
        /// </summary>
        /// <param name="points">Все изначально введенные координаты.</param>
        /// <returns></returns>
        static string MultiPointString(List<Point> points)
        {
            string res = "MULTIPOINT (";
            for (int i = 0; i < points.Count; i++)
            {
                res += $"({points[i].X} {points[i].Y})";
                if (i < points.Count - 1)
                {
                    res += ", ";
                }
            }
            res += ")";
            return res;
        }

        /// <summary>
        /// Парсит строку в две координаты. 
        /// </summary>
        /// <param name="input">Пара точек.</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static void ParseString(string input, ref int x, ref int y)
        {
            string reg = @"(\d+) (\d+)";
            Match memb = Regex.Match(input, reg);
            x = int.Parse(memb.Groups[1].ToString());
            y = int.Parse(memb.Groups[2].ToString());
        }

        /// <summary>
        /// Заполняет коллекцию точек.
        /// </summary>
        /// <param name="N">Количество пар.</param>
        /// <param name="input">Координаты в строковом формате.</param>
        /// <returns></returns>
        static List<Point> FullPointList(int N, string[] input)
        {
            List<Point> points = new List<Point>(N);
            for (int i = 0; i < N; i++)
            {
                int x = 0, y = 0;
                ParseString(input[i + 1], ref x, ref y);
                Point point = new Point(x, y);
                points.Add(point);
            }
            return points;
        }

        /// <summary>
        /// Чтение пар координат из файла.
        /// </summary>
        /// <param name="N">Количество пар.</param>
        /// <returns>Коллекция пар координат.</returns>
        static List<Point> ReadFromFile(ref int N, string path)
        {
            string[] input = File.ReadAllLines(path);
            N = int.Parse(input[0]);
            // Заполненный, но еще не отсортированный стек.
            List<Point> points = FullPointList(N, input);
            return points;
        }

        /// <summary>
        /// Высчитывает полярный угол относительно точки q.
        /// </summary>
        /// <param name="p">Точка.</param>
        /// <param name="q">Точка, относительно которой расчет.</param>
        /// <returns>Полярный угол.</returns>
        static double Angle(Point p, Point q)
        {
            if (p.X == q.X && p.Y == q.Y) return 0;
            if (p.X - q.X == 0) return Math.PI / 2;
            if (p.X - q.X < 0)
                return Math.Atan((p.Y - q.Y) / (double)(p.X - q.X)) + Math.PI;
            return Math.Atan((p.Y - q.Y) / (double)(p.X - q.X));
        }

        /// <summary>
        /// Сортирует точки в порядке возрастания полярного угла.
        /// </summary>
        /// <param name="points">Коллекция для сортировки.</param>
        /// <param name="q">Точка, относительно которой сортировка.</param>
        /// <returns>Отсортированные точки в порядке возрастания полярного угла.</returns>
        static List<Point> SortPoints(List<Point> points, Point q)
        {
            return points.OrderBy(x => Angle(x, q)).ToList();
        }

        /// <summary>
        /// Определяет, образуют ли три точки a, b, c левый поворот.
        /// </summary>
        /// <param name="a">Предпоследняя точка в стеке.</param>
        /// <param name="b">Последняя точка в стеке.</param>
        /// <param name="c">"Новая" точка.</param>
        /// <returns>True - поворот левый. False - поворот не левый.</returns>
        static bool Left(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X) > 0;
            // >0 - left 
            // =0 - on the line
            // <0 - right
        }
    }

    public class Point
    {
        int x;
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                if (value < 0 || value > 10000)
                {
                    throw new ArgumentException("Значение X должно быть в пределах от 0 до 10000");
                }
                x = value;
            }
        }

        int y;
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                if (value < 0 || value > 10000)
                {
                    throw new ArgumentException("Значение Y должно быть в пределах от 0 до 10000");
                }
                y = value;
            }
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{X} {Y}";
        }
    }

    public class Stack<T> : IEnumerable<T>
    {
        List<T> stack = new List<T>();
        public const long MaxValue = 1000; // Максимально допустимый размер стека.
        int max;
        /// <summary>
        /// Конструкор для создания стека.
        /// </summary>
        /// <param name="max">Максимальный размер стека.</param>
        public Stack(int max)
        {
            if (max > MaxValue)
            {
                throw new ArgumentException("Ошибка. Размер превышает максимально допустимый.");
            }
            this.max = max;
        }

        /// <summary>
        /// Количество элементов в стеке.
        /// </summary>
        public int Size()
        {
            return stack.Count;
        }

        /// <summary>
        /// Если стек пуст возвращает True, иначе - False.
        /// </summary>
        public bool IsEmpty()
        {
            return Size() == 0;
        }

        /// <summary>
        /// Возвращает предпоследний элемент стека без его удаления.
        /// </summary>
        /// <returns>Предпоследний элемент стека.</returns>
        public T NextToTop()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("Ошибка. Стек пуст.");
            }
            if (Size() < 2)
            {
                throw new InvalidOperationException("Ошибка. В стеке недостаточно элементов.");
            }
            return stack[Size() - 2];
        }

        /// <summary>
        /// Возвращает последний элемент стека без его удаления.
        /// </summary>
        /// <returns>Последний элемент стека.</returns>
        public T Top()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("Ошибка. Стек пуст.");
            }
            return stack[Size() - 1];
        }

        /// <summary>
        /// Возвращает последний элемент стека и удаляет его.
        /// </summary>
        /// <returns>Последний элемент стека.</returns>
        public T Pop()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("Ошибка. Стек пуст.");
            }
            T last = stack[Size() - 1];
            stack.Remove(stack[Size() - 1]);
            return last;
        }

        /// <summary>
        /// Добавляет элемент в конец стека.
        /// </summary>
        /// <param name="item">Элемент для добавления в конец стека.</param>
        public void Push(T item)
        {
            if (Size() == MaxValue)
            {
                throw new InvalidOperationException("Ошибка. Достигнуто максимально допустимое кол-во элементов в стеке.");
            }
            stack.Add(item);
        }

        // Далее я реализовала интерфейс IEnumerable<T>, что позволит
        // перебирать стек в foreach.
        public IEnumerator<T> GetEnumerator()
        {
            return stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
