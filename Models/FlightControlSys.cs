namespace MinimalJSim {
    public class FlightControlSys {
        public Property aileronPosL, aileronPosR, elevatorPos, rudderPos;
        public Property leadEdgeFlapPos, flaperonMix;
        public Property gearPos, speedBrakePos;
        public Property aileronPos;

        public FlightControlSys() { }
        public FlightControlSys(DModel model) {
            aileronPosL = model.GetProperty("fcs/left-aileron-pos-1?");
            aileronPosR = model.GetProperty("fcs/right-aileron-pos-1?");
            aileronPos = model.GetProperty("fcs/aileron-pos-1?");
            elevatorPos = model.GetProperty("fcs/elevator-pos-1?");
            rudderPos = model.GetProperty("fcs/rudder-pos-1?");
            leadEdgeFlapPos = model.GetProperty("fcs/lef-pos-1?");
            flaperonMix = model.GetProperty("fcs/flaperon-mix-1?");
            speedBrakePos = model.GetProperty("fcs/speedbrake-pos-1?");
            gearPos = model.GetProperty("gear/gear-pos-1?");
        }
    }
}