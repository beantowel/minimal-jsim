using UnityEngine;

namespace MinimalJSim {
    public class ScreenPanel : MonoBehaviour {
        private FDM fdm;
        private UnityEngine.UI.Text text;

        private void Awake() {
            fdm = GameObject.Find("player").GetComponent<FDM>();

            text = transform.GetComponent<UnityEngine.UI.Text>();
        }



        private void Update() {
            if (Time.frameCount % 10 != 0 || fdm == null || text == null) {
                return;
            }

            float throttle = fdm.controller.axes[(int)Controller.AxisChannel.Throttle].value;
            float pitch = fdm.controller.axes[(int)Controller.AxisChannel.Pitch].value;
            float roll = fdm.controller.axes[(int)Controller.AxisChannel.Roll].value;
            float yaw = fdm.controller.axes[(int)Controller.AxisChannel.Yaw].value;
            Vector3 control = new Vector3(pitch, roll, yaw);

            text.text = $@"TAS: {fdm.Rb.velocity.magnitude * 3.6:F0}
ALT: {fdm.model.motion.alt.Val:F0}
HDG: {fdm.Rb.rotation.eulerAngles.y:F0}
mach: {fdm.model.aero.mach.Val:F2}
Throttle: {throttle * 100:F0}%
Origin: {Overlook.OriginShifter.Get().WorldOrigin.Length}
";
        }

        public static string FormatVector(UnityEngine.Vector3 v) {
            return $"({v.x,6:+0.00E+00;-0.00E+00},{v.y,6:+0.00E+00;-0.00E+00},{v.z,6:+0.00E+00;-0.00E+00})";
        }

        public static string FormatVector(System.Numerics.Vector3 s) {
            return FormatVector(FDM.UVec3(s));
        }
    }
}