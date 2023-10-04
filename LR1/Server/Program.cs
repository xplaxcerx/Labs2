using System;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;

class Program
{
    static void Main()
    {
        using (var serverPipe = new NamedPipeServerStream("MyPipe", PipeDirection.InOut))
        {
            Console.WriteLine("Сервер ожидает подключения...");
            serverPipe.WaitForConnection();

            var formatter = new BinaryFormatter();

            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = serverPipe.Read(buffer, 0, buffer.Length);
                var memoryStream = new System.IO.MemoryStream(buffer, 0, bytesRead);
                MyData receivedData = (MyData)formatter.Deserialize(memoryStream);
                Console.WriteLine($"Получено от клиента: Number = {receivedData.Number}, Message = {receivedData.Message}");

                MyData responseData = new MyData { Number = 42, Message = "Ответ от сервера" };
                memoryStream = new System.IO.MemoryStream();
                formatter.Serialize(memoryStream, responseData);
                serverPipe.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                Console.WriteLine("Отправлено клиенту.");
            }
        }
    }
}
