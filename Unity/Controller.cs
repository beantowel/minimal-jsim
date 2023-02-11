using System;
using UnityEngine;

namespace MinimalJSim {

    public class Controller {
        public enum AxisChannel {
            Pitch,
            Roll,
            Yaw,
            Throttle,
        }

        public struct Axis {
            public float value;
            public bool isRelative;
            public KeyCode pos, neg;
        }

        public Axis[] axes = new Axis[] {
            new Axis{pos = KeyCode.W, neg = KeyCode.S, isRelative = false},
            new Axis{pos = KeyCode.A, neg = KeyCode.D},
            new Axis{pos = KeyCode.Q, neg = KeyCode.E, isRelative = false},
            new Axis{pos = KeyCode.Equals, neg = KeyCode.Minus, isRelative = true},
        };

        public FlightControlSys fcs;

        public Controller(FlightControlSys _fcs) {
            fcs = _fcs;
        }


        public void UpdateChannel() {
            foreach (AxisChannel channel in Enum.GetValues(typeof(AxisChannel))) {
                var axis = axes[(int)channel];
                float v = axis.value;
                bool pos = Input.GetKey(axis.pos);
                bool neg = Input.GetKey(axis.neg);
                float control = (pos ? 1 : 0) + (neg ? -1 : 0);
                if (axis.isRelative) {
                    v = Mathf.Clamp(v + control * 0.15f * Time.fixedDeltaTime, -1, 1);
                } else {
                    v = control;
                }
                axes[(int)channel].value = v;
            }

            foreach (AxisChannel channel in Enum.GetValues(typeof(AxisChannel))) {
                float v = axes[(int)channel].value;
                switch (channel) {
                    case AxisChannel.Pitch:
                        fcs.elevatorPos.Val = v * 0.436f;
                        break;
                    case AxisChannel.Roll:
                        fcs.aileronPosL.Val = -v * 1f;
                        fcs.aileronPosR.Val = v * 1f;
                        fcs.aileronPos.Val = -v * 1f;
                        break;
                    case AxisChannel.Yaw:
                        fcs.rudderPos.Val = v * 1f;
                        break;
                }
            }
        }
    }
}