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
        //Thrust/ISP/this number provides you the with the fuel flow needed to achieve the selected thrust.
        private double ThrustIspToFuelFlowConstant;
        //Max fuel flow of base engine.
        private float BaseMaxFuelflow;
        //Max thrust of base engine
        private float BaseMaxThrust;
        //Atmo curve of base engine
        private FloatCurve BaseAtmosphereCurve;
        //Density of fuel mixture
        private float fuelRatioMultiplier;

        //Isp of engine operating in pure LfO mode
        float baseLfOIsp = 310;

        [KSPField]
        public float maxEEFlow = 0.08f;

        //Maximum ISP available in variable drive
        [KSPField]
        public float MaxVariableISP = 5000;

        [KSPField]
        public float VariableIspStep = 50;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "ISP"), UI_ScaleEdit(intervals = new float[] { 0, 100 }, incrementSlide = new float[] { 50 })]
        public float SelectedISP = 350f;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Max EE Flow", guiFormat = "F4"), UI_Label()]
        public float EEFlow = 0f;

        public override void OnAwake()
        {
            Fields["SelectedISP"].uiControlEditor.onFieldChanged += IspChanged;
            Fields["SelectedISP"].uiControlFlight.onFieldChanged += IspChanged;

            base.OnAwake();

        }

        public override void OnStart(StartState state)
        {
            ((UI_ScaleEdit)Fields["SelectedISP"].uiControlEditor).intervals[0] = atmosphereCurve.Evaluate(0);
            ((UI_ScaleEdit)Fields["SelectedISP"].uiControlEditor).intervals[1] = MaxVariableISP;
            ((UI_ScaleEdit)Fields["SelectedISP"].uiControlEditor).incrementSlide[0] = VariableIspStep;

            BaseMaxFuelflow = maxFuelFlow;
            BaseAtmosphereCurve = atmosphereCurve;
            BaseMaxThrust = maxThrust;

            float fuelMixtureDensity = 0;
            foreach (Propellant p in propellants)
            {
                PartResourceDefinition partResource = PartResourceLibrary.Instance.GetDefinition(p.id);
                fuelMixtureDensity += partResource.density * p.ratio;
            }
            //KSPLog.print("fuelMixtureDensity: " + fuelMixtureDensity);
            fuelRatioMultiplier = maxFuelFlow / fuelMixtureDensity;
            //KSPLog.print("fuelRatioMultiplier: " + fuelRatioMultiplier);

            ThrustIspToFuelFlowConstant = maxThrust / maxFuelFlow / BaseAtmosphereCurve.Evaluate(0);

            baseLfOIsp = BaseAtmosphereCurve.Evaluate(0);

            adjustEngineParams(SelectedISP);

            base.OnStart(state);
        }

        private void IspChanged(BaseField field, object what)
        {
            //KSPLog.print("ThrustOrISPChanged");
            adjustEngineParams(SelectedISP);
        }


        /*[KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUncommand = false, guiName = "Activate Kinetic Shunt", name = "ShuntToggle", requireFullControl = true)]
        public void logEngineData()
        {
            KSPLog.print("maxFuelFlow: " + maxFuelFlow);
            foreach(Propellant p in propellants)
            {
                KSPLog.print(p.name + " id: " + p.id);
                KSPLog.print(p.name + " ratio: " + p.ratio);
                KSPLog.print(p.name + " getMaxFuelFlow: " + getMaxFuelFlow(p));
                KSPLog.print(p.name + " name from id: " + PartResourceLibrary.Instance.GetDefinition(p.id).name);
            }
        }*/

        //EE ISP 2500
        //newISP = (float)(newThrust/maxFuelFlow/ThrustIspToFuelFlowConstant);
        private void adjustEngineParams(float newISP)
        {
            //Calculate amount of EE needed to achieve new Isp & thrust
            float LfOIspDifference = newISP - baseLfOIsp;
            float MaxIspDifference = MaxVariableISP - baseLfOIsp;
            float EMLfORatio = LfOIspDifference / MaxIspDifference * maxEEFlow / fuelRatioMultiplier; //1:1 mass ratio??

            ConfigNode loadNode = new ConfigNode();
            ConfigNode newLfNode = loadNode.AddNode("PROPELLANT");
            newLfNode.AddValue("name", "LiquidFuel");
            newLfNode.AddValue("ratio", 0.9f);
            ConfigNode newOxNode = loadNode.AddNode("PROPELLANT");
            newOxNode.AddValue("name", "Oxidizer");
            newOxNode.AddValue("ratio", 1.1f);
            if (EMLfORatio > 0.0001f)
            {
                ConfigNode newEENode = loadNode.AddNode("PROPELLANT");
                newEENode.AddValue("name", "ExoticEnergies");
                newEENode.AddValue("ratio", EMLfORatio);
            }
            this.Load(loadNode);

            if(EMLfORatio > 0.0001f)
            {
                EEFlow = getMaxFuelFlow(propellants[2]);
            }
            else
                EEFlow = 0f;


            //KSPLog.print("Make new atmo curve " + newISP);
            //float frameMultiplier = newISP / BaseAtmosphereCurve.Evaluate(0);
            FloatCurve newAtmosphereCurve = new FloatCurve();
            AnimationCurve innerCurve = newAtmosphereCurve.Curve;
            foreach (Keyframe frame in BaseAtmosphereCurve.Curve.keys)
            {
                Keyframe newframe = new Keyframe(frame.time, frame.value + LfOIspDifference);
                //KSPLog.print("New frame: " + newframe.time + " " + newframe.value);
                innerCurve.AddKey(newframe);
            }
            //KSPLog.print("Set new atmo curve");
            atmosphereCurve = newAtmosphereCurve;
            //KSPLog.print("ISP at 0 atm: " + atmosphereCurve.Evaluate(0));
            //KSPLog.print("ISP at 1 atm: " + atmosphereCurve.Evaluate(1));
        }
    }
}
