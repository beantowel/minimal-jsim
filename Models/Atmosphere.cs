using System;
namespace MinimalJSim {
    public static class Atmosphere {
        // geopotential altitude
        static readonly float[] geoPotAltList = { 0, 11000, 20000, 32000, 47000, 51000, 71000 };
        // standard temperature (kelvin)
        static readonly float[] stdTempList = { 288.15f, 216.65f, 216.65f, 228.65f, 270.65f, 270.65f, 214.65f };
        // temperature lapse rate (kelvin/m)
        static readonly float[] lapseRateList = { -0.0065f, 0, 0.001f, 0.0028f, 0, -0.0028f, -0.002f };
        // static pressure
        static readonly float[] pressureList = { 101325, 22632.1f, 5474.89f, 868.019f, 110.906f, 66.9389f, 3.95642f };

        static float PotentialAltitude(float geometalt) =>
            (geometalt * Consts.EarthRadius) / (Consts.EarthRadius + geometalt);

        public static float Density(float pressure, float temperature) =>
            pressure / (Consts.RSpecific * temperature);

        public static float SoundSpeed(float temperature) =>
            (float)Math.Sqrt(Consts.GammaAir * Consts.RSpecific * temperature);

        public static float Viscosity(float temperature) =>
            Consts.Beta * (float)Math.Pow(temperature, 1.5) / (Consts.SutherlandConstant + temperature);

        public static float Density(float altitude) {
            var (p, t) = GetPressureTemp(altitude);
            return Density(p, t);
        }

        public static (float pressure, float temperature) GetPressureTemp(float altitude) {
            float pressure, temperature;
            float geoPotAlt = PotentialAltitude(altitude);
            int i = MathJ.SearchOrdered(geoPotAltList, geoPotAlt);
            if (i == -1 || i == -2) {
                int idx = (i == -1) ? 0 : pressureList.Length - 1;
                pressure = pressureList[idx];
                temperature = stdTempList[idx];
                return (pressure, temperature);
            }
            float baseAlt = geoPotAltList[i];
            float deltaH = geoPotAlt - baseAlt;
            float T0 = stdTempList[i];
            float P0 = pressureList[i];
            float lapseRate = lapseRateList[i];
            temperature = T0 + lapseRate * deltaH;
            if (lapseRate != 0.0f) {
                float exp = Consts.Gravity / (Consts.RSpecific * lapseRate);
                float factor = T0 / temperature;
                pressure = P0 * (float)Math.Pow(factor, exp);
            } else {
                pressure = P0 * (float)Math.Exp(-Consts.Gravity * deltaH / (Consts.RSpecific * T0));
            }
            return (pressure, temperature);
        }

    }
}
