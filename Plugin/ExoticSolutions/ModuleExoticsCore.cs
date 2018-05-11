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

        [KSPField(isPersistant = true)]
        double saveTime;

        public double generateEE(double amount)
        {
            if (vessel)
            {
                double EMAvailable;
                double EMMax;
                part.GetConnectedResourceTotals(Constants.EMDefinition.id, out EMAvailable, out EMMax, true);
                KSPLog.print("GenerateEE");
                KSPLog.print("EMAvailable: " + EMAvailable);
                KSPLog.print("EMMax: " + EMMax);
                KSPLog.print("amount: " + amount);

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

        public override void OnInitialize()
        {
            KSPLog.print("Core: OnInitialize");
            base.OnInitialize();
            if (vessel)
            {
                KSPLog.print(vessel.situation.ToString());
                if((vessel.situation | Vessel.Situations.PRELAUNCH) == Vessel.Situations.PRELAUNCH)
                {
                    KSPLog.print(exoticEnergy.amount.ToString() + "/" + exoticEnergy.maxAmount.ToString());
                    //Perform prelaunch conversion
                    if ((vessel.situation & Vessel.Situations.PRELAUNCH) == Vessel.Situations.PRELAUNCH)
                        generateEE(exoticEnergy.maxAmount - exoticEnergy.amount);
                    else
                        generateEE(EEGeneration * (Planetarium.GetUniversalTime() - saveTime));
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            KSPLog.print("Core: OnLoad");
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            KSPLog.print("Core: OnStart(" + state.ToString() + ")");
        }

        public override void OnAwake()
        {
            saveTime = Planetarium.GetUniversalTime();
            base.OnAwake();
            KSPLog.print("Core: OnAwake");
            exoticEnergy = this.part.Resources[Constants.EEDefinition.name];
        }

        public override void OnSave(ConfigNode node)
        {
            saveTime = Planetarium.GetUniversalTime();
            base.OnSave(node);
            KSPLog.print("Core: OnSave");
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
