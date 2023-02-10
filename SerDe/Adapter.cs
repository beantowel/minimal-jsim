using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace MinimalJSim {
    public static class Adapter {
        public static void DeSerXML<T>(TextReader reader, out T v) {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            v = (T)serializer.Deserialize(reader);
            reader.Close();
        }

        public static void ExpandInclude(fdm_config conf, string rootPath) {
            for (int i = 0; i < conf.propulsion.Items.Length; i++) {
                var item = conf.propulsion.Items[i];
                if (item is engine e) {
                    turbine_engine te;
                    DeSerXML(ReadFile(rootPath, "Engines", e.file), out te);
                    e.Item = te;
                    conf.propulsion.Items[i] = e;
                }
            }
            for (int i = 0; i < (conf.system?.Length ?? 0); i++) {
                var sys = conf.system[i];
                if (sys.file != null) {
                    DeSerXML(ReadFile(rootPath, "Systems", sys.file), out conf.system[i]);
                }
            }
            if (conf.aerodynamics.file != null) {
                aerodynamics a;
                DeSerXML(ReadFile(rootPath, conf.aerodynamics.file), out a);
                conf.aerodynamics = a;
            }
        }

        static TextReader ReadFile(params string[] paths) {
            var path = Path.Combine(paths);
            path += path.EndsWith(".xml") ? "" : ".xml";
            Logger.Debug($"read path={path}");
            return new StreamReader(path);
        }
    }
}