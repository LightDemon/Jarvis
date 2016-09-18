using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Globalization;

namespace Jarvis
{
    class JarvisProgram
    {

        static SpeechSynthesizer ss = new SpeechSynthesizer();
        static SpeechRecognitionEngine sre;
        static bool done = false;
        static bool speechOn = true;


        static void Main(string[] args)
        {
            try
            {
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
                Choices ch_status = new Choices();
                ch_status.Add("status report");
                ch_status.Add("house status");
                ch_status.Add("system status");
                GrammarBuilder gb_Info = new GrammarBuilder();
                gb_Info.Append("Jarvis");
                gb_Info.Append(ch_status);
                Grammar g_Info = new Grammar(gb_Info);


                Choices ch_Numbers = new Choices();
                ch_Numbers.Add("1");
                ch_Numbers.Add("2");
                ch_Numbers.Add("3");
                ch_Numbers.Add("4");
                GrammarBuilder gb_WhatIsXplusY = new GrammarBuilder();
                gb_WhatIsXplusY.Append("What is");
                gb_WhatIsXplusY.Append(ch_Numbers);
                gb_WhatIsXplusY.Append("plus");
                gb_WhatIsXplusY.Append(ch_Numbers);
                Grammar g_WhatIsXplusY = new Grammar(gb_WhatIsXplusY);

                //Load Grammer
                sre.LoadGrammarAsync(g_StartStop);
                sre.LoadGrammarAsync(g_WhatIsXplusY);
                sre.LoadGrammarAsync(g_Info);
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
            if (txt.IndexOf("What") >= 0 && txt.IndexOf("plus") >= 0)
            {
                string[] words = txt.Split(' ');
                int num1 = int.Parse(words[2]);
                int num2 = int.Parse(words[4]);
                int sum = num1 + num2;
                Console.WriteLine("(Speaking: " + words[2] + " plus " +
                  words[4] + " equals " + sum + ")");
                ss.SpeakAsync(words[2] + " plus " + words[4] +
                  " equals " + sum);
            }

            if (txt.IndexOf("report") >= 0)
            {

                ss.SpeakAsync("status report, temporal coordinates unknown, engines offline, main power offline, auxillary power at 35%, emergency power on standby");

            }



        } // sre_SpeechRecognized


    } // JarvisProgram
} // Jarvis