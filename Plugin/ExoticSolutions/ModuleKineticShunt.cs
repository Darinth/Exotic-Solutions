using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleKineticShunt : PartModule
    {
        private char[] aSpace = new char[] { ' ' };

        public enum ShuntMode
        {
            Off = 0,
            ManualControl = 1,
            KillSurfaceVelocity = 2,
            KillOrbitalVelocity = 3,
            KillRelativeVelocity = 4
        }

        public enum ShuntTarget
        {
            Self = 0,
            CelestialBody = 1,
            CraftTarget = 2,
            NearestVessel = 3
        }

        [KSPField(isPersistant = true)]
        public bool active = false;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Thrust"), UI_Label()]
        public string thrustStatus = "";

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Torque"), UI_Label()]
        public string torqueStatus = "";

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

        [KSPField]
        public double minForceloadRange = 35000d;

        [KSPField]
        public bool allowSourceChange = true;

        [KSPField]
        public bool allowSinkChange = true;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Mode", advancedTweakable = true), UI_ChooseOption(display = new string[] { "Off", "Manual", "Kill Surface Belocity", "Kill Orbital Velocity", "Kill Relative Velocity", "Set Target Distance" }, options = new string[] { "Off", "ManualControl", "KillSurfaceVelocity", "KillOrbitalVelocity", "KillRelativeVelocity", "SetTargetDistance" })]
        public string shuntModeString = "ManualControl";
        public ShuntMode shuntMode = ShuntMode.ManualControl;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Source", advancedTweakable = true), UI_ChooseOption(display = new string[] { "Self", "Celestial Body", "Craft Target", "Nearest Vessel" }, options = new string[] { "Self", "CelestialBody", "CraftTarget", "NearestVessel" })]
        public string sourceTargetString = "Self";
        public ShuntTarget sourceTarget = ShuntTarget.Self;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Sink", advancedTweakable = true), UI_ChooseOption(display = new string[] { "Self", "Celestial Body", "Craft Target", "Nearest Vessel" }, options = new string[] { "Self", "CelestialBody", "CraftTarget", "NearestVessel" })]
        public string sinkTargetString = "CelestialBody";
        public ShuntTarget sinkTarget = ShuntTarget.CelestialBody;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Src Dst", guiFormat = "F2"), UI_Label()]
        public float sourceDistance = 0f;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Snk Dst", guiFormat = "F2"), UI_Label()]
        public float sinkDistance = 0f;

        public bool sourceValid;
        public Forceable sourceForceable = new Forceable();

        public bool sinkValid;
        public Forceable sinkForceable = new Forceable();

        [KSPField]
        public string shortName = "Kinetic Shunt";

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUncommand = false, guiName = "Activate Kinetic Shunt", name = "ShuntToggle", requireFullControl = true)]
        public void ShuntToggle()
        {
            if (active)
            {
                DeactivateShunt();
            }
            else
            {
                ActivateShunt();
            }
        }

        public void ActivateShunt()
        {
            active = true;
            Events["ShuntToggle"].guiName = "Deactivate " + shortName;
        }

        public void DeactivateShunt()
        {
            active = false;
            Events["ShuntToggle"].guiName = "Activate " + shortName;
        }

        [KSPAction(guiName = "Toggle Kinetic Shunt", requireFullControl = true)]
        public void ActionToggleExcitationField(KSPActionParam actionParams)
        {
            ShuntToggle();
        }

        [KSPAction(guiName = "Activate Kinetic Shunt", requireFullControl = true)]
        public void ActionActivateExcitationField(KSPActionParam actionParams)
        {
            ActivateShunt();
        }

        [KSPAction(guiName = "Deactivate Kinetic Shunt", requireFullControl = true)]
        public void ActionDeactivateExcitationField(KSPActionParam actionParams)
        {
            DeactivateShunt();
        }

        public override void OnAwake()
        {
            Fields["shuntModeString"].uiControlEditor.onFieldChanged += ShuntModeStringChanged;
            Fields["shuntModeString"].uiControlFlight.onFieldChanged += ShuntModeStringChanged;

            Fields["sourceTargetString"].uiControlEditor.onFieldChanged += sourceTargetStringChanged;
            Fields["sourceTargetString"].uiControlFlight.onFieldChanged += sourceTargetStringChanged;

            Fields["sinkTargetString"].uiControlEditor.onFieldChanged += sinkTargetStringChanged;
            Fields["sinkTargetString"].uiControlFlight.onFieldChanged += sinkTargetStringChanged;

            base.OnAwake();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            updateShuntModeFromString();
            updateSourceTargetFromString();
            updateSinkTargetFromString();
            sourceForceable.setRanges((float)minForceloadRange);
            sinkForceable.setRanges((float)minForceloadRange);
            if(active)
                Events["ShuntToggle"].guiName = "Deactivate " + shortName;
            else
                Events["ShuntToggle"].guiName = "Activate " + shortName;
            Fields["sourceTargetString"].guiActive = allowSourceChange;
            Fields["sourceTargetString"].guiActiveEditor = allowSourceChange;
            Fields["sinkTargetString"].guiActive = allowSinkChange;
            Fields["sinkTargetString"].guiActiveEditor = allowSinkChange;
            Actions["ActionToggleExcitationField"].guiName = "Toggle " + shortName;
            Actions["ActionActivateExcitationField"].guiName = "Activate " + shortName;
            Actions["ActionDeactivateExcitationField"].guiName = "Deactivate " + shortName;
        }

        public override void OnActive()
        {
            base.OnActive();
            ActivateShunt();
        }

        private void ShuntModeStringChanged(BaseField field, object what)
        {
            updateShuntModeFromString();
        }

        public void updateShuntModeFromString()
        {
            shuntMode = (ShuntMode)Enum.Parse(typeof(ShuntMode), shuntModeString);
        }

        private void sourceTargetStringChanged(BaseField field, object what)
        {
            updateSourceTargetFromString();
        }

        public void updateSourceTargetFromString()
        {
            sourceTarget = (ShuntTarget)Enum.Parse(typeof(ShuntTarget), sourceTargetString);
        }

        private void sinkTargetStringChanged(BaseField field, object what)
        {
            updateSinkTargetFromString();
        }

        public void updateSinkTargetFromString()
        {
            sinkTarget = (ShuntTarget)Enum.Parse(typeof(ShuntTarget), sinkTargetString);
        }

        /*public Vessel getSink()
        {
            if (sinkTarget == ShuntTarget.CelestialBody)
            {
                return null;
            }
            else if (sinkTarget == ShuntTarget.CraftTarget && typeof(Vessel).IsInstanceOfType(vessel.targetObject))
            {
                return (Vessel)vessel.targetObject;
            }
            else if(sinkTarget == ShuntTarget.NearestVessel)
            {
                List<Vessel> sortedLoadedVessels = FlightGlobals.FindNearestVesselWhere(part.transform.position, vessel => vessel.loaded);
                if (sinkTarget == ShuntTarget.CraftTarget || sourceTarget == ShuntTarget.CraftTarget)
                {
                    if (sortedLoadedVessels.Count < 3)
                    {
                        return null;
                    }
                    else
                    {

                    }
                }
                    if (sortedLoadedVessels.Count < 2)
                {
                    return null;
                }
                else 
                {
                    return FlightGlobals.FindNearestVesselWhere(part.transform.position, vessel => vessel.loaded)[1];
                }
            }
        }*/

        /*public float getTargetDistance()
        {
            if(sinkTarget == ShuntTarget.CelestialBody)
            {
                return (float)vessel.radarAltitude;
            }
            else if(sinkTarget == ShuntTarget.CraftTarget)
            {
                return (getSink().transform.position - part.transform.position).magnitude;
            }
            else
            {
                return (FlightGlobals.FindNearestVesselWhere(part.transform.position, vessel => vessel.loaded)[1].transform.position - part.transform.position).magnitude;
            }
        }*/

        public void updateForceables()
        {
            if (sourceTarget == ShuntTarget.Self)
            {
                sourceValid = true;
                sourceForceable.wrap(part);
            }
            else if (sourceTarget == ShuntTarget.CelestialBody)
            {
                sourceValid = true;
                sourceForceable.wrap();
            }
            else if (sourceTarget == ShuntTarget.CraftTarget && typeof(Vessel).IsInstanceOfType(vessel.targetObject))
            {
                sourceValid = true;
                sourceForceable.wrap((Vessel)vessel.targetObject);
            }
            else if (sourceTarget == ShuntTarget.NearestVessel)
            {
                List<Vessel> sortedLoadedVessels = FlightGlobals.FindNearestVesselWhere(part.transform.position, vessel => vessel.loaded);
                if (sinkTarget == ShuntTarget.CraftTarget)
                {
                    if (sortedLoadedVessels.Count < 3)
                    {
                        sourceValid = false;
                    }
                    else
                    {
                        if (sortedLoadedVessels[1] != (Vessel)vessel.targetObject)
                        {
                            sourceValid = true;
                            sourceForceable.wrap(sortedLoadedVessels[1]);
                        }
                        else
                        {
                            sourceValid = true;
                            sourceForceable.wrap(sortedLoadedVessels[2]);
                        }
                    }
                }
                if (sortedLoadedVessels.Count < 2)
                {
                    sourceValid = false;
                }
                else
                {
                    sourceValid = true;
                    sourceForceable.wrap(sortedLoadedVessels[1]);
                }
            }
            else
            {
                sourceValid = false;
            }

            if (sinkTarget == ShuntTarget.Self)
            {
                sinkValid = true;
                sinkForceable.wrap(part);
            }
            else if (sinkTarget == ShuntTarget.CelestialBody)
            {
                sinkValid = true;
                sinkForceable.wrap();
            }
            else if (sinkTarget == ShuntTarget.CraftTarget && typeof(Vessel).IsInstanceOfType(vessel.targetObject))
            {
                sinkValid = true;
                sinkForceable.wrap((Vessel)vessel.targetObject);
            }
            else if (sinkTarget == ShuntTarget.NearestVessel)
            {
                List<Vessel> sortedLoadedVessels = FlightGlobals.FindNearestVesselWhere(part.transform.position, vessel => vessel.loaded);
                if (sourceTarget == ShuntTarget.CraftTarget)
                {
                    if (sortedLoadedVessels.Count < 3)
                    {
                        sinkValid = false;
                    }
                    else
                    {
                        if (sortedLoadedVessels[1] != (Vessel)vessel.targetObject)
                        {
                            sinkValid = true;
                            sinkForceable.wrap(sortedLoadedVessels[1]);
                        }
                        else
                        {
                            sinkValid = true;
                            sinkForceable.wrap(sortedLoadedVessels[2]);
                        }
                    }
                }
                if (sortedLoadedVessels.Count < 2)
                {
                    sinkValid = false;
                }
                else
                {
                    sinkValid = true;
                    sinkForceable.wrap(sortedLoadedVessels[1]);
                }
            }
            else
            {
                sinkValid = false;
            }
        }

        public void transferKineticEnergy(Forceable source, Forceable sink, Vector3 force)
        {
            source.addForce(force, ForceMode.Impulse);
            sink.addForce(-force, ForceMode.Impulse);
        }

        public void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                updateForceables();

                //Test to make sure we have a valid sink
                if (!sinkValid)
                {
                    thrustStatus = "No Sink";
                    torqueStatus = "No Sink";
                    this.sourceDistance = 0f;
                }
                else if (!sourceValid)
                {
                    thrustStatus = "No Source";
                    torqueStatus = "No Source";
                    this.sourceDistance = 0f;
                }
                else if (sinkForceable.Equals(sourceForceable))
                {
                    thrustStatus = "Source & Sink Same";
                    torqueStatus = "Source & Sink Same";
                }
                else if(active)
                {
                    if(!part.vessel.rootPart.Modules.Contains<ModuleRangeReverter>())
                        ModuleRangeReverter.SetVesselRanges(vessel, (float)minForceloadRange + 2000, (float)minForceloadRange + 10000, (float)minForceloadRange + 5000, (float)minForceloadRange, (float)minForceloadRange);
                    double EEAvailable;
                    double EEMax;
                    double maxEEUpdate = maxEESec * TimeWarp.fixedDeltaTime;
                    part.GetConnectedResourceTotals(Constants.EEDefinition.id, out EEAvailable, out EEMax);
                    if (EEAvailable < maxEEUpdate)
                        maxEEUpdate = EEAvailable;

                    double remainingEEUpdate = maxEEUpdate;

                    if (vessel.ctrlState.yaw != 0 || vessel.ctrlState.pitch != 0 || vessel.ctrlState.roll != 0)
                    {
                        double EETorqueCost = Math.Max(Math.Max(vessel.ctrlState.yaw, vessel.ctrlState.pitch), vessel.ctrlState.roll) * maxTorqueEE * TimeWarp.fixedDeltaTime;
                        double throttleLimit;

                        if (remainingEEUpdate < EETorqueCost)
                        {
                            throttleLimit = remainingEEUpdate / EETorqueCost;
                            torqueStatus = "Power Limited " + throttleLimit.ToString("P2");
                        }
                        else
                        {
                            throttleLimit = 1;
                            torqueStatus = "Full power";
                        }

                        part.Rigidbody.AddTorque(transform.forward * (float)maxTorque * -vessel.ctrlState.yaw * (float)throttleLimit * TimeWarp.fixedDeltaTime, ForceMode.Impulse);
                        part.Rigidbody.AddTorque(transform.right * (float)maxTorque * -vessel.ctrlState.pitch * (float)throttleLimit * TimeWarp.fixedDeltaTime, ForceMode.Impulse);
                        part.Rigidbody.AddTorque(transform.up * (float)maxTorque * -vessel.ctrlState.roll * (float)throttleLimit * TimeWarp.fixedDeltaTime, ForceMode.Impulse);
                    }

                    sourceDistance = sourceForceable.distanceFrom(part);
                    sinkDistance = sinkForceable.distanceFrom(part);

                    double EEDistanceCost = (EECostPerMeterCurve.Evaluate(sourceForceable.distanceFrom(part)) + EECostPerMeterCurve.Evaluate(sinkForceable.distanceFrom(part))) * TimeWarp.fixedDeltaTime;
                    remainingEEUpdate -= EEDistanceCost;

                    if (remainingEEUpdate > 0)
                    {
                        if (shuntMode == ShuntMode.ManualControl && vessel.ctrlState.mainThrottle > 0)
                        {
                            double EEThrustCost = vessel.ctrlState.mainThrottle * maxThrustEE * TimeWarp.fixedDeltaTime;
                            double throttleLimit;

                            if (remainingEEUpdate < EEThrustCost)
                            {
                                throttleLimit = remainingEEUpdate / EEThrustCost;
                                thrustStatus = "Power Limited " + throttleLimit.ToString("P2");
                            }
                            else
                            {
                                throttleLimit = 1;
                                thrustStatus = "Full power";
                            }

                            remainingEEUpdate -= EEThrustCost * throttleLimit;
                            part.RequestResource(Constants.EEDefinition.id, EEDistanceCost + EEThrustCost * throttleLimit);

                            //Accelerate in part's forward direction
                            Vector3 force = transform.up * (float)maxThrust * vessel.ctrlState.mainThrottle * (float)throttleLimit;
                            force *= TimeWarp.fixedDeltaTime;

                            transferKineticEnergy(sourceForceable, sinkForceable, force);
                        }
                        else if (shuntMode == ShuntMode.KillSurfaceVelocity)
                        {
                            Vector3 force = (sourceForceable.getSrfVelocity() + sourceForceable.getGraviticAcceleration() + sourceForceable.getCentrifugalAcceleration()) * (float)sourceForceable.getVesselMass() * -1;
                            if (force.magnitude > maxThrust)
                                force = force.normalized * (float)maxThrust;
                            force *= TimeWarp.fixedDeltaTime;

                            double EEThrustCost = (force.magnitude / maxThrust) * maxThrustEE;
                            double throttleLimit;

                            if (remainingEEUpdate < EEThrustCost)
                            {
                                throttleLimit = remainingEEUpdate / EEThrustCost;
                                thrustStatus = "Power Limited " + throttleLimit.ToString("P2");
                            }
                            else
                            {
                                throttleLimit = 1;
                                thrustStatus = "Full power";
                            }
                            force *= (float)throttleLimit;

                            remainingEEUpdate -= EEThrustCost * throttleLimit;
                            part.RequestResource(Constants.EEDefinition.id, EEDistanceCost + EEThrustCost * throttleLimit);

                            transferKineticEnergy(sourceForceable, sinkForceable, force);
                        }
                        else if (shuntMode == ShuntMode.KillOrbitalVelocity)
                        {
                            Vector3 force = (sourceForceable.getObtVelocity() + sourceForceable.getGraviticAcceleration() + sourceForceable.getCentrifugalAcceleration()) * (float)sourceForceable.getVesselMass() * -1;
                            if (force.magnitude > maxThrust)
                                force = force.normalized * (float)maxThrust;
                            force *= TimeWarp.fixedDeltaTime;

                            double EEThrustCost = (force.magnitude / maxThrust) * maxThrustEE;
                            double throttleLimit;

                            if (remainingEEUpdate < EEThrustCost)
                            {
                                throttleLimit = remainingEEUpdate / EEThrustCost;
                                thrustStatus = "Power Limited " + throttleLimit.ToString("P2");
                            }
                            else
                            {
                                throttleLimit = 1;
                                thrustStatus = "Full power";
                            }
                            force *= (float)throttleLimit;

                            remainingEEUpdate -= EEThrustCost * throttleLimit;
                            part.RequestResource(Constants.EEDefinition.id, EEDistanceCost + EEThrustCost * throttleLimit);

                            transferKineticEnergy(sourceForceable, sinkForceable, force);
                        }
                        else if (shuntMode == ShuntMode.KillRelativeVelocity)
                        {
                            if (sourceTarget == ShuntTarget.Self)
                            {
                                thrustStatus = "Invalid source";
                            }
                            else
                            {
                                Vector3 force;
                                if (sinkTarget != ShuntTarget.Self)
                                {
                                    force = (sourceForceable.getObtVelocity() - vessel.GetObtVelocity()) * (float)sourceForceable.getVesselMass() * -1;
                                }
                                else
                                {
                                    force = (sourceForceable.getObtVelocity() - vessel.GetObtVelocity()) * ((float)sourceForceable.getVesselMass() + vessel.GetTotalMass()) / 2 * -1;
                                }

                                if (force.magnitude > maxThrust)
                                    force = force.normalized * (float)maxThrust;
                                force *= TimeWarp.fixedDeltaTime;

                                double EEThrustCost = (force.magnitude / maxThrust) * maxThrustEE;
                                double throttleLimit;

                                if (remainingEEUpdate < EEThrustCost)
                                {
                                    throttleLimit = remainingEEUpdate / EEThrustCost;
                                    thrustStatus = "Power Limited " + throttleLimit.ToString("P2");
                                }
                                else
                                {
                                    throttleLimit = 1;
                                    thrustStatus = "Full power";
                                }
                                force *= (float)throttleLimit;

                                remainingEEUpdate -= EEThrustCost * throttleLimit;
                                part.RequestResource(Constants.EEDefinition.id, EEDistanceCost + EEThrustCost * throttleLimit);

                                transferKineticEnergy(sourceForceable, sinkForceable, force);
                            }
                        }
                        else
                        {
                            thrustStatus = "Inactive";
                        }
                    }
                    else
                    {
                        thrustStatus = "Max Distance Exceeded";
                        remainingEEUpdate += EEDistanceCost;
                    }
                }
                else
                {
                    thrustStatus = "Inactive";
                    torqueStatus = "Inactive";
                }
            }
        }

    }
}
