namespace MinimalJSim {
    public class FlightControlSys {
        public Property aileronPosL, aileronPosR, elevatorPos, rudderPos;
        public Property leadEdgeFlapPos, flaperonMix;
        public Property gearPos, speedBrakePos;

        public FlightControlSys(DynamicsModel model) {
            aileronPosL = model.GetProperty("fcs/left-aileron-pos-1?");
            aileronPosR = model.GetProperty("fcs/right-aileron-pos-1?");
            elevatorPos = model.GetProperty("fcs/elevator-pos-1?");
            rudderPos = model.GetProperty("fcs/rudder-pos-1?");
            leadEdgeFlapPos = model.GetProperty("fcs/lef-pos-1?");
            flaperonMix = model.GetProperty("fcs/flaperon-mix-1?");
            speedBrakePos = model.GetProperty("fcs/speedbrake-pos-1?");
            gearPos = model.GetProperty("gear/gear-pos-1?");
        }

        public void Set(float aileronL, float aileronR, float elevator, float rudder) {
            aileronPosL.Val = aileronL;
            aileronPosR.Val = aileronR;
            elevatorPos.Val = elevator;
            rudderPos.Val = rudder;
        }
    }
}