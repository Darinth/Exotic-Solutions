using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleExoticsCore : PartModule
    {
        [KSPField]
        public double ElectricUsage = 100f;

        [KSPField]
        public double EEGeneration = 0.2f;

        [KSPField]
        public double EELossPercent = 0.08f;

        [KSPField]
        public double EELossFlat = 0.02f;

        [KSPField]
        public double CoreStrengthExponentMultiplier = 1d;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Core Strength:", guiFormat = "P2"), UI_Label()]
        public double CoreStrength = 1f;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Active:", guiActiveEditor = false), UI_Toggle(disabledText = "No", enabledText = "Yes")]
        public bool active = false;

        PartResource exoticEnergy;

        double activeTime = 0f;

        [KSPAction(guiName = "Toggle EE Generator", requireFullControl = true)]
        public void ActionToggleEEGenerator(KSPActionParam actionParams)
        {
            active = !active;
        }

        [KSPAction(guiName = "Activate EE Generator", requireFullControl = true)]
        public void ActionActivateEEGenerator(KSPActionParam actionParams)
        {
            active = true;
        }

        [KSPAction(guiName = "Deactivate EE Generator", requireFullControl = true)]
        public void ActionDeactivateEEGenerator(KSPActionParam actionParams)
        {
            active = false;
        }

        public override void OnAwake()
        {
            base.OnAwake();
        }

        public override void OnLoad(ConfigNode node)
        {
            exoticEnergy = this.part.Resources[Constants.EEDefinition.name];
        }

        public void FixedUpdate()
        {
            if(part.CrewCapacity > 0)
            {

            }

            if(active)
            {
                activeTime += TimeWarp.fixedDeltaTime;
                CoreStrength = Math.Pow(0.995, activeTime * CoreStrengthExponentMultiplier);
                if (CoreStrength < 0.1)
                {
                    active = false;
                }
            }
            else
            {
                activeTime -= TimeWarp.fixedDeltaTime;
                if (activeTime < 0f) activeTime = 0f;
                CoreStrength = Math.Pow(0.995, activeTime * CoreStrengthExponentMultiplier);
            }

            double EEChange;
            EEChange = -((exoticEnergy.amount * EELossPercent + EELossFlat) * TimeWarp.fixedDeltaTime);
            if (active)
            {
                double ECRequest = ElectricUsage * TimeWarp.fixedDeltaTime;
                double returnedEC = this.part.RequestResource(Constants.ECDefinition.id, ECRequest);
                EEChange += (EEGeneration * (returnedEC / ElectricUsage) * CoreStrength);
            }
            double EEAvailable;
            double EEMax;
            part.GetConnectedResourceTotals(Constants.EEDefinition.id, out EEAvailable, out EEMax, true);
            if (EEChange > 0)
            {
                double EMAvailable;
                double EMMax;
                part.GetConnectedResourceTotals(Constants.EMDefinition.id, out EMAvailable, out EMMax, true);
                double EESpace = EEMax - EEAvailable; ;
                if (EMAvailable < EEChange)
                    EEChange = EMAvailable;
                if (EESpace < EEChange)
                    EEChange = EESpace;
                part.RequestResource(Constants.EMDefinition.id, EEChange);
            }
            else
            {
                if (EEAvailable < -EEChange)
                    EEChange = -EEAvailable;
            }
            exoticEnergy.amount += EEChange;
            //if (exoticEnergy.amount / exoticEnergy.maxAmount > 0.999)
            //    exoticEnergy.amount = exoticEnergy.maxAmount;
        }
    }
}
