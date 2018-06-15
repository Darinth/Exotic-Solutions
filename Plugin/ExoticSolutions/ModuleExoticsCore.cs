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
        public double EMtoECRatio = 1000f;

        [KSPField]
        public double EEGeneration = 1d / 3600d;

        [KSPField]
        public double EEReversion = 1d / 1200d;

        [KSPField(isPersistant = true)]
        public bool active = false;

        PartResource exoticEnergy;

        double saveTime = 0;

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiActiveUncommand = false, guiName = "Activate Core", name = "CoreToggle", requireFullControl = true)]
        public void CoreToggle()
        {
            if (active)
                DeactivateCore();
            else
                ActivateCore();
        }

        public void deactivateOtherCores()
        {
            List<Part> partlist;
            if(vessel)
            {
                partlist = vessel.parts;
            }
            else if(EditorLogic.fetch != null && EditorLogic.fetch.ship != null)
            {
                partlist = EditorLogic.fetch.ship.parts;
            }
            else
            {
                partlist = null;
                KSPLog.print("ERROR: deactivateOtherCores called... but can't figure out how!");
            }
            foreach (Part part in partlist)
            {
                ModuleExoticsCore otherModule = part.FindModuleImplementing<ModuleExoticsCore>();
                if (otherModule != null && otherModule != this)
                {
                    otherModule.DeactivateCore();
                }
            }
        }

        public void ActivateCore()
        {
            Events["CoreToggle"].guiName = "Deactivate Core";
            active = true;
            deactivateOtherCores();
        }

        public void DeactivateCore()
        {
            Events["CoreToggle"].guiName = "Activate Core";
            active = false;
        }

        [KSPAction(guiName = "Toggle Core", requireFullControl = true)]
        public void ActionToggleCore(KSPActionParam actionParams)
        {
            CoreToggle();
        }

        [KSPAction(guiName = "Activate Core", requireFullControl = true)]
        public void ActionActivateCore(KSPActionParam actionParams)
        {
            ActivateCore();
        }

        [KSPAction(guiName = "Deactivate Core", requireFullControl = true)]
        public void ActionDeactivateCore(KSPActionParam actionParams)
        {
            DeactivateCore();
        }

        public double generateEE(double amount)
        {
            if (vessel)
            {
                double EMAvailable;
                double EMMax;
                part.GetConnectedResourceTotals(Constants.EMDefinition.id, out EMAvailable, out EMMax, true);

                if (EMAvailable < amount)
                    amount = EMAvailable;

                double EESpace = exoticEnergy.maxAmount - exoticEnergy.amount;
                if (EESpace < amount)
                    amount = EESpace;
                exoticEnergy.amount += amount;
                part.RequestResource(Constants.EMDefinition.id, amount);
                part.RequestResource(Constants.ECDefinition.id, -amount * EMtoECRatio);
                return amount;
            }
            return 0d;
        }

        public void revertEE(double amount)
        {
            if(vessel)
            {
                if (exoticEnergy.amount < amount)
                    amount = exoticEnergy.amount;
                exoticEnergy.amount -= amount;

                double EMAvailable;
                double EMMax;
                part.GetConnectedResourceTotals(Constants.EMDefinition.id, out EMAvailable, out EMMax, false);

                if (EMAvailable < amount)
                    amount = EMAvailable;

                part.RequestResource(Constants.EMDefinition.id, -amount);
            }
        }

        public override void OnInitialize()
        {
            KSPLog.print("Core: OnInitialize");
            base.OnInitialize();
        }

        public override void OnCopy(PartModule fromModule)
        {
            base.OnCopy(fromModule);
            KSPLog.print("Core: OnCopy");
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            KSPLog.print("Core: OnLoad");
            node.TryGetValue("SaveTime", ref saveTime);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            KSPLog.print("Core: OnStart(" + state.ToString() + ")");
            if (vessel)
            {
                //KSPLog.print(vessel.situation.ToString());
                if ((vessel.situation | Vessel.Situations.PRELAUNCH) == Vessel.Situations.PRELAUNCH)
                {
                    //KSPLog.print(exoticEnergy.amount.ToString() + "/" + exoticEnergy.maxAmount.ToString());
                    //Perform prelaunch conversion
                    //if ((vessel.situation & Vessel.Situations.PRELAUNCH) == Vessel.Situations.PRELAUNCH)
                    //generateEE(exoticEnergy.maxAmount - exoticEnergy.amount);
                }
                //KSPLog.print(Planetarium.GetUniversalTime());
                //KSPLog.print(saveTime);
                //KSPLog.print(Planetarium.GetUniversalTime() - saveTime);
                if (saveTime != 0)
                    generateEE(EEGeneration * (Planetarium.GetUniversalTime() - saveTime));
            }
            if(active)
            {
                Events["CoreToggle"].guiName = "Deactivate Core";
            }
        }

        public override void OnAwake()
        {
            base.OnAwake();
            KSPLog.print("Core: OnAwake");
            exoticEnergy = this.part.Resources[Constants.EEDefinition.name];
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            KSPLog.print("Core: OnSave");
            node.AddValue("SaveTime", Planetarium.GetUniversalTime());
        }

        public void FixedUpdate()
        {
            //KSPLog.print("Core: FixedUpdate");
            if (vessel)
            {
                if (active)
                {
                    generateEE(EEGeneration * TimeWarp.fixedDeltaTime);
                }
                else
                {
                    revertEE(EEReversion * TimeWarp.fixedDeltaTime);
                }
            }
        }
    }
}
