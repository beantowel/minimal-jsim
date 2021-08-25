using System;
namespace MinimalJSim {
    class Units {
        public const float gravity = 9.80665f;
        public const float earthRadius = 6356766;
        public const float rSpecific = 287.058f; // specific gas constant (J/kg*K)
        public const float gammaAir = 1.4f; // heat capacity ratio/isentropic expansion factor

        public static float ToMetric(AreaType o) {
            switch (o) {
                case AreaType.FT2:
                    return 0.092903f;
                case AreaType.M2:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(LengthType o) {
            switch (o) {
                case LengthType.FT:
                    return 0.30480f;
                case LengthType.IN:
                    return 0.025400f;
                case LengthType.M:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(AngleType o) {
            switch (o) {
                case AngleType.DEG:
                    return (float)Math.PI / 180;
                case AngleType.RAD:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(InertiaType o) {
            switch (o) {
                case InertiaType.SLUGFT2:
                    return 14.5939f * ToMetric(AreaType.FT2);
                case InertiaType.KGM2:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(WeightType o) {
            switch (o) {
                case WeightType.LBS:
                    return 0.453592f;
                case WeightType.KG:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(SpringCoeffType o) {
            switch (o) {
                case SpringCoeffType.LBSFT:
                    return ToMetric(WeightType.LBS) * gravity * ToMetric(LengthType.FT);
                case SpringCoeffType.NM:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(DampingCoeffType o) {
            switch (o) {
                case DampingCoeffType.LBSFTSEC:
                    return ToMetric(SpringCoeffType.LBSFT);
                case DampingCoeffType.NMSEC:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(locationUnit o) {
            switch (o) {
                case locationUnit.FT:
                    return ToMetric(LengthType.FT);
                case locationUnit.IN:
                    return ToMetric(LengthType.IN);
                case locationUnit.M:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(Property p) {
            string unit = p.Unit();
            if (unit.Length == 0) {
                Logger.Warn("no unit, property={0}", p.identifier);
            }
            return ToMetric(unit);
        }

        public static float ToMetric(string unit) {
            switch (unit) {
                case "ft":
                    return ToMetric(LengthType.FT);
                case "sqft":
                    return ToMetric(AreaType.FT2);
                case "psf":
                    return ToMetric(WeightType.LBS) / ToMetric(AreaType.FT2) * gravity;
                case "slugs_ft3":
                    return ToMetric(InertiaType.SLUGFT2) / ToMetric(LengthType.FT);
                case "rad":
                case "norm":
                case "rad-sec":
                case "1?": // metric special default unit
                default:
                    return 1;
            }
        }
    }
}