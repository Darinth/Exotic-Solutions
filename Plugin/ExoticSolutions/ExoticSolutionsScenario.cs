using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    //[KSPAddon(KSPAddon.Startup.AllGameScenes,true)]
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    class ExoticSolutionsScenario : ScenarioModule
    {
        [KSPField(isPersistant = true)]
        public double storedExoticMatter = 0d;

        public void Start()
        {
            Constants.exoticSolutionScenario = this;
            GameEvents.OnVesselRollout.Add(ShipRolloutEvent);
            GameEvents.onVesselRecovered.Add(VesselRecoveredEvent);
            GameEvents.onVesselRecoveryProcessing.Add(VesselRecoveryProcessingEvent);
            GameEvents.onVesselRecoveryProcessingComplete.Add(VesselRecoveryProcessingCompleteEvent);
            GameEvents.OnVesselRecoveryRequested.Add(VesselRecoveryRequestedEvent);
            GameEvents.onVesselCreate.Add(VesselCreateEvent);
            KSPLog.print("ES: ScenarioStart");
        }

        public void OnDisable()
        {
            Constants.exoticSolutionScenario = null;
            GameEvents.OnVesselRollout.Remove(ShipRolloutEvent);
            GameEvents.onVesselRecovered.Remove(VesselRecoveredEvent);
            GameEvents.onVesselRecoveryProcessing.Remove(VesselRecoveryProcessingEvent);
            GameEvents.onVesselRecoveryProcessingComplete.Remove(VesselRecoveryProcessingCompleteEvent);
            GameEvents.OnVesselRecoveryRequested.Remove(VesselRecoveryRequestedEvent);
            GameEvents.onVesselCreate.Remove(VesselCreateEvent);
            KSPLog.print("ES: ScenarioDisable");
        }

        public void ShipRolloutEvent(ShipConstruct construct)
        {
            KSPLog.print("ES: ShipRolloutEvent");
            foreach(Part part in construct.parts)
            {
                if(part.Modules.Contains<ModuleExoticMaterialsResourceTweaker>())
                {
                    ModuleExoticMaterialsResourceTweaker tweaker = part.Modules.GetModule<ModuleExoticMaterialsResourceTweaker>();
                    if (tweaker.TweakedEM > storedExoticMatter)
                        tweaker.TweakedEM = (float)storedExoticMatter;
                    part.Resources[Constants.EMDefinition.name].amount = tweaker.TweakedEM;
                    storedExoticMatter -= tweaker.TweakedEM;
                    KSPLog.print("Spent " + tweaker.TweakedEM + " EM");
                }
            }
            KSPLog.print("Stored EM: " + storedExoticMatter);
        }

        public void VesselRecoveredEvent(ProtoVessel protoVessel, bool somethin)
        {
            KSPLog.print("ES: VesselRecoveredEvent");
            foreach(ProtoPartSnapshot partSnapshot in protoVessel.protoPartSnapshots)
            {
                foreach(ProtoPartResourceSnapshot resourceSnapshot in partSnapshot.resources)
                {
                    if(resourceSnapshot.resourceName == Constants.EMDefinition.name || resourceSnapshot.resourceName == Constants.EEDefinition.name)
                    {
                        storedExoticMatter += resourceSnapshot.amount;
                        KSPLog.print("Storing " + resourceSnapshot.amount + " EM");
                        resourceSnapshot.amount = 0;
                    }
                }
            }
            KSPLog.print("Stored EM: " + storedExoticMatter);
        }

        public void VesselRecoveryProcessingEvent(ProtoVessel protoVessel, KSP.UI.Screens.MissionRecoveryDialog dialog, float something)
        {
            KSPLog.print("ES: VesselRecoveryProcessing");
        }

        public void VesselRecoveryProcessingCompleteEvent(ProtoVessel protoVessel, KSP.UI.Screens.MissionRecoveryDialog dialog, float something)
        {
            KSPLog.print("ES: VesselRecoveryProcessingCompleteEvent");
        }

        public void VesselRecoveryRequestedEvent(Vessel vessel)
        {
            KSPLog.print("ES: VesselRecoveryRequestedEvent");
        }

        public void VesselCreateEvent(Vessel vessel)
        {
            KSPLog.print("ES: VesselCreateEvent");
        }
    }
}
