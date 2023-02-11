using Newtonsoft.Json;

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
            var json = JsonConvert.SerializeObject(obj, Settings);
            Debug($"{message}, obj={json}");
        }

        public static void ErrorObj(object message, object obj) {
            var json = JsonConvert.SerializeObject(obj, Settings);
            Error($"{message}, obj={json}");
        }

        static JsonSerializerSettings Settings =>
            new JsonSerializerSettings {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Auto,
            };
    }
}