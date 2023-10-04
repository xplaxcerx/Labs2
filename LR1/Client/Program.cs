using System;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;

class Program
{
    static void Main()
    {
        using (var clientPipe = new NamedPipeClientStream(".", "MyPipe", PipeDirection.InOut))
        {
            Console.WriteLine("Подключение к серверу...");
            clientPipe.Connect();

            var formatter = new BinaryFormatter();

            while (true)
            {
                MyData requestData = new MyData { Number = 10, Message = "Запрос от клиента" };
                var memoryStream = new System.IO.MemoryStream();
                formatter.Serialize(memoryStream, requestData);
                clientPipe.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                Console.WriteLine("Отправлено серверу.");

                byte[] buffer = new byte[1024];
                int bytesRead = clientPipe.Read(buffer, 0, buffer.Length);
                memoryStream = new System.IO.MemoryStream(buffer, 0, bytesRead);
                MyData response = (MyData)formatter.Deserialize(memoryStream);
                Console.WriteLine($"Получено от сервера: Number = {response.Number}, Message = {response.Message}");

                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}

