using System;
namespace MinimalJSim {
    static class Consts {
        public const float Gravity = 9.80665f;
        public const float EarthRadius = 6356766f;
        public const float RSpecific = 287.058f; // specific gas constant (J/kg*K)
        public const float GammaAir = 1.4f; // heat capacity ratio/isentropic expansion factor
        public const float SutherlandConstant = 111f;
        public const float Beta = 1.460846e-6f;
    }

    static class Units {
        public static float ToMetric(AreaType? o) {
            switch (o) {
                case AreaType.FT2:
                    return 0.092903f;
                case AreaType.M2:
                case null:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(LengthType? o) {
            switch (o) {
                case LengthType.FT:
                    return 0.30480f;
                case LengthType.IN:
                    return 0.025400f;
                case LengthType.M:
                case null:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(AngleType? o) {
            switch (o) {
                case AngleType.DEG:
                    return (float)Math.PI / 180;
                case AngleType.RAD:
                case null:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(InertiaType? o) {
            switch (o) {
                case InertiaType.SLUGFT2:
                    return 14.5939f * ToMetric(AreaType.FT2);
                case InertiaType.KGM2:
                case null:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(WeightType? o) {
            switch (o) {
                case WeightType.LBS:
                    return 0.453592f;
                case WeightType.KG:
                case null:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(SpringCoeffType? o) {
            switch (o) {
                case SpringCoeffType.LBSFT:
                    return ToMetric(WeightType.LBS) * Consts.Gravity * ToMetric(LengthType.FT);
                case SpringCoeffType.NM:
                case null:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(DampingCoeffType? o) {
            switch (o) {
                case DampingCoeffType.LBSFTSEC:
                    return ToMetric(SpringCoeffType.LBSFT);
                case DampingCoeffType.NMSEC:
                case null:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(locationUnit? o) {
            switch (o) {
                case locationUnit.FT:
                    return ToMetric(LengthType.FT);
                case locationUnit.IN:
                    return ToMetric(LengthType.IN);
                case locationUnit.M:
                case null:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(string unit) {
            switch (unit) {
                case "ft":
                    return ToMetric(LengthType.FT);
                case "sqft":
                    return ToMetric(AreaType.FT2);
                case "psf":
                    return ToMetric(WeightType.LBS) / ToMetric(AreaType.FT2) * Consts.Gravity;
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