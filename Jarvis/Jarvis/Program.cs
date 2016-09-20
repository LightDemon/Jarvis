using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Globalization;
using System.Net.Http;


namespace Jarvis
{
    class JarvisProgram
    {

        static SpeechSynthesizer ss = new SpeechSynthesizer();
        static SpeechRecognitionEngine sre;
        static bool done = false;
        static bool speechOn = true;
        static string DeviceList;


        static void Main(string[] args)
        {
            try
            {

                GetDeviceList();
                

                //Set up speech
                ss.SetOutputToDefaultAudioDevice();
                Console.WriteLine("\n(Speaking: Hello sir)");
                ss.Speak("Hello sir");
                CultureInfo ci = new CultureInfo("en-us");
                sre = new SpeechRecognitionEngine(ci);
                sre.SetInputToDefaultAudioDevice();
                sre.SpeechRecognized += sre_SpeechRecognized;

                //build control grammer
                Choices ch_StartStopCommands = new Choices();
                ch_StartStopCommands.Add("speech on");
                ch_StartStopCommands.Add("speech off");
                ch_StartStopCommands.Add("klatu barada nikto");
                GrammarBuilder gb_StartStop = new GrammarBuilder();
                gb_StartStop.Append(ch_StartStopCommands);
                Grammar g_StartStop = new Grammar(gb_StartStop);

                //build status report grammer
                Choices ch_Status = new Choices();
                ch_Status.Add("status report");
                ch_Status.Add("house status");
                ch_Status.Add("system status");
                ch_Status.Add("print device list");
                GrammarBuilder gb_Info = new GrammarBuilder();
                gb_Info.Append("Jarvis");
                gb_Info.Append(ch_Status);
                Grammar g_Info = new Grammar(gb_Info);

                //build lighting grammer
                Choices ch_Light = new Choices();
                ch_Light.Add("living room light");
                ch_Light.Add("living room fan");
                ch_Light.Add("study light");
                Choices ch_Power = new Choices();
                ch_Power.Add("turn on");
                ch_Power.Add("turn off");
                GrammarBuilder gb_Light = new GrammarBuilder();
                gb_Light.Append("Jarvis");
                gb_Light.Append(ch_Power);
                gb_Light.Append("the");
                gb_Light.Append(ch_Light);
                Grammar g_Light = new Grammar(gb_Light);

                //build thermostat grammer
                Choices ch_Temp = new Choices();
                ch_Temp.Add("70");
                ch_Temp.Add("71");
                ch_Temp.Add("72");
                ch_Temp.Add("73");
                ch_Temp.Add("74");
                ch_Temp.Add("75");
                ch_Temp.Add("76");
                ch_Temp.Add("77");
                GrammarBuilder gb_Temp = new GrammarBuilder();
                gb_Temp.Append("Jarvis set the temperature to");
                gb_Temp.Append(ch_Temp);
                Grammar g_Temp = new Grammar(gb_Temp);

                GrammarBuilder gb_SetTemp = new GrammarBuilder("Jarvis what is the temperature");
                Grammar g_SetTemp = new Grammar(gb_SetTemp);

                //Load Grammer
                sre.LoadGrammarAsync(g_StartStop);
                sre.LoadGrammarAsync(g_Info);
                sre.LoadGrammarAsync(g_Light);
                sre.LoadGrammarAsync(g_Temp);
                sre.LoadGrammarAsync(g_SetTemp);
                sre.RecognizeAsync(RecognizeMode.Multiple);

                //Exit point
                while (done == false) {; }
                Console.WriteLine("\nHit <enter> to close shell\n");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }  //Main

        static void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string txt = e.Result.Text;
            float confidence = e.Result.Confidence;
            Console.WriteLine("\nRecognized: " + txt);
            if (confidence < 0.70) return;
            if (txt.IndexOf("speech on") >= 0)
            {
                Console.WriteLine("Speech is now ON");
                speechOn = true;
            }
            if (txt.IndexOf("speech off") >= 0)
            {
                Console.WriteLine("Speech is now OFF");
                speechOn = false;
            }
            if (speechOn == false) return;
            if (txt.IndexOf("klatu") >= 0 && txt.IndexOf("barada") >= 0)
            {
                ((SpeechRecognitionEngine)sender).RecognizeAsyncCancel();
                done = true;
                Console.WriteLine("(Speaking: Farewell)");
                ss.Speak("Farewell");
            }
            //if (txt.IndexOf("What") >= 0 && txt.IndexOf("plus") >= 0)
            //{
            //    string[] words = txt.Split(' ');
            //    int num1 = int.Parse(words[2]);
            //    int num2 = int.Parse(words[4]);
            //    int sum = num1 + num2;
            //    Console.WriteLine("(Speaking: " + words[2] + " plus " +
            //      words[4] + " equals " + sum + ")");
            //    ss.SpeakAsync(words[2] + " plus " + words[4] +
            //      " equals " + sum);w
            //}

            if (txt.IndexOf("status") >= 0 && txt.IndexOf("report") >= 0)
            {

                ss.SpeakAsync("status report, temporal coordinates unknown, engines offline, main power offline, auxillary power at 35%, emergency power on standby");

            }

            if (txt.IndexOf("turn on") >= 0 && txt.IndexOf("living room light") >= 0)
            {

                ss.SpeakAsync("Turning on the living room light");
                SetLightState("light_bulb","27105","true" );

            }

            if (txt.IndexOf("turn off") >= 0 && txt.IndexOf("living room light") >= 0)
            {

                ss.SpeakAsync("Turning off the living room light");
                SetLightState("light_bulb", "27105", "flase");

            }

            if (txt.IndexOf("system") >= 0 && txt.IndexOf("status") >= 0)
            {
               // DeviceList = null;
                GetDeviceList();
                ss.SpeakAsync("checking, please stand by");
                Console.Write(DeviceList);
                if (DeviceList != null)
                {
                    ss.SpeakAsync("I have a connection to the wink servers and have an updated device list");
                }
                else
                {
                    ss.SpeakAsync("I have no connection to the wink servers and have purged the device list");
                }
            }

            if (txt.IndexOf("print device list") >= 0)
            {

                Console.Write(DeviceList);

            }


        } // sre_SpeechRecognized

        static async void GetDeviceList()
        {
            try
            {

                var baseAddress = new Uri("https://private-3c4032-winkapiv2.apiary-mock.com/");

                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {

                    using (var response = await httpClient.GetAsync("users/me/wink_devices"))
                    {

                        string responseData = await response.Content.ReadAsStringAsync();
                        DeviceList = responseData;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                DeviceList = null;

            }
        } // GetDeviceList


        static async void SetLightState(string d_type, string d_id, string state )
        {
            try
            {
                var baseAddress = new Uri("https://private-3c4032-winkapiv2.apiary-mock.com/");

                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {


                    using (var content = new StringContent("{  \"desired_state\": {    \"powered\": " + state + "  }}", System.Text.Encoding.Default, "application/json"))
                    {
                        using (var response = await httpClient.PutAsync("{" + d_type + "}/{" + d_id + "}/desired_state", content))
                        {
                            string responseData = await response.Content.ReadAsStringAsync();
                            Console.Write(responseData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        } // SetLightState



    } // JarvisProgram
} // Jarvis