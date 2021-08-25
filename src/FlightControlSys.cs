namespace MinimalJSim {
    public class FlightControlSys {
        public Property aileronPos, elevatorPos, rudderPos;
        public Property leadEdgeFlapPos, flaperonMix;
        public Property gearPos, speedBrakePos;

        public void Set(float aileron, float elevator, float rudder) {
            aileronPos.Value = aileron;
            elevatorPos.Value = elevator;
            rudderPos.Value = rudder;
        }
    }
}