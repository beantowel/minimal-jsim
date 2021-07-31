using System;

namespace MinimalJSim {
    class fdm_configTest {
        public static void TestReadXML() {
            Logger.Init();
            string path = "./xmls/f16.xml";
            fdm_config config = Adapter.Deserialize(path);
            Adapter.Parse(config);
        }
    }
}