using System;
using Newtonsoft.Json;


namespace MinimalJSim {
#nullable enable
    class Logger {

        enum LogLevel {
            Debug = 0,
            Warn = 1,
            Error = 2,
        }
        static object mutex = new object();
        static readonly string[] levels = { "Debug", "Warn", "Error" };
        static readonly ConsoleColor[] colors = { ConsoleColor.Blue, ConsoleColor.Yellow, ConsoleColor.Red };

        public static void Init() {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        static string Prefix(LogLevel l) {
            return String.Format("{0} {1} ", levels[(int)l], DateTime.Now);
        }

        static void Write(LogLevel l, string format, params object?[]? arg) {
            lock (mutex) {
                Console.ForegroundColor = colors[(int)l];
                Console.Write(Prefix(l));
                Console.ResetColor();
                Console.WriteLine(format, arg);
            }
        }

        public static void Debug(string format, params object?[]? arg) {
            Write(LogLevel.Debug, format, arg);
        }

        public static void Warn(string format, params object?[]? arg) {
            Write(LogLevel.Warn, format, arg);
        }

        public static void Error(string format, params object?[]? arg) {
            Write(LogLevel.Error, format, arg);
        }

        public static void DebugObj(string desc, object obj) {
            var json = JsonConvert.SerializeObject(obj);
            Write(LogLevel.Debug, "{0}, obj={1}", desc, json);
        }
    }
#nullable disable
}