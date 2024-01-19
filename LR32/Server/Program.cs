﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public struct Ad
{
    public double A;
    public double B;
}

public struct Ud
{
    public double Result;
    public override string ToString() => $"Res = {Result}";
}

internal class Program
{
    private static int id = 0;
    static CancellationTokenSource up = new CancellationTokenSource();
    static CancellationToken token = up.Token;
    static PriorityQueue<Ad, string> queue = new PriorityQueue<Ad, string>();
    static Mutex mutex = new Mutex();

    private static Task clientTask(CancellationToken token)
    {
        return Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                var data = new Ad();
                Console.WriteLine($"Введите значение A -> ");
                var nach = Console.ReadLine();
                if (!double.TryParse(nach, out double valueA))
                {
                    Console.WriteLine("Ошибка: введено некорректное значение для A. Попробуйте заново.\n");
                    continue;
                }
                else
                {
                    data.A = valueA;
                }
                Console.WriteLine($"Введите значение B -> ");
                var conch = Console.ReadLine();
                if (!double.TryParse(conch, out double valueB))
                {
                    Console.WriteLine("Ошибка: введено некорректное значение для B. Попробуйте заново.\n");
                    continue;
                }
                else
                {
                    data.B = valueB;
                }

                queue.Enqueue(data, Guid.NewGuid().ToString()); // Using Guid as a non-nullable string
            }
        });
    }

    private static Task serverTask(CancellationToken token)
    {
        return Task.Run(() =>
        {
            List<Ud> uds = new List<Ud>();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Ad data;
                    lock (mutex)
                    {
                        if (queue.Count >= 1)
                        {
                            data = queue.Dequeue();
                        }
                        else
                        {
                            // Queue is empty, wait for a short time before checking again
                            Thread.Sleep(100);
                            continue;
                        }
                    }

                    Task task_3 = runclient(token, uds, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            foreach (var item in uds)
            {
                Console.WriteLine(item);
            }
        });
    }

    private static async Task runclient(CancellationToken token, List<Ud> uds, Ad data)
    {
        id++;
        string name = $"tonel_{id}";
        using (Process myProcess = new Process())
        {
            myProcess.StartInfo.FileName = "C:\\Users\\egoro\\Desktop\\LR3\\Client\\bin\\Debug\\net7.0\\Client.exe";
            myProcess.StartInfo.Arguments = name;
            myProcess.Start();

            var stream = new NamedPipeServerStream($"{name}", PipeDirection.InOut);
            await stream.WaitForConnectionAsync();

            byte[] spam = new byte[Unsafe.SizeOf<Ad>()];
            MemoryMarshal.Write(spam, ref data);
            await stream.WriteAsync(spam, token);

            byte[] array = new byte[Unsafe.SizeOf<Ud>()];
            await stream.ReadAsync(array, token);

            uds.Add(MemoryMarshal.Read<Ud>(array));

            await Task.Run(() => myProcess.WaitForExit());
        }
    }

    static async Task Main(string[] args)
    {
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            up.Cancel();
        };

        Console.WriteLine("Клиент подключен!\n");

        Task task_1 = serverTask(token);
        Task task_2 = clientTask(token);
        await Task.WhenAll(task_1, task_2);
    }
}