using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Components
{
    public class ShipLogStarChartMode : ShipLogMode
    {
        public override bool AllowCancelInput()
        {
            return true;
        }

        public override bool AllowModeSwap()
        {
            return true;
        }

        public override void EnterMode(string entryID = "", List<ShipLogFact> revealQueue = null)
        {

        }

        public override void ExitMode()
        {

        }

        public override string GetFocusedEntryID()
        {
            return "0";
        }

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            
        }

        public override void OnEnterComputer()
        {

        }

        public override void OnExitComputer()
        {

        }

        public override void UpdateMode()
        {

        }
    }
}
