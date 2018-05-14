using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleInertialShunt : PartModule
    {
        private char[] aSpace = new char[] {' '};

        public enum ShuntMode
        {
            Off = 0,
            ManualControl = 1,
            KillSurfaceVelocity = 2
        }

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Status"), UI_Label()]
        public string Status = "";

        [KSPField]
        public double maxThrust = 300f;

        [KSPField]
        public double maxTorque = 20f;

        [KSPField]
        public double maxThrustEE = 0.005;

        [KSPField]
        public double maxTorqueEE = 0.005;

        [KSPField]
        public FloatCurve EECostPerMeterCurve = new FloatCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(20000, 0.005f), new Keyframe(31000, 0.016f), new Keyframe(70000, 0.1f) });

        [KSPField]
        public double maxEESec = 0.015f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Mode", advancedTweakable = true), UI_ChooseOption(display = new string[] { "Off", "Manual", "Kill Surface Belocity" }, options = new string[] { "Off", "ManualControl", "KillSurfaceVelocity" })]
        public string shuntModeString = "ManualControl";
        public ShuntMode shuntMode;

        public override void OnAwake()
        {
            Fields["shuntModeString"].uiControlEditor.onFieldChanged += ShuntModeStringChanged;
            Fields["shuntModeString"].uiControlFlight.onFieldChanged += ShuntModeStringChanged;

            base.OnAwake();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            updateShuntModeFromString();
        }

        private void ShuntModeStringChanged(BaseField field, object what)
        {
            updateShuntModeFromString();
        }

        public void updateShuntModeFromString()
        {
            shuntMode = (ShuntMode)Enum.Parse(typeof(ShuntMode), shuntModeString);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            KSPLog.print("Shunt: OnSave");

           /* ConfigNode CostPerMeterNode = new ConfigNode();
            foreach (Keyframe key in EECostPerMeterCurve.Curve.keys)
            {
                CostPerMeterNode.AddValue("key", key.time + " " + key.value + " " + key.inTangent + " " + key.outTangent);
            }
            node.AddNode("CostPerMeterCurve", CostPerMeterNode);*/
        }

        public override void OnCopy(PartModule fromModule)
        {
            base.OnCopy(fromModule);
            //EECostPerMeterCurve = new FloatCurve(((ModuleInertialShunt)fromModule).EECostPerMeterCurve.Curve.keys);
        }

        public override void OnLoad(ConfigNode node)
        {
            //ConfigNode CostPerMeterNode = new ConfigNode();
            base.OnLoad(node);
            /*KSPLog.print("Shunt: OnLoad");
            if(node.TryGetNode("CostPerMeterCurve", ref CostPerMeterNode))
            {
                KSPLog.print("Shunt: CostPerMeterCurve");
                EECostPerMeterCurve = new FloatCurve();
                string[] keys = CostPerMeterNode.GetValues("key");
                foreach (string key in keys)
                {
                    string[] values = key.Split(aSpace);
                    if(values.Length == 2)
                    {
                        try
                        {
                            EECostPerMeterCurve.Add(float.Parse(values[0]), float.Parse(values[1]));
                        }
                        catch(Exception e)
                        {
                            KSPLog.print("Exception processing CostPerMeterCurve key " + key);
                            KSPLog.print(e.ToString());
                        }
                    }
                    else if(values.Length == 4)
                    {
                        try
                        {
                            EECostPerMeterCurve.Add(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                        }
                        catch (Exception e)
                        {
                            KSPLog.print("Exception processing CostPerMeterCurve key " + key);
                            KSPLog.print(e.ToString());
                        }
                    }
                    else
                    {
                        KSPLog.print("Invalid key: " + key);
                    }
                }
            }*/
        }

        public void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if(shuntMode == ShuntMode.ManualControl && (vessel.ctrlState.mainThrottle > 0 || vessel.ctrlState.yaw != 0 || vessel.ctrlState.pitch != 0 || vessel.ctrlState.roll != 0))
                {
                    double EEAvailable;
                    double EEMax;
                    double maxEEUpdate = maxEESec * TimeWarp.fixedDeltaTime;
                    KSPLog.print("maxEEUpdate: " + maxEEUpdate);
                    part.GetConnectedResourceTotals(Constants.EEDefinition.id, out EEAvailable, out EEMax);
                    KSPLog.print("EEAvailable: " + EEAvailable);
                    if (EEAvailable < maxEEUpdate)
                        maxEEUpdate = EEAvailable;

                    double remainingEEUpdate = maxEEUpdate;

                    double EEDistanceCost = EECostPerMeterCurve.Evaluate((float)vessel.radarAltitude) * TimeWarp.fixedDeltaTime;
                    KSPLog.print("EEDistanceCost: " + EEDistanceCost);
                    KSPLog.print("EECostPerMeterKeys: " + EECostPerMeterCurve.Curve.keys.Length);
                    remainingEEUpdate -= EEDistanceCost;

                    if (remainingEEUpdate > 0)
                    {
                        double EEThrustCost = vessel.ctrlState.mainThrottle * maxThrustEE * TimeWarp.fixedDeltaTime;
                        double EETorqueCost = Math.Max(Math.Max(vessel.ctrlState.yaw, vessel.ctrlState.pitch), vessel.ctrlState.roll) * maxTorqueEE * TimeWarp.fixedDeltaTime;
                        double throttleLimit;

                        if (remainingEEUpdate < EEThrustCost + EETorqueCost)
                        {
                            throttleLimit = remainingEEUpdate / (EEThrustCost + EETorqueCost);
                            Status = "Power Limited " + throttleLimit.ToString("P2");
                        }
                        else
                        {
                            throttleLimit = 1;
                            Status = "Full power";
                        }

                        part.RequestResource(Constants.EEDefinition.id, EEDistanceCost + (EEThrustCost + EETorqueCost) * throttleLimit);

                        //Accelerate in part's forward direction
                        Vector3 force = transform.up * (float)maxThrust * vessel.ctrlState.mainThrottle * (float)throttleLimit;
                        force *= TimeWarp.fixedDeltaTime;

                        part.Rigidbody.AddForce(force, ForceMode.Impulse);

                        part.Rigidbody.AddTorque(transform.forward * (float)maxTorque * -vessel.ctrlState.yaw * (float)throttleLimit * TimeWarp.fixedDeltaTime, ForceMode.Impulse);
                        part.Rigidbody.AddTorque(transform.right * (float)maxTorque * -vessel.ctrlState.pitch * (float)throttleLimit * TimeWarp.fixedDeltaTime, ForceMode.Impulse);
                        part.Rigidbody.AddTorque(transform.up * (float)maxTorque * -vessel.ctrlState.roll * (float)throttleLimit * TimeWarp.fixedDeltaTime, ForceMode.Impulse);
                    }
                    else
                    {
                        Status = "Max Distance Exceeded";
                    }

                }
                else if(shuntMode == ShuntMode.KillSurfaceVelocity)
                {
                    Vector3 force = (vessel.srf_velocity + vessel.graviticAcceleration + FlightGlobals.getCentrifugalAcc(part.transform.position, vessel.mainBody)) * vessel.totalMass * -1;
                    if (force.magnitude > maxThrust)
                        force = force.normalized * (float)maxThrust;
                    force *= TimeWarp.fixedDeltaTime;

                    part.Rigidbody.AddForce(force, ForceMode.Impulse);

                    part.Rigidbody.AddTorque(transform.forward * (float)maxTorque * -vessel.ctrlState.yaw * TimeWarp.fixedDeltaTime, ForceMode.Impulse);
                    part.Rigidbody.AddTorque(transform.right * (float)maxTorque * -vessel.ctrlState.pitch * TimeWarp.fixedDeltaTime, ForceMode.Impulse);
                    part.Rigidbody.AddTorque(transform.up * (float)maxTorque * -vessel.ctrlState.roll * TimeWarp.fixedDeltaTime, ForceMode.Impulse);
                }
                else
                {
                    Status = "Inactive";
                }
            }
        }

    }
}
