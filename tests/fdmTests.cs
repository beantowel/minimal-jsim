using System;
using System.Numerics;

namespace MinimalJSim {
    class fdm_configTest {
        public static void TestReadXML() {
            Logger.Init();
            string path = "./xmls/737.xml";
            fdm_config config = DynamicsModel.Deserialize(path);
            DynamicsModel model = DynamicsModel.Parse(config);
            model.motion.vel = new Vector3(0, 0, 0);
            model.UpdateProperty();
            // debug log
            foreach (Property prop in model.properties.Values) {
                Logger.Debug("prop={0}, val={1}", prop.identifier, prop.Value);
            }
            foreach (var axis in model.axes) {
                Logger.Debug("axis[{0}]={1}", axis.axisDim, axis.Eval());
            }
            // assertions
            AssertEqual(model.vehicle.inertiaMatrix.M11, 12874.6768, "inertia tensor");
            AssertEqual(model.vehicle.inertiaTensor.X, 21139.348, "inertia tensor");
            AssertEqual(Units.ToMetric("psf"), 47.88, "unit");
            AssertEqual(Units.ToMetric(InertiaType.SLUGFT2), 1.3558, "unit");
            AssertEqual(Atmosphere.GeoPotentialAltitude(11000), 10980.999, "altitude");
            AssertEqual(Atmosphere.GetDensity(0), 1.225, "density");
            AssertEqual(Atmosphere.GetPressure(11019), 22632, "pressure");
            AssertEqual(Atmosphere.GetPressure(20063), 5474.9, "pressure");
            AssertEqual(Atmosphere.GetSoundSpeed(288.15f), 340.29, "sound speed");

            float[] seq = new float[]{-0.5240f,-0.4360f,-0.3490f,-0.2620f,-0.1750f,-0.0870f,0.0000f,
                0.0870f,0.1750f,0.2620f,0.3490f,0.4360f,0.5240f};
            AssertEqual(MathUtil.SearchOrdered(seq, 0), 6, "search ordered");
            AssertEqual(MathUtil.SearchOrdered(seq, -0.44f), 0, "search ordered");
            AssertEqual(MathUtil.SearchOrdered(seq, -0.4360f), 1, "search ordered");
        }

        static void AssertEqual(float got, double expect, string msg) {
            const float eps = 0.0001f;
            float delta = (float)Math.Abs(got - expect);
            if (delta > eps * (float)Math.Abs(expect)) {
                Logger.Error("assertion failed: got={0}, expect={1}, msg={2}", got, expect, msg);
            }
        }
    }
}