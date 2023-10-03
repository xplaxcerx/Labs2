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
        Console.WriteLine("Сервер: Ожидание подключения клиента...");
        using (var serverPipe = new NamedPipeServerStream("MyPipe", PipeDirection.InOut))
        {
            serverPipe.WaitForConnection();
            Console.WriteLine("Сервер: Клиент подключен.");

            CustomData dataToClient = new CustomData
            {
                Number = 42,
                Text = "Привет, клиент!"
            };

            byte[] dataToClientBytes = SerializeData(dataToClient);
            serverPipe.Write(dataToClientBytes, 0, dataToClientBytes.Length);
            Console.WriteLine("Сервер: Данные отправлены клиенту.");

            byte[] dataFromClientBytes = new byte[1024]; 
            int bytesRead = serverPipe.Read(dataFromClientBytes, 0, dataFromClientBytes.Length);
            byte[] trimmedData = new byte[bytesRead];
            Array.Copy(dataFromClientBytes, trimmedData, bytesRead);
            CustomData dataFromClient = DeserializeData(trimmedData);
            Console.WriteLine($"Сервер: Получен ответ от клиента - Number: {dataFromClient.Number}, Text: {dataFromClient.Text}");
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
