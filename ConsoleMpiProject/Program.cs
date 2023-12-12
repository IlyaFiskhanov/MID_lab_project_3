
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using MPI;
//для запуска выполнить команду в папке с файлом exe
//mpiexec -n 9 ConsoleMpiProject.exe

namespace PermutationsMPI
{
    class Program

    {

        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch(); // инициализация таймера 

            stopwatch.Start();  // старт таймера 
            using (new MPI.Environment(ref args))
            {
                Intracommunicator world = Communicator.world;
                if (world.Rank == 0) // задания для первого процесса
                {
                    StreamReader sr = new StreamReader("output.txt"); // откыть файл с заданием из 8 символов
                    string input = sr.ReadLine(); // считать задание из 8 символов
                    sr.Close(); // закрыть файл
                    File.Create("output.txt").Close(); // очистка файла

                    char[] characters = input.ToCharArray(); // преобразовать задание в массив символов

                    int numOfProcesses = world.Size; // узнать колличество процессов исходя из команды запуска (mpiexec)


                    if (numOfProcesses > 1) // проверка на запуск без команды (mpiexec)
                    {
                        for (int dest = 1; dest < world.Size; dest++)  // для каждого процесса выдаем индивидуальное задание
                        {
                            char first = input[dest - 1];  // индивидуальный первый символ задания для процесса
                            world.Send(first, dest, 2);  // отправка процесса с идентификатором 2
                            char[] values = new char[7];  //  создаем массив из 7 символов
                            int i = 0;
                            int k = 0;
                            foreach (char c in characters)  //  заполняем массив символами которые не попали в first
                            {
                                if (c != first)
                                {
                                    values[i] = characters[k];
                                    i = i + 1;
                                }
                                k= k + 1;   
                            }
                            world.Send(values, dest, 3);  // отправляем задание процессу (идентификатор 3)
                        }
                        
                        string[] output_names= { };
                        Thread.Sleep(2000); // ждем завершения всех процессов при необходимости увеличить

                        // 
                        // 
                        // сбор всех данных не происходит при помощи    world.Send в исходный процесс, так как возникают непреодолимые сложности с передачей
                        // данный типа string, а при передаче посимвольно скорость сравнима с программами не применяющими MPI
                        // 

                        for (int dest = 1; dest < 9; dest++)  //  собираем наработки от процессов
                        {
                                if (File.Exists("output" + dest + ".txt"))
                                {
                                    string text = File.ReadAllText("output" + dest + ".txt");  
                                File.AppendAllText("output.txt", text);  //  добавляем наработки в исходный файл

                                output_names.Append("output" + dest + ".txt");
                                    File.Delete("output" + dest + ".txt");  // удаляем промежуточный результат работы


                            }



                            }
                        

                    }
                    else // обработка ошибки одного процесса
                    {
                        Console.WriteLine("Ошибка: Количество процессов должно быть больше одного.");
                    }
                }
                else // Для остальных 8 процессов
                {
                    // Получение блока перестановок от мастер процесса

                    char first = world.Receive<char>(0, 2); // получаем первый элемент
                    string result = "";
                    char[] characters = new char[7];
                    world.Receive(0, 3, ref characters); // получаем 7 символов для перестановок

                    int[] indexes = Enumerable.Range(0, characters.Length).ToArray(); // нумерация элементов 0,1,2,3,4,5,6


                    List<int[]> permutations = GeneratePermutations(characters, indexes); // генерация перестановок

                    foreach (var permutation in permutations) // сборка перестановок
                    {
                                string sout = "" + first; // добавление первого элемнта(одинаковый длф всех перестановок процесса)

                            foreach (var index in permutation)
                                {
                                    sout = sout + characters[index];  // замена номера элемента на символ из задания

                        }
                                result = result + (sout + "\r\n"); // добавление перестановки в результирующую переменную


                    }
                    string Path = "output" + world.Rank + ".txt";
                    File.AppendAllText(Path, result); // создание файла с промежуточным результатом


                }
                
            }
            stopwatch.Stop(); // остановка таймера

            Console.WriteLine("Work Time is {0} ms", stopwatch.ElapsedMilliseconds); // вывод времени работы процесса
        }

        // Функция генерации перестановок
        static List<int[]> GeneratePermutations(char[] characters, int[] indexes) 
        {
            List<int[]> permutations = new List<int[]>();
            GeneratePermutations(indexes, 0, indexes.Length, permutations);
            return permutations;
        }

        static void GeneratePermutations(int[] indexes, int start, int end, List<int[]> permutations)
        {
            if (start == end)
            {
                permutations.Add(indexes.ToArray());
            }
            else
            {
                for (int i = start; i < end; i++)
                {
                    Swap(ref indexes[start], ref indexes[i]);
                    GeneratePermutations(indexes, start + 1, end, permutations);
                    Swap(ref indexes[start], ref indexes[i]);
                }
            }
        }

        static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}

