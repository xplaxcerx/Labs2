﻿﻿using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Client;
class Program
{
    public struct Ad
    {
        public int X;
        public bool Podtv;
    }
    public static void Main()
    {
        try
        {
            Console.WriteLine("Соеденение с базой...\n");
            var stream = new NamedPipeClientStream(".", "tonel", PipeDirection.InOut);
            stream.Connect();
            Console.WriteLine("Соеденено \n");
            Console.WriteLine("Ожидание данных... \n");
            while (true)
            {
                byte[] array = new byte[Unsafe.SizeOf<Ad>()];
                stream.Read(array);
                var answer = MemoryMarshal.Read<Ad>(array);

                Console.WriteLine($"Получил: {answer.X}, {answer.Podtv}\n");
                answer.Podtv = true;
                Console.WriteLine($"Отправил {answer.X}, {answer.Podtv}...\n");

                byte[] spam = new byte[Unsafe.SizeOf<Ad>()];
                MemoryMarshal.Write<Ad>(spam, ref answer);
                stream.Write(spam);
            }
        }
        catch
        {
        }
    }   
}
