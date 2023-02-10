using System.IO;
using FlightDisplay;
using UnityEditor;
using UnityEngine;

namespace MinimalJSim {
    public class FDM : MonoBehaviour {
        public TextAsset ConfigXML;

        public Rigidbody Rb { get; private set; }
        public DynamicsModel model;
        public Controller controller;
        [System.NonSerialized]
        public Vector3 force, torque;
        System.Numerics.Vector3 frc, trq;
        HUDController hud;
        Overlook.OriginShifter shifter;
        Overlook.GeoClipMap geo;

        // Start is called before the first frame update
        void Start() {
            ConvertData();
            Init();
            geo = Overlook.GeoClipMap.Get();
            shifter = Overlook.OriginShifter.Get(transform);
            Overlook.OriginShifter.SetViewer(transform);
        }

        [ContextMenu("GenFDM")]
        void ConvertData() {
            Debug.LogFormat($"ConfigXML={ConfigXML.name}");
            fdm_config conf;
            Adapter.DeSerXML(new StringReader(ConfigXML.text), out conf);
            string root = Path.GetDirectoryName(AssetDatabase.GetAssetPath(ConfigXML));
            Adapter.ExpandInclude(conf, root);
            model = Parser.Parse(conf);
        }

        void Init() {
            Rb = transform.GetComponent<Rigidbody>();
            Rb.mass = model.vehicle.emptyWeight.Val;
            Rb.centerOfMass = UVector3(model.vehicle.centerOfMass);
            Rb.inertiaTensor = UInertia(model.vehicle.inertiaTensor);
            Rb.inertiaTensorRotation = UQuaternion(model.vehicle.massFrame);
            Rb.maxAngularVelocity = 7;
            Rb.velocity = transform.forward * 200;

            controller = new Controller(model.fcs);
            controller.axes[(int)Controller.AxisChannel.Throttle].value = 0.5f;
            hud = transform.Find("HUD")?.GetComponent<HUDController>();
        }

        void FixedUpdate() {
            controller.UpdateChannel();

            // update property
            model.aero.force = frc;
            model.aero.vel = SVector3(transform.InverseTransformVector(-Rb.velocity));
            model.motion.angular = SVector3(transform.InverseTransformDirection(Rb.angularVelocity));
            model.motion.roll.Val = Rb.rotation.eulerAngles.z * Mathf.Deg2Rad;
            model.motion.alt.Val = Rb.position.y - (float)shifter.WorldOrigin.y;
            model.motion.terrainAlt.Val = geo.GetHeight(shifter.WorldOrigin + transform.position);
            model.UpdateProperty(Time.deltaTime);
            (frc, trq) = model.Eval();

            (force, torque) = (UVector3(frc), UVector3(trq));
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

        public static Vector3 UVector3(System.Numerics.Vector3 v) {
            return new Vector3(v.Y, v.Z, -v.X);
        }

        public static Vector3 UInertia(System.Numerics.Vector3 v) {
            return new Vector3(v.Y, v.Z, v.X);
        }

        public static Quaternion UQuaternion(System.Numerics.Quaternion q) {
            return new Quaternion(q.Y, q.Z, -q.X, q.W);
        }

        public static System.Numerics.Vector3 SVector3(Vector3 v) {
            return new System.Numerics.Vector3(-v.z, v.x, v.y);
        }
    }
}
