﻿using System;
using System.IO.Pipes;
using System.Runtime.CompilerServices;

public struct Structure
{
    public double a;
    public double b;
    public double result;
}

class Client
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            using (NamedPipeClientStream Client = new(".", args[0], PipeDirection.InOut))
            {
                try
                {
                    Client.Connect();

                    byte[] bytes = new byte[Unsafe.SizeOf<Structure>()];
                    Client.Read(bytes, 0, bytes.Length);
                    Structure receivedData = Unsafe.As<byte, Structure>(ref bytes[0]);
                    Console.WriteLine($"Received data: a = {receivedData.a}, b = {receivedData.b}");

                    int n = 1000;
                    receivedData.result = TrapezoidalRule(receivedData.a, receivedData.b, n);

                    Console.WriteLine($"Result calculated by client: {receivedData.result}");

                    byte[] modifiedBytes = new byte[Unsafe.SizeOf<Structure>()];
                    Unsafe.As<byte, Structure>(ref modifiedBytes[0]) = receivedData;
                    Client.Write(modifiedBytes, 0, modifiedBytes.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }

    static double Function(double x)
    {
        const double coefficient = 2;
        return coefficient * x * x;
    }

    static double TrapezoidalRule(double a, double b, int n)
    {
        double h = (b - a) / Convert.ToDouble(n);
        double result = 0.5 * (Function(a) + Function(b));

        for (int i = 1; i < n; i++)
        {
            double x = a + i * h;
            result += Function(x);
        }

        result *= h;
        Console.WriteLine($"Result calculated by trapezoidal rule: {result}");
        return result;
    }
}
