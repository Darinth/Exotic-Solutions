using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExoticSolutions
{
    class ModuleExoticMaterialsResourceTweaker : PartModule
    {
        [KSPField(guiActive = false, guiActiveEditor = true, guiName = "AvailableEM", guiFormat = "F2"), UI_Label()]
        public float AvailableEM = 0f;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "ExoticMaterials"), UI_FloatRange(minValue = 0,maxValue = 1,stepIncrement = 0.1f)]
        public float TweakedEM = 0f;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            AvailableEM = (float)Constants.exoticSolutionScenario.storedExoticMatter;
            //KSPLog.print("AvailableEM: " + AvailableEM);
            ((UI_FloatRange)Fields["TweakedEM"].uiControlEditor).maxValue = (float)part.Resources[Constants.EMDefinition.name].maxAmount;
            ((UI_FloatRange)Fields["TweakedEM"].uiControlEditor).stepIncrement = (float)part.Resources[Constants.EMDefinition.name].maxAmount/10;
            /*if ((state & StartState.PreLaunch) == StartState.PreLaunch)
            {
                KSPLog.print("TweakedEM: " + TweakedEM);
                if (AvailableEM < TweakedEM)
                    AvailableEM = TweakedEM;
                KSPLog.print("NewTweakedEM: " + TweakedEM);
                part.Resources[Constants.EMDefinition.name].amount = TweakedEM;
                Constants.exoticSolutionScenario.storedExoticMatter -= TweakedEM;
            }*/
        }

    }
}
