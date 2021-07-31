namespace MinimalJSim {
    class Units {
        public static float gravity = 9.80665f;

        public static float ToMetric(AreaType o) {
            switch (o) {
                case AreaType.FT2:
                    return 0.025400f;
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
                    return 0.01745f;
                case AngleType.RAD:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(InertiaType o) {
            switch (o) {
                case InertiaType.SLUGFT2:
                    return 14.594f / 0.30480f / 0.30480f;
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
                    return 0.453592f * 0.30480f;
                case SpringCoeffType.NM:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(DampingCoeffType o) {
            switch (o) {
                case DampingCoeffType.LBSFTSEC:
                    return 0.453592f * 0.30480f;
                case DampingCoeffType.NMSEC:
                    return 1;
            }
            return 1;
        }

        public static float ToMetric(locationUnit o) {
            switch (o) {
                case locationUnit.FT:
                    return 0.30480f;
                case locationUnit.IN:
                    return 0.025400f;
                case locationUnit.M:
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
                    return ToMetric(WeightType.LBS) / ToMetric(AreaType.FT2) * gravity;
                case "slugs_ft3":
                    return ToMetric(InertiaType.SLUGFT2) / ToMetric(LengthType.FT);
                case "rad":
                case "norm":
                case "rad-sec":
                case "1?": // metric special default unit
                    return 1;
                default:
                    Logger.Warn("unknown unit, name={0}", unit);
                    return 1;
            }
        }
    }
}