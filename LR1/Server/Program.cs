using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public struct Ad
    {
        public int X;
        public bool Podtv;

        public override string ToString() => $"Данные = {X}, Ответ = {Podtv}";
    }

    internal class Program
    {
        private static readonly CancellationTokenSource Up = new CancellationTokenSource();
        private static readonly CancellationToken Token = Up.Token;
        private static readonly PriorityQueue<Ad, int> Queue = new PriorityQueue<Ad, int>();
        private static readonly Mutex Mutex = new Mutex();

        private static Task ClientTask(CancellationToken token)
        {
            return Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine("Введите значение: ");
                    var value = Console.ReadLine();
                    if (value.Length == 0)
                    {
                        Console.WriteLine("Вы не ввели цифры, попробуйте снова: \n");
                        continue;
                    }

                    Console.WriteLine("Введите приоритет: ");
                    var priority = Console.ReadLine();
                    if (priority.Length == 0)
                    {
                        Console.WriteLine("Вы не ввели цифры, попробуйте снова: \n");
                        continue;
                    }
                    var data = new Ad { X = Convert.ToInt32(value), Podtv = false };

                    lock (Mutex)
                    {
                        Queue.Enqueue(data, Convert.ToInt32(priority));
                    }
                }
            });
        }

        private static Task ServerTask(NamedPipeServerStream stream, CancellationToken token)
        {
            return Task.Run(() =>
            {
                List<Ad> uds = new List<Ad>();
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (Queue.Count >= 1)
                        {
                            lock (Mutex)
                            {
                                var data = Queue.Dequeue();
                                byte[] spam = new byte[Unsafe.SizeOf<Ad>()];
                                MemoryMarshal.Write(spam, ref data);
                                stream.Write(spam);
                                byte[] array = new byte[Unsafe.SizeOf<Ad>()];
                                stream.Read(array);
                                uds.Add(MemoryMarshal.Read<Ad>(array));
                            }
                        }
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IOException in ServerTask: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred in ServerTask: {ex.Message}");
                }
                finally
                {
                    stream.Dispose(); 
                }

                foreach (var item in uds)
                {
                    Console.WriteLine(item);
                }
            });
        }

        private static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                Up.Cancel();
            };

            try
            {
                using (var stream = new NamedPipeServerStream("tonel", PipeDirection.InOut))
                {
                    stream.WaitForConnection();
                    Console.WriteLine("Клиент подключен\n");
                    Task task1 = ServerTask(stream, Token);
                    Task task2 = ClientTask(Token);
                    await Task.WhenAll(task1, task2);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IOException in Main: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred in Main: {ex.Message}");
            }
            finally
            {
                Up.Dispose();  
            }
        }
    }
}
#An unexpected error occurred in ServerTask: Index was outside the bounds of the array. 
