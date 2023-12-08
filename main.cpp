using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            using (new MPI.Environment(ref args))
            {
                Intracommunicator world = Communicator.world;
                if (world.Rank == 0)
                {
                    Console.WriteLine("Введите последовательность символов (латинские буквы без пробелов): ");
                    string input = Console.ReadLine();
   
                    char[] characters = input.ToCharArray();
   
                    int numOfProcesses = world.Size;
  
                    if (numOfProcesses > 1)
                    {
                        for (int dest = 1; dest < world.Size; dest++)
                        {
                            char first = input[dest - 1];
                            world.Send(first, dest, 2);
                            char[] values = new char[7];
                            int i = 0;
                            foreach(char c in characters)         
                            {
                                if (c != first)
                                {
                                    values[i] = characters[i];
                                    i=i+1;
                                }
                            }
                            world.Send(values, dest, 3);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка: Количество процессов должно быть больше одного.");
                    }
                }
                else // Для остальных процессов
                {
                    // Получение блока перестановок от мастер процесса

                    char first = world.Receive<char>(0, 2);
                    char[] characters = new char[7];
                    world.Receive(0, 3, ref characters);

                    int[] indexes = Enumerable.Range(0, characters.Length).ToArray();
                    if (world.Rank > 2) { 
                    List<int[]> permutations = GeneratePermutations(characters, indexes);
                    
                    foreach (var permutation in permutations)
                    {
                            string sout = ""+ first;
      
                        foreach (var index in permutation)
                        {
                                sout = sout + characters[index];
                         
                        }
                        Console.WriteLine(sout);
                    }

                }
            }
                
            }
            stopwatch.Stop();

            Console.WriteLine("Work Time is {0} ms", stopwatch.ElapsedMilliseconds);
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


