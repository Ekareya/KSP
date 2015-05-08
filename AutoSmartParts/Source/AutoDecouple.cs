using UnityEngine;

namespace AutoSmartParts
{
    class AutoDecouple : PartModule
    {
        #region attribut
        private bool EditorOn = false;
        [KSPField(isPersistant = true)]
        public bool AutoDecoupleOn = true;

        #endregion

        #region GUI
        private Rect windowPos = new Rect();

        private void OnDraw()
        {
            windowPos = GUILayout.Window(this.part.GetInstanceID(), windowPos, OnWindow, "AutoGear Editor Part #" + (uint)this.part.GetInstanceID());
        }

        private void OnWindow(int windowId)
        {
            //Debug
            //this.part.RequestFuel
            GUILayout.BeginVertical();
            GUILayout.Label("Debug-------------------------", GUILayout.Width(300f));
            GUILayout.Label("child : " + this.part.children, GUILayout.Width(300f));
            foreach(var truc in this.part.children)
            {GUILayout.Label("   : " + truc, GUILayout.Width(300f));}
           // GUILayout.Label("fuelrequest : " + this.part.RequestResource("LiquidFuel",0.000001), GUILayout.Width(300f));
            GUILayout.Label("parent : " + this.part.parent, GUILayout.Width(300f));
            GUILayout.Label("separationIndex : " + this.part.separationIndex, GUILayout.Width(300f));
           /* GUILayout.Label(" : " + this.part., GUILayout.Width(300f));
            GUILayout.Label(" : " + this.part., GUILayout.Width(300f));
            GUILayout.Label(" : " + this.part., GUILayout.Width(300f));
            */
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
        #endregion

        #region tweakable
        public override string GetInfo()
        {
            return "\n\n";
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName = "Turn  AutoDecouple off")]
        public void ToggleAutoDecouple()// toggleable through action group
        {
            AutoDecoupleOn = !AutoDecoupleOn;
            Events["ToggleAutoDecouple"].guiName = (AutoDecoupleOn ? "Turn AutoDecouple off" : "Turn AutoDecouple on");
            Events["ToggleEditor"].active = AutoDecoupleOn;
        }
        
        [KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName = "    Turn Editor on" )]
        public void ToggleEditor() // toggleable through action group
        {
            EditorOn = !EditorOn;
            Events["ToggleEditor"].guiName = (EditorOn ? "    Turn Editor off" : "    Turn Editor on");
           
            if (EditorOn)
                RenderingManager.AddToPostDrawQueue(0, OnDraw);
            else
                RenderingManager.RemoveFromPostDrawQueue(0, OnDraw);
        }

        #endregion
        
        #region action
        [KSPAction("Toggle Automation")]
        public void actTg(KSPActionParam kap)
        {
            ToggleAutoDecouple();
        }

        [KSPAction("Toggle Editor")]
        public void actTgEd(KSPActionParam kap)
        {
            ToggleEditor();
        }
        #endregion

        #region pipeline
        public override void OnStart(StartState state)
        {
            Events["ToggleAutoDecouple"].guiName = (AutoDecoupleOn ? "Turn AutoDecouple off" : "Turn AutoDecouple on");

            if (state == StartState.Editor)
            {
                this.part.force_activate();
            }
        }
        public override void OnFixedUpdate()
        {
            if (this.part.RequestResource("LiquidFuel", 0.000001) == 0 && this.part.RequestResource("Oxydiser", 0.000001) == 0) 
            {
                ((ModuleDecouple)this.part.Modules["ModuleDecouple"]).Decouple();
                ((ModuleAnchoredDecoupler)this.part.Modules["ModuleAnchoredDecoupler"]).Decouple();
            }
        }
        #endregion
    }
}
