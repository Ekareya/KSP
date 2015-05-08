using UnityEngine;

namespace AutoSmartParts
{
    class AutoDeploy : PartModule
    {
        /*TODO:
         * Deploy on Landing
         * Activate on parent activation
         */

        #region attribut
        private bool isRetracted = true;
        [KSPField(isPersistant = true)]
        public bool AutoDeployOn = true;
        #endregion

            //onDetach

        #region tweakable
        public override string GetInfo()
        {
            return "\nAutomatic toggle link to the altitude\n";
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName = "Turn AutoDeploy off")]
        public void ToggleAutoDeploy()// toggleable through action group
        {
            AutoDeployOn = !AutoDeployOn;
            Events["ToggleAutoDeploy"].guiName = (AutoDeployOn ? "Turn AutoDeploy off" : "Turn AutoDeploy on");
        }

        #endregion

        #region action
        [KSPAction("Toggle Automation")]
        public void actTg(KSPActionParam kap)
        {
            ToggleAutoDeploy();
        }
        #endregion

        #region pipeline
        public override void OnStart(PartModule.StartState state)
        {
            if(state != StartState.Editor)
            { 
                this.part.force_activate();
                switch ((int)((ModuleDeployableSolarPanel)this.part.Modules["ModuleDeployableSolarPanel"]).panelState)
                {
                    case 0:
                    case 2:
                    case 4: isRetracted = true; break;
                    case 1:
                    case 3: isRetracted = false; break;
                }
            }
        }
        
        public override void OnFixedUpdate()
        {
             switch ((int)((ModuleDeployableSolarPanel)this.part.Modules["ModuleDeployableSolarPanel"]).panelState)
             {
                 case 0:
                 case 2:
                 case 4: isRetracted = true; break;
                 case 1:
                 case 3: isRetracted = false; break;
              }

              double windResist = ((ModuleDeployableSolarPanel)this.part.Modules["ModuleDeployableSolarPanel"]).windResistance;
              double safetyZone = windResist - this.part.atmDensity * this.vessel.speed;
              if (safetyZone > 0.95 * windResist && this.part.atmDensity < 0.01 && isRetracted && !this.part.ShieldedFromAirstream)
              {
                  ((ModuleDeployableSolarPanel)this.part.Modules["ModuleDeployableSolarPanel"]).Extend();
              }

              else if (!isRetracted)
              {
                  ((ModuleDeployableSolarPanel)this.part.Modules["ModuleDeployableSolarPanel"]).Retract();
              }
        }
        #endregion

    }
}