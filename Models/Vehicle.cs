using System.Numerics;
namespace MinimalJSim {
    public class Vehicle {
        // metrics
        public Property WingArea, WingSpan, WingIncidence, WingChord;
        public Property HTailArea, HTailArm, VTailArea, VTailArm;
        public Location[] locations;

        // inertia
        public Property emptyWeight;
        public Vector3 inertiaTensor;
        public Quaternion massFrame;
        public Vector3 centerOfMass;

        public Vehicle() { }
        public Vehicle(DModel model, metrics cm, mass_balance m) {
            WingArea = model.GetProperty("metrics/Sw-1?");
            WingSpan = model.GetProperty("metrics/bw-1?");
            WingIncidence = model.GetProperty("metrics/iw-deg");
            WingChord = model.GetProperty("metrics/cbarw-1?");
            HTailArea = model.GetProperty("metrics/Sh-1?");
            HTailArm = model.GetProperty("metrics/lh-1?");
            VTailArea = model.GetProperty("metrics/Sv-1?");
            VTailArm = model.GetProperty("metrics/lv-1?");
            emptyWeight = model.GetProperty("inertia/empty-weight-1?");
            WingArea.Val = (float)(cm.wingarea?.Value ?? 0) * Units.ToMetric(cm.wingarea?.unit);
            WingSpan.Val = (float)(cm.wingspan?.Value ?? 0) * Units.ToMetric(cm.wingspan?.unit);
            WingIncidence.Val = (float)(cm.wing_incidence?.Value ?? 0) * Units.ToMetric(cm.wing_incidence?.unit);
            WingChord.Val = (float)(cm.chord?.Value ?? 0) * Units.ToMetric(cm.chord?.unit);
            HTailArea.Val = (float)(cm.htailarea?.Value ?? 0) * Units.ToMetric(cm.htailarea?.unit);
            HTailArm.Val = (float)(cm.htailarm?.Value ?? 0) * Units.ToMetric(cm.htailarm?.unit);
            VTailArea.Val = (float)(cm.vtailarea?.Value ?? 0) * Units.ToMetric(cm.vtailarea?.unit);
            VTailArm.Val = (float)(cm.vtailarm?.Value ?? 0) * Units.ToMetric(cm.vtailarm?.unit);
            emptyWeight.Val = (float)m.emptywt.Value * Units.ToMetric(m.emptywt.unit);

            locations = new Location[cm.location.Length];
            for (int i = 0; i < cm.location.Length; i++) {
                location loc = cm.location[i];
                locations[i] = new Location(loc.name, loc.x, loc.y, loc.z);
                locations[i].Scale(Units.ToMetric(loc.unit));
            }

            m.ixy ??= new ixy();
            m.iyz ??= new iyz();
            m.ixz ??= new ixz();
            Matrix3x3 inertiaMatrix = (m.negated_crossproduct_inertia) ?
                new Matrix3x3(
                (float)m.ixx.Value, -(float)m.ixy.Value, (float)m.ixz.Value,
                -(float)m.ixy.Value, (float)m.iyy.Value, -(float)m.iyz.Value,
                (float)m.ixz.Value, -(float)m.iyz.Value, (float)m.izz.Value) :
                new Matrix3x3(
                (float)m.ixx.Value, (float)m.ixy.Value, -(float)m.ixz.Value,
                (float)m.ixy.Value, (float)m.iyy.Value, (float)m.iyz.Value,
                -(float)m.ixz.Value, (float)m.iyz.Value, (float)m.izz.Value);
            inertiaMatrix *= Units.ToMetric(m.ixx.unit);
            centerOfMass = new Vector3((float)m.location.x, (float)m.location.y, (float)m.location.z) *
                Units.ToMetric(m.location.unit);
            (inertiaTensor, massFrame) = MathJ.Diagonalize(inertiaMatrix);
        }
    }

    public class Location {
        public string name;
        public double locX, locY, locZ;
        public Location(string name, double x, double y, double z) {
            this.name = name;
            locX = x;
            locY = y;
            locZ = z;
        }

        public void Scale(float x) {
            locX *= x;
            locY *= x;
            locZ *= x;
        }
    }
}