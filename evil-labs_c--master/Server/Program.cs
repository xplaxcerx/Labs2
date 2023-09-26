using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Server
{
    class Program
    {
        public struct Ad
        {
            public int X;
            public int Y;
            public bool Podtv;
        }
        static async Task Main()
        {
            Console.WriteLine("Ожидание клиента...\n");

            using (var serverPipe = new NamedPipeServerStream("tonel", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                await serverPipe.WaitForConnectionAsync();

                Console.WriteLine("Клиент подключен.\n");

                Ad receivedData = ReceiveData(serverPipe);
                Console.WriteLine($"Получены данные от клиента: X={receivedData.X}, Y={receivedData.Y}, Podtv={receivedData.Podtv}\n");

                receivedData.X += receivedData.Y;
                receivedData.Y -= receivedData.X;
                receivedData.Podtv = true;

                Console.WriteLine($"Отправляем данные клиенту: X={receivedData.X}, Y={receivedData.Y}, Podtv={receivedData.Podtv}\n");
                SendData(serverPipe, receivedData);

                await WaitForPipeDrainAsync(serverPipe);
            }

            Console.WriteLine("Сервер завершил работу.");
        }
        static Ad ReceiveData(NamedPipeServerStream pipe)
        {
            byte[] buffer = new byte[Marshal.SizeOf<Ad>()];
            pipe.Read(buffer, 0, buffer.Length);
            return MemoryMarshal.Read<Ad>(buffer);
        }

        static void SendData(NamedPipeServerStream pipe, Ad data)
        {
            byte[] buffer = new byte[Marshal.SizeOf<Ad>()];
            MemoryMarshal.Write(buffer, ref data);
            pipe.Write(buffer, 0, buffer.Length);
        }

        static async Task WaitForPipeDrainAsync(NamedPipeServerStream pipe)
        {
            var semaphore = new SemaphoreSlim(1);
            await semaphore.WaitAsync();
            semaphore.Release();
        }
    }
}
