using Newtonsoft.Json;
using UnityEngine;

namespace MinimalJSim {
    class Logger {
        public static void Debug(object message) {
            UnityEngine.Debug.Log(message);
        }

        public static void Warn(object message) {
            UnityEngine.Debug.LogWarning(message);
        }

        public static void Error(object message) {
            UnityEngine.Debug.LogError(message);
        }

        public static void DebugObj(object message, object obj) {
            var json = JsonConvert.SerializeObject(obj);
            Debug($"{message}, obj={json}");
        }

        public static void ErrorObj(object message, object obj) {
            var json = JsonConvert.SerializeObject(obj);
            Error($"{message}, obj={json}");
        }
    }
}