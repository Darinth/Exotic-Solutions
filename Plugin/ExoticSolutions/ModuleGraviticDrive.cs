using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleGraviticDrive : PartModule
    {
        [KSPField(isPersistant = true)]
        bool active;

        //Actual maximum amount of tonnage part can lift.
        [KSPField]
        public float maxLiftTonnage = 100f;

        //EE cost per point of tonnage when initializing gravitic field
        [KSPField]
        public float EEPerLiftTonnage = 0.00083333333f;

        //EC cost per point of tonnage when actively lifting
        [KSPField]
        public float ECPerLiftTonnage = 5f;

        //Lift tonnage selected (used for both activating gravitic field and running it)
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Lift Tonnage"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        public float SelectedLiftTonnage = 0f;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Exotic Energy Usage", guiFormat = "F2"), UI_Label()]
        public double EEUsage = 0f;

        //Button to toggle gravitic field
        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUncommand = false, guiName = "Activate Gravitic Field", name = "GraviticFieldToggle", requireFullControl = true)]
        public void GraviticFieldToggle()
        {
            if (!active)
            {

                InitializeGraviticField();
            }
            else
            {

                ShutdownGraviticField();
            }
        }

        //Actions to toggle gravitic field
        [KSPAction(guiName = "Toggle Gravitic Field", requireFullControl = true)]
        public void ActionToggleGraviticField(KSPActionParam actionParams)
        {
            GraviticFieldToggle();
        }

        [KSPAction(guiName = "Activate Gravitic Field", requireFullControl = true)]
        public void ActionActivateGraviticField(KSPActionParam actionParams)
        {
            InitializeGraviticField();
        }

        [KSPAction(guiName = "Deactivate Gravitic Field", requireFullControl = true)]
        public void ActionDeactivateGraviticField(KSPActionParam actionParams)
        {
            ShutdownGraviticField();
        }

        public override void OnAwake()
        {
            Fields["SelectedLiftTonnage"].uiControlEditor.onFieldChanged += TonnageChanged;
            Fields["SelectedLiftTonnage"].uiControlFlight.onFieldChanged += TonnageChanged;

            base.OnAwake();

            updateEE();
        }

        public override void OnStart(StartState state)
        {
            updateEE();
            base.OnStart(state);

            ((UI_FloatRange)Fields["SelectedLiftTonnage"].uiControlEditor).minValue = -maxLiftTonnage;
            ((UI_FloatRange)Fields["SelectedLiftTonnage"].uiControlEditor).maxValue = maxLiftTonnage;
        }

        private void TonnageChanged(BaseField field, object what)
        {
                updateEE();
        }

        public void updateEE()
        {
            EEUsage = Math.Abs(SelectedLiftTonnage * EEPerLiftTonnage);
        }

        public void InitializeGraviticField()
        {
            active = true;
            Events["GraviticFieldToggle"].guiName = "Deactivate Gravitic Field";
        }

        public void ShutdownGraviticField()
        {
            active = false;
            Events["GraviticFieldToggle"].guiName = "Activate Gravitic Field";
        }

        public void FixedUpdate()
        {
            if(active)
            {
                if (SelectedLiftTonnage != 0)
                {
                    double driveLimit = 1;

                    double ECRequest = ECPerLiftTonnage * Math.Abs(SelectedLiftTonnage) * TimeWarp.fixedDeltaTime;
                    double AvailableEC, MaxEC;
                    this.part.GetConnectedResourceTotals(Constants.ECDefinition.id, out AvailableEC, out MaxEC);
                    if (ECRequest > AvailableEC)
                        driveLimit = AvailableEC / ECRequest;

                    double EERequest = EEPerLiftTonnage * Math.Abs(SelectedLiftTonnage) * TimeWarp.fixedDeltaTime;
                    double AvailableEE, MaxEE;
                    this.part.GetConnectedResourceTotals(Constants.EEDefinition.id, out AvailableEE, out MaxEE);
                    if (EERequest > AvailableEE)
                        driveLimit = Math.Min(AvailableEE / EERequest, driveLimit);

                    this.part.RequestResource(Constants.ECDefinition.id, ECRequest * driveLimit);
                    this.part.RequestResource(Constants.EEDefinition.id, EERequest * driveLimit);

                    Vector3 acceleration = vessel.graviticAcceleration * vessel.gravityMultiplier / part.mass * TimeWarp.fixedDeltaTime * SelectedLiftTonnage * -1 * driveLimit;
                    part.Rigidbody.AddForce(acceleration, ForceMode.VelocityChange);
                }
            }
        }

    }
}
