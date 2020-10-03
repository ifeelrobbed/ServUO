//UO Black Box - By GoldDraco13

using System;
using System.IO;

namespace Server.UOBlackBox
{
    public static class BBServerMessage
    {
        private static readonly string SaveLogDir = Directory.GetCurrentDirectory() + "\\UOBlackBox\\LOGS\\";

        public static void WriteConsoleColored(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void LogPacketCMD(string user, string command, bool IsError)
        {
            int time = DateTime.Today.Day;

            try
            {
                if (!IsError)
                {
                    if (!Directory.Exists(SaveLogDir + user))
                        Directory.CreateDirectory(SaveLogDir + user);

                    using (StreamWriter sw = new StreamWriter(SaveLogDir + user + "\\" + "Day_" + time + ".txt", true))
                    {
                        sw.WriteLine(command);
                    }
                }
                else
                {
                    if (!Directory.Exists(SaveLogDir + "ERROR"))
                        Directory.CreateDirectory(SaveLogDir + "ERROR");

                    using (StreamWriter sw = new StreamWriter(SaveLogDir + "ERROR\\" + "Day_" + time + ".txt", true))
                    {
                        sw.WriteLine(command);
                    }
                }
            }
            catch
            {
                Console.WriteLine("[Report] => There was a problem with log file!");
            }
        }
    }
}
