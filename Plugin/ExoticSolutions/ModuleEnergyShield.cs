﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleEnergyShield : PartModule, IAirstreamShield
    {
        [KSPField]
        public double shieldHeatCost = 0.1;

        [KSPField]
        public double shieldHeatThreshhold = 1000;

        [KSPField]
        public double EEToShieldRatio = 1000;

        [KSPField]
        public double EEToShieldRate = 100;

        GameObject shieldObject;
        GameObject colliderObject;
        Collider shieldCollider;

        [KSPField(isPersistant = true)]
        bool shieldOn = false;

        List<Callback<IAirstreamShield>> shieldCallbacks;

        PartResource shieldPower;

        bool shieldInitialized = false;

        float baseCrashTolerance;

        [KSPField(isPersistant = true)]
        public bool shieldGeneratorOn = false;

        public double generateShieldEnergy(double amount)
        {
            if (vessel)
            {
                double EEAvailable;
                double EEMax;
                part.GetConnectedResourceTotals(Constants.EEDefinition.id, out EEAvailable, out EEMax, true);

                if (EEAvailable < (amount / EEToShieldRatio))
                    amount = EEAvailable * EEToShieldRatio;

                double shieldSpace = shieldPower.maxAmount - shieldPower.amount;
                if (shieldSpace < amount)
                    amount = shieldSpace;
                shieldPower.amount += amount;
                part.RequestResource(Constants.EEDefinition.id, amount/EEToShieldRatio);
                return amount;
            }
            return 0d;
        }

        public override void OnAwake()
        {
            base.OnAwake();
            shieldObject = gameObject.GetChild("SphereShield");
            colliderObject = shieldObject.GetChild("collider");
            shieldCollider = colliderObject.GetComponent<MeshCollider>();
            shieldCallbacks = new List<Callback<IAirstreamShield>>();
            shieldPower = this.part.Resources[Constants.SPDefinition.name];
            shieldObject.SetActive(false);
            baseCrashTolerance = part.crashTolerance;
        }

        public override void OnInitialize()
        {
            if (vessel)
            {
                base.OnInitialize();
                foreach (Part part in vessel.parts)
                {
                    if (part != this.part)
                        shieldCallbacks.Add(part.AddShield(this));
                }
                foreach (Callback<IAirstreamShield> callback in shieldCallbacks)
                {
                    callback(this);
                }
                if (!shieldOn)
                {
                    part.DragCubes.SetCubeWeight("Default", 1);
                    part.DragCubes.SetCubeWeight("Shield", 0);
                }
                else
                {
                    shieldObject.SetActive(true);
                    CollisionManager.IgnoreCollidersOnVessel(vessel, new Collider[] { shieldCollider });
                    shieldInitialized = true;
                    part.DragCubes.SetCubeWeight("Default", 0);
                    part.DragCubes.SetCubeWeight("Shield", 1);
                }
            }
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiActiveUncommand = false, guiName = "Activate Shield", name = "ShieldToggle", requireFullControl = true)]
        public void ShieldToggle()
        {
            if (shieldOn)
                DeactivateShield();
            else
                ActivateShield();
        }

        public void ActivateShield()
        {
            Events["ShieldToggle"].guiName = "Deactivate Shield";
            shieldOn = true;
            part.DragCubes.SetCubeWeight("Default", 0);
            part.DragCubes.SetCubeWeight("Shield", 1);
            shieldObject.SetActive(true);
            if (!shieldInitialized && vessel != null)
            {
                shieldInitialized = true;
                CollisionManager.IgnoreCollidersOnVessel(vessel, new Collider[] { shieldCollider });
            }
            foreach (Callback<IAirstreamShield> callback in shieldCallbacks)
            {
                callback(this);
            }
        }

        public void DeactivateShield()
        {
            Events["ShieldToggle"].guiName = "Activate Shield";
            shieldOn = false;
            part.DragCubes.SetCubeWeight("Default", 1);
            part.DragCubes.SetCubeWeight("Shield", 0);
            shieldObject.SetActive(false);
            foreach (Callback<IAirstreamShield> callback in shieldCallbacks)
            {
                callback(this);
            }
            part.crashTolerance = baseCrashTolerance;
        }

        [KSPAction(guiName = "Toggle Shield", requireFullControl = true)]
        public void ActionToggleShield(KSPActionParam actionParams)
        {
            ShieldToggle();
        }

        [KSPAction(guiName = "Activate Shield", requireFullControl = true)]
        public void ActionActivateShield(KSPActionParam actionParams)
        {
            ActivateShield();
        }

        [KSPAction(guiName = "Deactivate Shield", requireFullControl = true)]
        public void ActionDeactivateShield(KSPActionParam actionParams)
        {
            DeactivateShield();
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiActiveUncommand = false, guiName = "Activate Generator", name = "GeneratorToggle", requireFullControl = true)]
        public void GeneratorToggle()
        {
            if (shieldGeneratorOn)
                DeactivateGenerator();
            else
                ActivateGenerator();
        }

        public void ActivateGenerator()
        {
            Events["GeneratorToggle"].guiName = "Deactivate Generator";
            shieldGeneratorOn = true;
        }

        public void DeactivateGenerator()
        {
            Events["GeneratorToggle"].guiName = "Activate Generator";
            shieldGeneratorOn = false;
        }

        [KSPAction(guiName = "Toggle Generator", requireFullControl = true)]
        public void ActionToggleGenerator(KSPActionParam actionParams)
        {
            ShieldToggle();
        }

        [KSPAction(guiName = "Activate Generator", requireFullControl = true)]
        public void ActionActivateGenerator(KSPActionParam actionParams)
        {
            ActivateShield();
        }

        [KSPAction(guiName = "Deactivate Generator", requireFullControl = true)]
        public void ActionDeactivateGenerator(KSPActionParam actionParams)
        {
            DeactivateShield();
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (shieldOn)
            {
                bool shieldCollided = false;
                foreach (ContactPoint contactPoint in collision.contacts)
                {
                    if (contactPoint.thisCollider == shieldCollider)
                    {
                        shieldCollided = true;
                        break;
                    }
                }
                if (shieldCollided)
                {
                    double energyRequired = collision.impulse.magnitude * 2;

                    double SPReceived = part.RequestResource(Constants.SPDefinition.id, energyRequired);
                    if ((energyRequired - SPReceived) > 1d)
                        part.explode();
                }
            }
        }

        public void OnDestroy()
        {
            shieldOn = false;
            foreach (Callback<IAirstreamShield> callback in shieldCallbacks)
            {
                callback(this);
            }
        }

        public void FixedUpdate()
        {
            if (vessel)
            {
                if(shieldGeneratorOn)
                    generateShieldEnergy(EEToShieldRate * TimeWarp.fixedDeltaTime);

                if (shieldOn)
                {
                    if (part.skinTemperature > shieldHeatThreshhold)
                    {
                        double SPReceived = part.RequestResource(Constants.SPDefinition.id, (part.skinTemperature - shieldHeatThreshhold) * shieldHeatCost);
                        part.skinTemperature -= SPReceived / shieldHeatCost;
                    }
                    if (part.temperature > shieldHeatThreshhold)
                    {
                        double SPReceived = part.RequestResource(Constants.SPDefinition.id, (part.temperature - shieldHeatThreshhold) * shieldHeatCost);
                        part.temperature -= SPReceived / shieldHeatCost;
                    }
                    part.crashTolerance = (float)shieldPower.amount / 30;
                }
            }

        }

        public bool ClosedAndLocked()
        {
            return shieldOn;
        }

        public Vessel GetVessel()
        {
            return vessel;
        }

        public Part GetPart()
        {
            return part;
        }
    }
}
