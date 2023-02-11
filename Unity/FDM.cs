using System.IO;
using FlightDisplay;
using UnityEngine;

namespace MinimalJSim {
    public class FDM : MonoBehaviour {
        public TextAsset ConfigXML;
        public TextAsset FDMData;

        public Rigidbody Rb { get; private set; }
        public DynamicsModel model;
        public Controller controller;
        public Vector3 force, torque;
        System.Numerics.Vector3 frc, trq;
        HUDController hud;
        Overlook.OriginShifter shifter;
        Overlook.GeoClipMap geo;

        // Start is called before the first frame update
        void Start() {
            Init();
            geo = Overlook.GeoClipMap.Get();
            shifter = Overlook.OriginShifter.Get(transform);
            Overlook.OriginShifter.SetViewer(transform);
        }

#if UNITY_EDITOR
        [ContextMenu("GenFDM")]
        void ConvertData() {
            Logger.Debug($"ConfigXML={ConfigXML.name}");
            fdm_config conf;
            Adapter.DeSerXML(new StringReader(ConfigXML.text), out conf);
            string path = UnityEditor.AssetDatabase.GetAssetPath(ConfigXML);
            Adapter.ExpandInclude(conf, Path.GetDirectoryName(path));
            model = Parser.Parse(conf);
            Adapter.Export(model, path);
        }
#endif

        void Init() {
            model = Adapter.LoadJSON(FDMData.text);
            model.BuildLUT();
            Rb = transform.GetComponent<Rigidbody>();
            Rb.mass = model.vehicle.emptyWeight.Val;
            Rb.centerOfMass = Vector3.zero;
            Rb.inertiaTensor = UVec3Abs(Frames.Body2Obj(model.vehicle.inertiaTensor));
            Rb.inertiaTensorRotation = UQuat(
                Frames.Body2Obj(Frames.Cons2Body(model.vehicle.massFrame)));
            Rb.maxAngularVelocity = 7;
            Rb.velocity = transform.forward * 200;

            controller = new Controller(model.fcs);
            controller.axes[(int)Controller.AxisChannel.Throttle].value = 0.5f;
            hud = transform.Find("HUD")?.GetComponent<HUDController>();
        }

        void FixedUpdate() {
            controller.UpdateChannel();

            Vector3 vel = transform.InverseTransformVector(Rb.velocity);
            var rot = Frames.WindRot(SVec3(vel));
            var w2b = Frames.Wind2Body(rot);
            // update property
            model.aero.force = frc;
            model.aero.rotation = rot;
            model.aero.vel = vel.magnitude;
            model.motion.angular = Frames.FlipHandedness(Frames.Obj2Body(SVec3(
                transform.InverseTransformVector(Rb.angularVelocity))));
            model.motion.roll.Val = Rb.rotation.eulerAngles.z * Mathf.Deg2Rad;
            model.motion.alt.Val = Rb.position.y - (float)shifter.WorldOrigin.y;
            model.motion.terrainAlt.Val = geo.GetHeight(shifter.WorldOrigin + transform.position);
            model.UpdateProperty(Time.deltaTime);
            (frc, trq) = model.Eval();

            force = UVec3(Frames.Body2Obj(w2b * frc));
            torque = UVec3(Frames.FlipHandedness(Frames.Body2Obj(trq)));
            Logger.Debug($"rot={rot}, vel={vel}");
            Logger.Debug($"angular={model.motion.angular}, p={model.motion.p}");
            Logger.Debug($"trq={trq}, torque={torque}");
            force.z += controller.axes[(int)Controller.AxisChannel.Throttle].value * 80000;
            Rb.AddRelativeForce(force, ForceMode.Force);
            Rb.AddRelativeTorque(torque, ForceMode.Force);
            UpdateMonitor();
        }

        void UpdateMonitor() {
            hud.speed = model.aero.IAS * 3.6f;
            hud.alt = model.motion.alt.Val;
            hud.mach = model.aero.mach.Val;
            hud.UpdateParams(Rb, Time.fixedDeltaTime);
        }

        public static Vector3 UVec3(System.Numerics.Vector3 v) {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector3 UVec3Abs(System.Numerics.Vector3 v) {
            return new Vector3(Mathf.Abs(v.X), Mathf.Abs(v.Y), Mathf.Abs(v.Z));
        }

        public static System.Numerics.Vector3 SVec3(Vector3 v) {
            return new System.Numerics.Vector3(v.x, v.y, v.z);
        }

        public static Quaternion UQuat(System.Numerics.Quaternion q) {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
    }
}
