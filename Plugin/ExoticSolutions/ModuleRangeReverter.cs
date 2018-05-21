using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExoticSolutions
{
    class ModuleRangeReverter : PartModule
    {
        [KSPField]
        float revertAltitude = 0f;

        [KSPField]
        VesselRanges originalRanges;

        public static void SetVesselRanges(Vessel vessel, float load, float unload, float pack, float unpack, float revertAltitude)
        {
            ModuleRangeReverter moduleRangeReverter;
            if (!vessel.rootPart.Modules.Contains<ModuleRangeReverter>())
            {
                moduleRangeReverter = (ModuleRangeReverter)vessel.rootPart.AddModule("ModuleRangeReverter");
                moduleRangeReverter.originalRanges = vessel.vesselRanges;
            }
            else
            {
                moduleRangeReverter = (ModuleRangeReverter)vessel.rootPart.Modules.GetModule<ModuleRangeReverter>();
            }
            moduleRangeReverter.revertAltitude = revertAltitude;

            VesselRanges newRanges = new VesselRanges(vessel.vesselRanges);
            newRanges.escaping = new VesselRanges.Situation(load, unload, pack, unpack);
            newRanges.flying = new VesselRanges.Situation(load, unload, pack, unpack);
            newRanges.landed = new VesselRanges.Situation(load, unload, pack, unpack);
            newRanges.orbit = new VesselRanges.Situation(load, unload, pack, unpack);
            newRanges.prelaunch = new VesselRanges.Situation(load, unload, pack, unpack);
            newRanges.splashed = new VesselRanges.Situation(load, unload, pack, unpack);
            newRanges.subOrbital = new VesselRanges.Situation(load, unload, pack, unpack);

            vessel.vesselRanges = newRanges;
        }

        public void FixedUpdate()
        {
            //KSPLog.print("ModuleRangeReverter: FixedUpdate");
            if(vessel.transform.position.magnitude > revertAltitude)
            {
                //KSPLog.print("ModuleRangeReverter: Reverting Range");
                vessel.vesselRanges = originalRanges;
                part.RemoveModule(this);
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        }
    }
}
