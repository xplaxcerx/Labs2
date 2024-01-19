﻿using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        public struct Ad
        {
            public double A;
            public double B;
            public double sum;
        }

        public struct Ud
        {
            public double Result;
        }

        static async Task Main(string[] args)
        {
            try
            {
                int i;
                var name = args[0];
                var stream = new NamedPipeClientStream(".", name, PipeDirection.InOut);
                await stream.ConnectAsync();
                byte[] array = new byte[Unsafe.SizeOf<Ad>()];
                await stream.ReadAsync(array);
                var answer = MemoryMarshal.Read<Ad>(array);

                var result = 0.0;
                double h = (answer.B - answer.A) / 1000;
                double h2 = (answer.B - answer.A) * h;

                for (i = 0; i < 1000; i++)
                {
                    double xi = answer.A + i * h + h / 2; 
                    result += 3 * Math.Pow(xi, 3); 
                }

                result *= h; 

                byte[] spam = new byte[Unsafe.SizeOf<Ud>()];

                var pipa = new Ud { Result = result };
                MemoryMarshal.Write(spam, ref pipa);
                await stream.WriteAsync(spam);
            }
            catch
            {
                
            }
        }
    }
}
