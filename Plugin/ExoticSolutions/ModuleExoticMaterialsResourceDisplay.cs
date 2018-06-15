using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExoticSolutions
{
    class ModuleExoticMaterialsResourceDisplay : PartModule
    {
        [KSPField(guiActive = false, guiActiveEditor = true, guiName = "AvailableEM", guiFormat = "F2"), UI_Label()]
        public float AvailableEM = 0f;

        public override void OnInitialize()
        {
            base.OnInitialize();
            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                AvailableEM = (float)Constants.exoticSolutionScenario.storedExoticMatter;
            }
        }

    }
}
