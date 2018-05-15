using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    public static class vesselExtensions
    {
        public static void addForce(this Vessel vessel, Vector3 force, ForceMode mode)
        {
            if(mode == ForceMode.Force || mode == ForceMode.Impulse)
            {
                foreach (Part part in vessel.parts)
                {
                    part.Rigidbody.AddForce(force * (float)(part.mass / vessel.totalMass), mode);
                }
            }
            else
            {
                foreach (Part part in vessel.parts)
                {
                    part.Rigidbody.AddForce(force, mode);
                }
            }
        }
    }
}
