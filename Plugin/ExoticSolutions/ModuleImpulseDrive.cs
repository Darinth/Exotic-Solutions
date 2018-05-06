using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//click side arrows to cycle between options
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_ChooseOptionTest", advancedTweakable = true), UI_ChooseOption(display = new string[] { "Display1", "Display2", "Display3" }, options = new string[] { "Option1", "Option2", "Option3" })]
//public string UI_ChooseOptionTest = "Option1";

//Click button to cycle between options
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_CycleTest", advancedTweakable = true), UI_Cycle(stateNames = new string[] { "Option1", "Option2", "Option3" })]
//public int UI_CycleTest = 1;

//[KSPField]
//public double MaxValue = 50d;

//[KSPField]
//public double MinValue = 0d;

//Defunct? Couldn't get it to show up
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_FieldFloatRangeTest", advancedTweakable = true), UI_FieldFloatRange(minValue = "MaxValue", maxValue = "MinValue")]
//public float UI_FieldFloatRangeTest = 92f;

//Modify floating point values with bar or side arrows. Bar doesn't seem to function properly.
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_FloatEditTest", advancedTweakable = true), UI_FloatEdit(minValue = 0f, maxValue = 100f, incrementSmall = 1, incrementSlide = 2, incrementLarge = 3)]
//public float UI_FloatEditTest = 92f;

//Scrollable floating point editor bar.
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_FloatRangeTest", advancedTweakable = true), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
//public float UI_FloatRangeTest = 92f;

//Presents a label
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_LabelTest", advancedTweakable = true), UI_Label()]
//public string UI_LabelTest = "Stuff";

//Display bar like UI_FloatRange, but not modifiable
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_ProgressBarTest", advancedTweakable = true), UI_ProgressBar(minValue = 85f, maxValue = 100f)]
//public float UI_ProgressBarTest = 92f;

//Scaleable UI Float editor it looks like. Probably good for making very fine-tuneable floating point editors.
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_ScaleEditTest", advancedTweakable = true), UI_ScaleEdit()]
//public float UI_ScaleEditTest = 1f;

//Toggle button. On/off, true/false, enabled/disabled
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_ToggleTest", advancedTweakable = true), UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
//public bool UI_ToggleTest = true;

//Select variant versions of a part
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_VariantSelectorTest", advancedTweakable = true), UI_VariantSelector()]
//public int UI_VariantSelectorTest = true;

//Defunct? Couldn't get it to show up
//[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "UI_Vector2Test", advancedTweakable = true), UI_Vector2()]
//public Vector3d UI_Vector2Test = new Vector3d(0,0);


