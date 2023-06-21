using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

namespace HurricaneVR.Framework.Core.Utils
{
    public class HVRShoulderSocket : HVRSocket
    {
        public float VelocityCutoff = 2f;
        
        public override bool CanHover(HVRGrabbable grabbable)
        {
            if (!base.CanHover(grabbable))
                return false;

            var handGrabber = grabbable.PrimaryGrabber as HVRHandGrabber;
            if (!handGrabber)
                return false;

            //if a forward throw is happening don't grab it
            var velocity = handGrabber.ComputeThrowVelocity(grabbable, out var dummy, true);
            velocity.y = transform.position.y;
            var dot = Vector3.Dot(transform.forward, velocity);
            if (dot > VelocityCutoff)
            {
                return false;
            }

            return true;
        }
    }
}
