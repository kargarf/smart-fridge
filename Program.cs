#define CreatingJson
#define SendingToSparkfun

using Microsoft.SPOT.Hardware;
using System.Net;
using System;
using Microsoft.SPOT;
using System.Text;
using System.IO;
using System.Threading;
using SmartRefrigerator.Json;
using SecretLabs.NETMF.Hardware;

using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace SmartRefrigerator
{
    public class Program
    {
        

        // Variables
        static int fridgeId = 2;
        static double[] weights = new double[4];

        #region Ports
        // Pilot LED
        static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

        // Buttons
        static InterruptPort btnTest = new InterruptPort(Pins.ONBOARD_SW1, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);
        static InterruptPort btnDoor = new InterruptPort(Pins.GPIO_PIN_D0, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);

        // Egg Sensors
        static InputPort eggReader0 = new InputPort(Pins.GPIO_PIN_D1, true, Port.ResistorMode.PullDown);
        static InputPort eggReader1 = new InputPort(Pins.GPIO_PIN_D2, true, Port.ResistorMode.PullDown);
        static InputPort eggReader2 = new InputPort(Pins.GPIO_PIN_D3, true, Port.ResistorMode.PullDown);
        static InputPort eggReader3 = new InputPort(Pins.GPIO_PIN_D4, true, Port.ResistorMode.PullDown);
        static InputPort eggReader4 = new InputPort(Pins.GPIO_PIN_D5, true, Port.ResistorMode.PullDown);
        static InputPort eggReader5 = new InputPort(Pins.GPIO_PIN_D6, true, Port.ResistorMode.PullDown);

        // Weight Sensors
        static AnalogInput weightReader0 = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
        static AnalogInput weightReader1 = new AnalogInput(Cpu.AnalogChannel.ANALOG_1);

        //temperature sensors
        //static AnalogInput weightReader2 = new AnalogInput(Cpu.AnalogChannel.ANALOG_2);
        //static AnalogInput weightReader3 = new AnalogInput(Cpu.AnalogChannel.ANALOG_3);

        #endregion

        // Cloud Connection
        //static Sender sender = new Sender("http://coolfridge.azurewebsites.net/Receiver.svc/UpdateData");
        //static Receiver receiver = new Receiver("coolfridge.azurewebsites.net", 80);

        //========================================================================


        public static void Main()
        {


            // Turn on the LED
            led.Write(true);

            // Generate Button Interrupts
            btnTest.OnInterrupt += btnTest_OnInterrupt;
            btnDoor.OnInterrupt += btnDoor_OnInterrupt;

            //Debug.Print(received);
            Debug.Print("CoolFridge is ready!");

            // Send initial report
            SendFridgeReport();

            // Turn off the LED
            led.Write(false);

            // Sleep the thread          
            Thread.Sleep(Timeout.Infinite);
        }

        static void SendFridgeReport()
        {
            // Generate egg positions
            string eggPositions = ((eggReader0.Read() == true) ? "1" : "0") + ","
                + ((eggReader1.Read() == true) ? "1" : "0") + ","
                + ((eggReader2.Read() == true) ? "1" : "0") + ","
                + ((eggReader3.Read() == true) ? "1" : "0") + ","
                + ((eggReader4.Read() == true) ? "1" : "0") + ","
                + ((eggReader5.Read() == true) ? "1" : "0");

            // Read weights
            //double weightReader0 = 0.745;
            //double weightReader1 = 0.00000;

            weights[0] = CalculateWeight(weightReader0.Read());
            weights[1] = CalculateWeight(weightReader1.Read());


#if CreatingJson

            JsonObject jo = new JsonObject();
           
            jo.Add("eggs", eggPositions);
            jo.Add("fridgeid", fridgeId.ToString());
            jo.Add("slot1weight", weights[0].ToString());
            jo.Add("slot2weight", weights[1].ToString());

            Debug.Print(jo.ToString());

#endif

#if SendingToSparkfun
            // https://data.sparkfun.com/input/*********
            try
            {
                byte[] postData = Encoding.UTF8.GetBytes(jo.ToString());

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"http://data.sparkfun.com/input/********");
                request.Method = "POST";
                request.Headers.Add("Phant-Private-Key", "xxxxxxxxxxxxxx");
                request.ContentType = "application/json";
                request.ContentLength = postData.Length;
                request.KeepAlive = false;

                Debug.Print("request set");

                Stream postDataStream = request.GetRequestStream();
                postDataStream.Write(postData, 0, postData.Length);
                postDataStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                request.Dispose();
                Debug.Print(response.StatusCode.ToString());

                Debug.Print("response done");
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message.ToString());
            }
#endif
        }

        static void btnDoor_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            SendFridgeReport();
        }

        static void btnTest_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            SendFridgeReport();
        }

        public static double CalculateWeight(double analogInput)
        {
            double result = (analogInput - 0.055) * (1 / 0.037);

            return result;
        }

      }
}
