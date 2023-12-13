using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MPI;
using Xunit;

using PermutationsMPI;

namespace PermutationsMPI
{
    class Program

    {

        public static void huo(string[] args)
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
                            char[] values = new char[input.Length - 1];
                            int i = 0;
                            foreach (char c in characters)
                            {
                                if (c != first)
                                {
                                    values[i] = characters[i];
                                    i = i + 1;
                                }
                            }


                            world.Send(values, dest, 3);// отсылает букву

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
                    if (world.Rank > 2)
                    {
                        List<int[]> permutations = GeneratePermutations(characters, indexes);

                        foreach (var permutation in permutations)
                        {
                            string sout = "" + first;

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
        public static List<int[]> GeneratePermutations(char[] characters, int[] indexes)
        {
            List<int[]> permutations = new List<int[]>();
            GeneratePermutations(indexes, 0, indexes.Length, permutations);
            return permutations;
        }

        public static void GeneratePermutations(int[] indexes, int start, int end, List<int[]> permutations)
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

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}


namespace PermutationsMPITests
{
    public class ProgramTests : IDisposable
    {
        private readonly IDisposable mpiEnvironment;

        public ProgramTests()
        {
            string[] args = null; // или передайте аргументы, если необходимо
            mpiEnvironment = new MPI.Environment(ref args);
        }

        public void Dispose()
        {
            mpiEnvironment.Dispose();
        }

        [Fact]
        public void GeneratePermutations_ReturnsCorrectNumberOfPermutations()
        {
            // Arrange
            char[] characters = { 'A', 'B', 'C' };
            int[] indexes = { 0, 1, 2 };

            // Act
            var permutations = PermutationsMPI.Program.GeneratePermutations(characters, indexes);

            // Assert
            Assert.Equal(Factorial(characters.Length), permutations.Count);
        }

        [Fact]
        public void Swap_ChangesValues()
        {
            // Arrange
            int a = 5;
            int b = 10;

            // Act
            PermutationsMPI.Program.Swap(ref a, ref b);

            // Assert
            Assert.Equal(10, a);
            Assert.Equal(5, b);
        }

        [Fact]
        public void Swap_DoesNotChangeSameValues()
        {
            // Arrange
            int a = 5;
            int b = 5;

            // Act
            PermutationsMPI.Program.Swap(ref a, ref b);

            // Assert
            Assert.Equal(5, a);
            Assert.Equal(5, b);
        }

        [Fact]
        public void GeneratePermutations_from_8_chars()
        {
            System.Diagnostics.Process.Start("C:\\Users\\Andrey\\source\\repos\\ConsoleMpiProject\\ConsoleMpiProject\\bin\\Debug\\net8.0\\xxx.bat");
            Thread.Sleep(5000);
            int count = System.IO.File.ReadAllLines("output.txt").Length;
            Assert.Equal(40320, count);

        }

        [Fact]
        public void GeneratePermutations_ReturnsSinglePermutationForSingleCharacter()
        {
            // Arrange
            char[] characters = { 'A' };

            // Act
            var permutations = PermutationsMPI.Program.GeneratePermutations(characters, new int[] { 0 });

            // Assert
            Assert.Single(permutations);
        }

        // Helper method to calculate factorial
        private int Factorial(int n)
        {
            if (n == 0)
                return 1;
            else
                return n * Factorial(n - 1);
        }
    }
}