namespace ExoticSolutions
{
    class ModuleImpulseDrive : ModuleEnginesFX
    {
        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUncommand = true, guiName = "Log G-Force", name = "LogGeeForce", requireFullControl = false)]
        public void LogGeeForce()
        {
            KSPLog.print("G-Force: " + vessel.geeForce + ", " + vessel.geeForce_immediate);
        }

        //Thrust/ISP/this number provides you the with the fuel flow needed to achieve the selected thrust.
        private double ThrustIspToFuelFlowConstant;
        //Max fuel flow of base engine.
        private float BaseMaxFuelflow;
        //Max thrust of base engine
        private float BaseMaxThrust;
        //Atmo curve of base engine
        private FloatCurve BaseAtmosphereCurve;

        //Time, in seconds, of the excitation field if the engine does not run
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Exotic Excitation Field Time"), UI_Label()]
        private double EETime;

        //Minimum thrust available in variable drive
        [KSPField]
        public float MinVariableThrust = 1;

        //Maximum thrust available in variable drive
        [KSPField]
        public float MaxVariableThrust = 5000;

        //Minimum ISP available in variable drive
        [KSPField]
        public float MinVariableISP = 100;

        //Maximum ISP available in variable drive
        [KSPField]
        public float MaxVariableISP = 5000;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Thrust"), UI_ScaleEdit(intervals = new float[] { 0, 100 },incrementSlide = new float[] { 50 })]
        public float SelectedThrust = 200f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "ISP"), UI_ScaleEdit(intervals = new float[] { 0, 100 }, incrementSlide = new float[] { 50 })]
        public float SelectedISP = 350f;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Exotic Energy Required", guiFormat = "F2"), UI_Label()]
        //public string EERequiredText = "";
        public double EERequired = 0f;

        [KSPField]
        public double EEMultiplier = 1f;

        [KSPField]
        public float EEToThrustRatio = 200;

        [KSPField]
        public float EEToISPRatio = 300;

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUncommand = false, guiName = "Activate Exotic Excitation Field", name = "ExcitationFieldToggle", requireFullControl = true)]
        public void ExcitationFieldToggle()
        {
            if (EETime <= 0)
            {

                InitializeExoticExcitationField();
            }
            else
            {

                ShutdownExoticExcitationField();
            }
        }

        [KSPAction(guiName = "Toggle Excitation Field", requireFullControl = true)]
        public void ActionToggleExcitationField(KSPActionParam actionParams)
        {
            ExcitationFieldToggle();
        }

        [KSPAction(guiName = "Activate Excitation Field", requireFullControl = true)]
        public void ActionActivateExcitationField(KSPActionParam actionParams)
        {
            InitializeExoticExcitationField();
        }

        [KSPAction(guiName = "Deactivate Excitation Field", requireFullControl = true)]
        public void ActionDeactivateExcitationField(KSPActionParam actionParams)
        {
            ShutdownExoticExcitationField();
        }

        public override void OnAwake()
        {
            Fields["SelectedThrust"].uiControlEditor.onFieldChanged += ThrustOrISPChanged;
            Fields["SelectedThrust"].uiControlFlight.onFieldChanged += ThrustOrISPChanged;

            Fields["SelectedISP"].uiControlEditor.onFieldChanged += ThrustOrISPChanged;
            Fields["SelectedISP"].uiControlFlight.onFieldChanged += ThrustOrISPChanged;

            ((UI_ScaleEdit)Fields["SelectedThrust"].uiControlEditor).intervals[0] = MinVariableThrust;
            ((UI_ScaleEdit)Fields["SelectedThrust"].uiControlEditor).intervals[1] = MaxVariableThrust;
            ((UI_ScaleEdit)Fields["SelectedThrust"].uiControlEditor).incrementSlide[0] = EEToThrustRatio;

            ((UI_ScaleEdit)Fields["SelectedISP"].uiControlEditor).intervals[0] = MinVariableISP;
            ((UI_ScaleEdit)Fields["SelectedISP"].uiControlEditor).intervals[1] = MaxVariableISP;
            ((UI_ScaleEdit)Fields["SelectedISP"].uiControlEditor).incrementSlide[0] = EEToISPRatio;

            base.OnAwake();

            BaseMaxFuelflow = maxFuelFlow;
            BaseAtmosphereCurve = atmosphereCurve;
            BaseMaxThrust = maxThrust;

            ThrustIspToFuelFlowConstant = maxThrust / maxFuelFlow / BaseAtmosphereCurve.Evaluate(0);

            KSPLog.print("EEUpdate from OnAwake");
            updateEE();
        }

        public override void OnStart(StartState state)
        {
            KSPLog.print("EEUpdate from OnStart");
            updateEE();
            //SelectedThrust = BaseMaxThrust;
            //SelectedISP = BaseAtmosphereCurve.Evaluate(0);
            resetEngineParams();
            base.OnStart(state);
        }

        private void ThrustOrISPChanged(BaseField field, object what)
        {
            KSPLog.print("ThrustOrISPChanged");
            updateEE();
            if (HighLogic.LoadedSceneIsEditor)
                adjustEngineParams(SelectedThrust, SelectedISP);
        }

        public void updateEE()
        {
            float thrustChange = SelectedThrust - BaseMaxThrust;
            double thrustCost = thrustChange / EEToThrustRatio;
            float ISPChange = SelectedISP - BaseAtmosphereCurve.Evaluate(0);
            double ISPCost = ISPChange / EEToISPRatio;
            EERequired = (thrustCost + ISPCost) * EEMultiplier;
            if (EERequired < 1f) EERequired = 0.1 + Math.Pow(0.9, Math.Abs(EERequired - 2f));
            //EERequiredText = EERequired.ToString();
            KSPLog.print("Update EERequired to " + EERequired);
        }

        public void InitializeExoticExcitationField()
        {

            double EEAvailable;
            double EEMax;
            part.GetConnectedResourceTotals(Constants.EEDefinition.id, out EEAvailable, out EEMax, true);

            if (EEAvailable >= EERequired - 0.01)
            {
                part.RequestResource(Constants.EEDefinition.id, EERequired - 0.01);
                KSPLog.print("Rename");
                Events["ExcitationFieldToggle"].guiName = "Deactivate Exotic Excitation Field";
                adjustEngineParams(SelectedThrust, SelectedISP);
                KSPLog.print("Set EETime");
                EETime = 180f;
            }
        }

        public void ShutdownExoticExcitationField()
        {
            Events["ExcitationFieldToggle"].guiName = "Activate Exotic Excitation Field";
            resetEngineParams();
            EETime = 0f;
        }

        public new void FixedUpdate()
        {
            if (EETime > 0f)
            {
                EETime -= TimeWarp.fixedDeltaTime * (1f - requestedThrottle * 2);
                if (EETime <= 0f)
                {
                    ShutdownExoticExcitationField();
                }
            }
            base.FixedUpdate();
        }

        private void adjustEngineParams(float newThrust, float newISP)
        {
            KSPLog.print("set MaxThrust " + newThrust);
            maxFuelFlow = (float)(newThrust / newISP / ThrustIspToFuelFlowConstant);

            KSPLog.print("Make new atmo curve " + newISP);
            float frameMultiplier = newISP / BaseAtmosphereCurve.Evaluate(0);
            FloatCurve newAtmosphereCurve = new FloatCurve();
            AnimationCurve innerCurve = newAtmosphereCurve.Curve;
            foreach (Keyframe frame in BaseAtmosphereCurve.Curve.keys)
            {
                Keyframe newframe = new Keyframe(frame.time, frame.value * frameMultiplier);
                KSPLog.print("New frame: " + newframe.time + " " + newframe.value);
                innerCurve.AddKey(newframe);
            }
            KSPLog.print("Set new atmo curve");
            atmosphereCurve = newAtmosphereCurve;
            KSPLog.print("ISP at 0" + atmosphereCurve.Evaluate(0));
            KSPLog.print("ISP at 1" + atmosphereCurve.Evaluate(1));
        }

        private void resetEngineParams()
        {
            maxFuelFlow = BaseMaxFuelflow;
            atmosphereCurve = BaseAtmosphereCurve;
        }
    }
}
