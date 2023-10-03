using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;

public struct CustomData
{
    public int Number;
    public string Text;
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Клиент: Подключение к серверу...");
        using (var clientPipe = new NamedPipeClientStream(".", "MyPipe", PipeDirection.InOut))
        {
            clientPipe.Connect();
            Console.WriteLine("Клиент: Подключено к серверу.");

            byte[] dataFromServerBytes = new byte[1024]; 
            int bytesRead = clientPipe.Read(dataFromServerBytes, 0, dataFromServerBytes.Length);
            byte[] trimmedData = new byte[bytesRead];
            Array.Copy(dataFromServerBytes, trimmedData, bytesRead);
            CustomData dataFromServer = DeserializeData(trimmedData);
            Console.WriteLine($"Клиент: Получены данные от сервера - Number: {dataFromServer.Number}, Text: {dataFromServer.Text}");

            CustomData dataToServer = new CustomData
            {
                Number = 123,
                Text = "Привет, сервер!"
            };

            byte[] dataToServerBytes = SerializeData(dataToServer);
            clientPipe.Write(dataToServerBytes, 0, dataToServerBytes.Length);
            Console.WriteLine("Клиент: Ответ отправлен серверу.");
        }
    }

    static byte[] SerializeData(CustomData data)
    {
        string serializedText = $"{data.Number},{data.Text}";
        return System.Text.Encoding.UTF8.GetBytes(serializedText);
    }

    static CustomData DeserializeData(byte[] buffer)
    {
        string serializedText = System.Text.Encoding.UTF8.GetString(buffer);
        string[] parts = serializedText.Split(',');
        CustomData data = new CustomData
        {
            Number = int.Parse(parts[0]),
            Text = parts[1]
        };
        return data;
    }
}
