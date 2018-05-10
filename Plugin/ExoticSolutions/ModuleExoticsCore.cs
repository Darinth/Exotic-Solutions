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

        PartResource exoticEnergy;

        double saveTime;

        public double generateEE(double amount)
        {
            if (vessel)
            {
                double EMAvailable;
                double EMMax;
                part.GetConnectedResourceTotals(Constants.EMDefinition.id, out EMAvailable, out EMMax, true);

                if (EMAvailable < amount)
                    amount = EMAvailable;
                exoticEnergy.amount += amount;
                part.RequestResource(Constants.EMDefinition.id, amount);
                part.RequestResource(Constants.ECDefinition.id, -amount * EMtoECRatio);
                return amount;
            }
            return 0d;
        }

        public override void OnInitialize()
        {
            KSPLog.print("Core: OnInitialize");
            base.OnInitialize();
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            KSPLog.print("Core: OnLoad");
            if(!node.TryGetValue("SaveTime", ref saveTime))
                saveTime = 0d;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            KSPLog.print("Core: OnStart(" + state.ToString() + ")");
            if (vessel)
            {
                KSPLog.print(vessel.situation.ToString());
                KSPLog.print(exoticEnergy.amount.ToString() + "/" + exoticEnergy.maxAmount.ToString());
                //Perform prelaunch conversion
                if ((vessel.situation & Vessel.Situations.PRELAUNCH) == Vessel.Situations.PRELAUNCH)
                    generateEE(exoticEnergy.maxAmount - exoticEnergy.amount);
                else
                    generateEE(EEGeneration * (Planetarium.GetUniversalTime() - saveTime));
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
            if (vessel)
            {
                generateEE(EEGeneration * TimeWarp.fixedDeltaTime);
            }
        }
    }
}
