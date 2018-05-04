using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleOrbitalResourceConverter : ModuleResourceConverter
    {
        public override void FixedUpdate()
        {
            if(vessel && vessel.situation != Vessel.Situations.ORBITING && IsActivated)
            {
                KSPLog.print("Shutting down Orbital Converter. Not in orbit.");
                StopResourceConverter();
            }
            base.FixedUpdate();
        }
    }
}
