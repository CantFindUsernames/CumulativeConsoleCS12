using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace CulmativeConsoleCS12
{
    internal class Program
    {
        private static DeviceClient deviceClient;
        public static void Main(string[] args)
        {
            string connectionString = "HostName=WCHS.azure-devices.net;DeviceId=wchs6;SharedAccessKey=5qXtc9bX4D66hoqSviZVXEMhoK/SVPCvOAIoTAVQHns=";
            try
            {
                SerialPort port = new SerialPort{BaudRate = 9600, PortName = "COM3"};
                port.Open();
                try
                {
                    while (true)
                    {
                        string oneLine = port.ReadLine();
                        System.Threading.Thread.Sleep(500);
                        deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
                        SendMsg(oneLine);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Encountered error while reading serial port");
                    Console.WriteLine(ex.ToString());
                    System.Threading.Thread.Sleep(50000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encountered error while opening serial port ");
                Console.WriteLine(ex.ToString());
                System.Threading.Thread.Sleep(50000);
            }
        }
        public async static void SendMsg(string msg)
        {
            string jsonData = JsonConvert.SerializeObject(msg);
            Message message = new Message(Encoding.ASCII.GetBytes(jsonData));
            await deviceClient.SendEventAsync(message);
            Console.WriteLine("{0:G} > Sent msg: {1}", DateTime.Now, msg);
            await Task.Delay(3000);
        }
    }
}
