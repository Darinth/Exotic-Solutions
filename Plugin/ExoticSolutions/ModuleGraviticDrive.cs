using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleGraviticDrive : PartModule
    {
        //Actual maximum amount of tonnage part can lift.
        [KSPField]
        public float maxLiftTonnage = 100f;

        //EE cost per point of tonnage when initializing gravitic field
        [KSPField]
        public float EEPerLiftTonnage = 1f;

        //EC cost per point of tonnage when actively lifting
        [KSPField]
        public float ECPerLiftTonnage = 5f;

        //Max tonnage when gravitic field was initialized
        public float ActivatedMaxLiftTonnage = 200f;

        //Lift tonnage selected (used for both activating gravitic field and running it)
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Max Lift Tonnage"), UI_ScaleEdit(intervals = new float[] { 0, 100 }, incrementSlide = new float[] { 1 })]
        public float SelectedLiftTonnage = 200f;

        //Time, in seconds, of the gravitic field
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Exotic Excitation Field Time"), UI_Label()]
        private double EETime;

        //EE required to activate gravitic field
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Exotic Energy Required", guiFormat = "F2"), UI_Label()]
        public double EERequired = 0f;

        //Button to toggle gravitic field
        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUncommand = false, guiName = "Activate Gravitic Field", name = "GraviticFieldToggle", requireFullControl = true)]
        public void GraviticFieldToggle()
        {
            if (EETime <= 0)
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
            Fields["SelectedLiftTonnage"].uiControlEditor.onFieldChanged += ThrustOrISPChanged;
            Fields["SelectedLiftTonnage"].uiControlFlight.onFieldChanged += ThrustOrISPChanged;

            ((UI_ScaleEdit)Fields["SelectedLiftTonnage"].uiControlEditor).intervals[1] = maxLiftTonnage;

            base.OnAwake();

            updateEE();
        }

        public override void OnStart(StartState state)
        {
            updateEE();
            base.OnStart(state);
        }

        private void ThrustOrISPChanged(BaseField field, object what)
        {
            if(EETime <= 0)
                updateEE();
        }

        public void updateEE()
        {
            EERequired = SelectedLiftTonnage * EEPerLiftTonnage;
            if (EERequired < 1f) EERequired = 0.1 + Math.Pow(0.9, Math.Abs(EERequired - 2f));
        }

        public void InitializeGraviticField()
        {
            double EEAvailable;
            double EEMax;
            part.GetConnectedResourceTotals(Constants.EEDefinition.id, out EEAvailable, out EEMax, true);

            if (EEAvailable >= EERequired - 0.01)
            {
                part.RequestResource(Constants.EEDefinition.id, EERequired - 0.01);
                Events["GraviticFieldToggle"].guiName = "Deactivate Gravitic Field";
                ActivatedMaxLiftTonnage = SelectedLiftTonnage;
                Fields["SelectedLiftTonnage"].guiName = "Active Lift Tonnage";
                ((UI_ScaleEdit)Fields["SelectedLiftTonnage"].uiControlEditor).intervals[0] = -ActivatedMaxLiftTonnage;
                ((UI_ScaleEdit)Fields["SelectedLiftTonnage"].uiControlEditor).intervals[1] = ActivatedMaxLiftTonnage;
                SelectedLiftTonnage = 0f;
                EETime = 600f;
            }
        }

        public void ShutdownGraviticField()
        {
            Fields["SelectedLiftTonnage"].guiName = "Max Lift Tonnage";
            ((UI_ScaleEdit)Fields["SelectedLiftTonnage"].uiControlEditor).intervals[0] = 0;
            ((UI_ScaleEdit)Fields["SelectedLiftTonnage"].uiControlEditor).intervals[1] = maxLiftTonnage;
            SelectedLiftTonnage = ActivatedMaxLiftTonnage;
            Events["ExcitationFieldToggle"].guiName = "Activate Gravitic Field";
            EETime = 0f;
        }

        public void FixedUpdate()
        {
            if (EETime > 0f)
            {
                if (SelectedLiftTonnage != 0)
                {
                    double ECRequest = ECPerLiftTonnage * Math.Abs(SelectedLiftTonnage) * TimeWarp.fixedDeltaTime;
                    double returnedEC = this.part.RequestResource(Constants.ECDefinition.id, ECRequest);
                    Vector3 acceleration = vessel.graviticAcceleration * vessel.gravityMultiplier / part.mass * TimeWarp.fixedDeltaTime * SelectedLiftTonnage * -1 * (returnedEC / ECRequest);
                    part.Rigidbody.AddForce(acceleration, ForceMode.VelocityChange);
                }
                EETime -= TimeWarp.fixedDeltaTime;
                if (EETime < 0f)
                {
                    EETime = 0f;
                    ShutdownGraviticField();
                }
            }
        }

    }
}
