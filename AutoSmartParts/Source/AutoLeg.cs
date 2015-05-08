using UnityEngine;

namespace AutoSmartParts
{
    public class AutoLeg : PartModule
    {

        /*TODO
         * what if landing leg is broken?
         * 
         */
        #region attribut
        [KSPField(isPersistant = true)]
        public int Altitude;

        [KSPField(isPersistant = true)]
        public bool raiseOverOcean = true;

        private double alt = 0;

        private double lastAlt = 0;

        private bool isLow = false;

        [KSPField(isPersistant = true)]
        public bool AutoLegOn = true;

        private bool EditorOn = false;

        #endregion

        #region methode
        private bool overOcean()
        {
            return FlightGlobals.ActiveVessel.pqsAltitude < 0;
        }
        private void Message(string message)
        {
            ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_CENTER);
        }
        #endregion

        #region GUI

        private Rect windowPos = new Rect();

        private void OnDraw()
        {
           windowPos = GUILayout.Window(this.part.GetInstanceID(), windowPos, OnWindow, "AutoLeg Editor Part #" + (uint)this.part.GetInstanceID());
        }
        private void OnWindow(int windowId)
        {
            GUILayout.BeginHorizontal();
                GUILayout.Label("Altitude", GUILayout.Width(100f));
                Altitude = (int)GUILayout.HorizontalSlider((float)Altitude, 10, 1000, GUILayout.Width(100f));
                Altitude = int.Parse(GUILayout.TextArea(Altitude + "", 4, GUILayout.Width(40f)));
                GUILayout.Label("m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.Label("Raise over Ocean");
                raiseOverOcean = GUILayout.Toggle(raiseOverOcean, "");
            GUILayout.EndHorizontal();
             
            GUI.DragWindow();
        }

        #endregion

        #region tweakable
        public override string GetInfo()
        {
            return "\nAutomatic toggle, link to the altitude\n";
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName = "Turn AutoLeg off")]
        public void ToggleAutoLeg()// toggleable through action group
        {
            AutoLegOn = !AutoLegOn;
            Events["ToggleAutoLeg"].guiName = (AutoLegOn ? "Turn AutoLeg off" : "Turn AutoLeg on");
            Events["ToggleEditor"].active = AutoLegOn;
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName = "    Turn Editor on")]
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
            ToggleAutoLeg();
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
            Events["ToggleAutoLeg"].guiName = (AutoLegOn ? "Turn AutoLeg off" : "Turn AutoLeg on");
            if (state == StartState.Editor)
            {
            }
            else
            {
                this.part.force_activate();
                switch ((int)((ModuleLandingLeg)this.part.Modules["ModuleLandingLeg"]).legState)
                {
                    case 0:
                    case 1: isLow = false; break;
                    case 2:
                    case 3: isLow = true; break;
                    case 4://broken
                    case 5: break;//repairing
                }
                // verifier Modules["ModuleLandingLeg"] existe, sinon shutdown?  this.enabled = false ???           
            }
            if (Altitude == 0)
                Altitude = 500;
        }

        public override void OnFixedUpdate()//check every 10 update ?
        {
            lastAlt = alt;

            if (FlightGlobals.ActiveVessel.heightFromTerrain < 100 && !overOcean()) // <10 because you don't need that much precision over 10m. and it avoid the go up and raycast go through you bug
            {
                RaycastHit pHit;
                Vector3 partEdge = this.part.collider.ClosestPointOnBounds(FlightGlobals.currentMainBody.position);
                Physics.Raycast(partEdge, FlightGlobals.ActiveVessel.mainBody.position, out pHit, (float)(FlightGlobals.ActiveVessel.mainBody.Radius + FlightGlobals.ActiveVessel.altitude), 33792);
                alt = pHit.distance;
            }
            else if (overOcean())
                alt = FlightGlobals.ActiveVessel.altitude;
            else
                alt = FlightGlobals.ActiveVessel.heightFromTerrain;

            //check de l'état pour eviter des bugs en cas de controle manuel
            switch ((int)((ModuleLandingLeg)this.part.Modules["ModuleLandingLeg"]).legState)
            {
                case 0:
                case 1: isLow = false; break;
                case 2:
                case 3: isLow = true; break;
                case 4://broken
                case 5: break;//repairing
            }

            if ((FlightUIController.fetch.gears.currentState == 1 && AutoLegOn))
            {
                if (raiseOverOcean && overOcean() && isLow)
                {
                    ((ModuleLandingLeg)this.part.Modules["ModuleLandingLeg"]).RaiseLeg();
                    isLow = false;
                }
                else
                {
                    if (isLow && alt > Altitude)
                    {
                            ((ModuleLandingLeg)this.part.Modules["ModuleLandingLeg"]).RaiseLeg();
                            isLow = false;
                    }
                    else if (!isLow && alt < Altitude)
                    {
                        if (lastAlt > alt && (int)this.vessel.situation > 2)// if descending and not landed
                        {
                            ((ModuleLandingLeg)this.part.Modules["ModuleLandingLeg"]).LowerLeg();
                            isLow = true;
                        }
                    }
                }
            }
        }
        #endregion

    }
}