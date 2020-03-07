using UnityEngine;

namespace SA
{
    public class XboxController
    {
        public float leftStickHorizontal;
        public float leftStickVertical;
        public float rightStickHoriztonal;
        public float rightStickVertical;

        public bool a_input;
        public bool b_input;
        public bool x_input;
        public bool y_input;

        public bool rt_input;
        public float rt_axis;

        public bool lt_input;
        public float lt_axis;

        public XboxController()
        {
            leftStickHorizontal = Input.GetAxis("LeftStickHorizontal");
            leftStickVertical = Input.GetAxis("LeftStickVertical");

            a_input = Input.GetButton("a_input");
            b_input = Input.GetButton("b_input");
            x_input = Input.GetButton("x_input");
            y_input = Input.GetButton("y_input");

            rt_input = Input.GetButton("RT");
            rt_axis = Input.GetAxis("RT");
            if(rt_axis != 0)
            {
                rt_input = true;
            }

            lt_input = Input.GetButton("LT");
            lt_axis = Input.GetAxis("LT");
            if(lt_axis != 0)
            {
                lt_input = true;
            }
        }
    }
}