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
                
                MyData receivedData = (MyData)formatter.Deserialize(serverPipe);
                Console.WriteLine($"Получено от клиента: Number = {receivedData.Number}, Message = {receivedData.Message}");

                MyData responseData = new MyData { Number = 42, Message = "Ответ от сервера" };
                formatter.Serialize(serverPipe, responseData);
                Console.WriteLine("Отправлено клиенту.");
            }
        }
    }
}
