using System;
namespace MinimalJSim {
    public class Atmosphere {
        // geopotential altitude
        static readonly float[] geoPotAltList = { 0, 11000, 20000, 32000, 47000, 51000, 71000 };
        // standard temperature (kelvin)
        static readonly float[] stdTempList = { 288.15f, 216.65f, 216.65f, 228.65f, 270.65f, 270.65f, 214.65f };
        // temperature lapse rate (kelvin/m)
        static readonly float[] lapseRateList = { -0.0065f, 0, 0.001f, 0.0028f, 0, -0.0028f, -0.002f };
        // static pressure
        static readonly float[] pressureList = { 101325, 22632.1f, 5474.89f, 868.019f, 110.906f, 66.9389f, 3.95642f };

        public static float GeoPotentialAltitude(float geometalt) {
            return (geometalt * Units.earthRadius) / (Units.earthRadius + geometalt);
        }

        public static float GetPressure(float altitude) {
            var (p, _, _) = GetPressureDensityTemp(altitude);
            return p;
        }

        public static float GetDensity(float altitude) {
            var (_, d, _) = GetPressureDensityTemp(altitude);
            return d;
        }

        public static float GetTemperature(float altitude) {
            var (_, _, t) = GetPressureDensityTemp(altitude);
            return t;
        }

        public static (float, float, float) GetPressureDensityTemp(float altitude) {
            float pressure, density, temperature;
            float geoPotAlt = GeoPotentialAltitude(altitude);
            int i = MathUtil.SearchOrdered(geoPotAltList, geoPotAlt);
            if (i == -1 || i == -2) {
                int idx = (i == -1) ? 0 : pressureList.Length - 1;
                pressure = pressureList[idx];
                temperature = stdTempList[idx];
                density = pressure / (Units.rSpecific * temperature);
                return (pressure, density, temperature);
            }
            float baseAlt = geoPotAltList[i];
            float deltaH = geoPotAlt - baseAlt;
            float T0 = stdTempList[i];
            float P0 = pressureList[i];
            float lapseRate = lapseRateList[i];
            temperature = T0 + lapseRate * deltaH;
            if (lapseRate != 0.0f) {
                float exp = Units.gravity / (Units.rSpecific * lapseRate);
                float factor = T0 / temperature;
                pressure = P0 * (float)Math.Pow(factor, exp);
            } else {
                pressure = P0 * (float)Math.Exp(-Units.gravity * deltaH / (Units.rSpecific * T0));
            }
            density = pressure / (Units.rSpecific * temperature);
            return (pressure, density, temperature);
        }

        public static float GetSoundSpeed(float temperature) {
            return (float)Math.Sqrt(Units.gammaAir * Units.rSpecific * temperature);
        }
    }
}
