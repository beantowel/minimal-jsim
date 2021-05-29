using System;
using UnityEngine;
using System.Collections.Generic;

public partial class fdm_config {
    public static fdm_config readXML(string path) {
        System.Xml.Serialization.XmlSerializer reader =
            new System.Xml.Serialization.XmlSerializer(typeof(fdm_config));
        System.IO.StreamReader file = new System.IO.StreamReader(path);
        fdm_config config = (fdm_config)reader.Deserialize(file);
        file.Close();
        return config;
    }

    public static double toMetricUnit(AreaType o) {
        switch (o) {
            case AreaType.FT2:
                return 1 / 10.764;
            case AreaType.M2:
                return 1;
        }
        return 1;
    }

    public static double toMetricUnit(LengthType o) {
        switch (o) {
            case LengthType.FT:
                return 1 / 3.2808;
            case LengthType.IN:
                return 1 / 39.370;
            case LengthType.M:
                return 1;
        }
        return 1;
    }
    public static double toMetricUnit(AngleType o) {
        switch (o) {
            case AngleType.DEG:
                return 1 / 57.296;
            case AngleType.RAD:
                return 1;
        }
        return 1;
    }
    public static double toMetricUnit(InertiaType o) {
        switch (o) {
            case InertiaType.SLUGFT2:
                return 1.3558179619;
            case InertiaType.KGM2:
                return 1;
        }
        return 1;
    }
    public static double toMetricUnit(WeightType o) {
        switch (o) {
            case WeightType.LBS:
                return 1 / 2.2046;
            case WeightType.KG:
                return 1;
        }
        return 1;
    }
    public static double toMetricUnit(SpringCoeffType o) {
        switch (o) {
            case SpringCoeffType.LBSFT:
                return 1 / 0.73756;
            case SpringCoeffType.NM:
                return 1;
        }
        return 1;
    }
    public static double toMetricUnit(DampingCoeffType o) {
        switch (o) {
            case DampingCoeffType.LBSFTSEC:
                return 1 / 0.73756;
            case DampingCoeffType.NMSEC:
                return 1;
        }
        return 1;
    }

    // public struct UnityInertiaVector {
    //     Vector3 inertia;
    // }

    // Evd<float> Evd = inertia.Unity3DCoordTrafo().ToMatrix().Evd(Symmetricity.Symmetric);
    // rigidbody.inertiaTensor = Evd.EigenValues.GetReal().ToVector3().FixMinInertia(); // optionally check vector for imaginary part = 0
    // rigidbody.inertiaTensorRotation = Evd.EigenVectors.ToQuaternion(); // optionally check matrix for determinant = 1

    public static void parseXML(fdm_config doc) {
        var model = new fdm_model();
    }
}

public class fdm_model {
    public Dictionary<string, double> properties;
}