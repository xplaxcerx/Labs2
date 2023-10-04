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
                formatter.Serialize(clientPipe, requestData);
                Console.WriteLine("Отправлено серверу.");

                
                MyData response = (MyData)formatter.Deserialize(clientPipe);
                Console.WriteLine($"Получено от сервера: Number = {response.Number}, Message = {response.Message}");

                
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
