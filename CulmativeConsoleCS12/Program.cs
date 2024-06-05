using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Framing;
using System.Web.UI.WebControls;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Azure.Messaging.EventHubs.Consumer;

namespace CulmativeConsoleCS12
{
    internal class Program
    {
        private static DeviceClient deviceClient;
        private readonly static string hubConnectionString = "Endpoint=sb://iothub-ns-wchs-58009161-d596108607.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=glM1A7WyVpMhjjMrhLcdQjkIr91upa8tSAIoTFiDvUI=;EntityPath=wchs";
        private readonly static string EventHubName = "wchs";
        public static void Main(string[] args)
        {
            string connectionString = "HostName=WCHS.azure-devices.net;DeviceId=wchs6;SharedAccessKey=5qXtc9bX4D66hoqSviZVXEMhoK/SVPCvOAIoTAVQHns=";
            try
            {
                SerialPort port = new SerialPort{BaudRate = 9600, PortName = "COM6"};
                port.Open();
                ReceiveMessagesFromDeviceAsync(hubConnectionString, EventHubName, port);
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
                    System.Threading.Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encountered error while opening serial port ");
                Console.WriteLine(ex.ToString());
                System.Threading.Thread.Sleep(500);
            }
        }
        public async static void SendMsg(string msg) // Sends data to the hub
        {
            string jsonData = JsonConvert.SerializeObject(msg);
            Message message = new Message(Encoding.ASCII.GetBytes(jsonData));
            Console.WriteLine("{0:G} > Sent msg: {1}", DateTime.Now, msg);
            try
            {
                await deviceClient.SendEventAsync(message);
            }
            catch (Exception e) // Stops it from crashing after 8000 messages
            {
                Console.WriteLine($"{e.Message}");
                
            }
            await Task.Delay(1000);
        }
        public static void UpdateMinAndMax(SerialPort port, string message) // Sends commands to arduino
        {
            port.WriteLine(message);
            port.WriteLine(message);
            port.WriteLine(message);
        }
        private async static Task ReceiveMessagesFromDeviceAsync(string hubConnectionString, string EventHubName, SerialPort port) // Recieve commands from remote location
        {
            await using EventHubConsumerClient consumer = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, hubConnectionString, EventHubName);
            await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync())
            {
                partitionEvent.Data.SystemProperties.TryGetValue("iothub-connection-device-id", out object deviceID);  // read event message from Event Hub partition 
                if (deviceID.ToString() == "wchs5")
                {
                    string datum = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                    if (datum != null)
                    {
                        UpdateMinAndMax(port, datum.Substring(2, datum.Length - 3));
                    }
                }
            }
        }

    }
}
