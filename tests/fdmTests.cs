using System;

class fdm_configTest {
    public static void TestReadXML() {
        string path = "./xmls/v2a-0.xml";
        fdm_config config = fdm_config.readXML(path);
        var coeff = fdm_config.toMetricUnit(config.mass_balance.ixx.unit);
        Console.WriteLine("value={0}", coeff * config.mass_balance.ixx.Value);
    }
}