using UnityEngine;

namespace AutoSmartParts
{
    public class AutoGear : PartModule
    {
        /* if you had the autogear module to a part, it needs to have the modulelandinggear module as well. otherwise, bad thing may happen =p.
         * if you know how to hardcode the dependency, i'm all ears.
         */
        /* TODO:
         * Space plane inside a space plane?
         * when autogear off => reset gear to actiongroup state.
         * Activate only on selected body
         * faire les calculs que tout les x updates
         * Default / setting
         */
        /*???
         * Cas ou vessel est dock? 
         */
        /*Modif
         *Raycast layer mask 
         * 
         */
        #region attribut
        [KSPField(isPersistant = true)]
        public int LowerAltitude = 0;

        [KSPField(isPersistant = true)]
        public int RaiseAltitude = 0;

        [KSPField(isPersistant = true)]
        public bool raiseOverOcean = true;

        private double alt = 0;

        private double lastAlt = 0;

        private bool isLow = true;

        private bool onGround = true;

        [KSPField(isPersistant = true)]
        public bool AutoGearOn = true;

        private bool EditorOn = false;

        private int count = 0;
        #endregion

        #region methode
        private void Message(string message)
        {
            ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_CENTER);
        }

        private bool overOcean()
        {
            return FlightGlobals.ActiveVessel.pqsAltitude < 0;
        }
        #endregion

        #region GUI
        private Rect windowPos = new Rect();

        private void OnDraw()
        {
            windowPos = GUILayout.Window(this.part.GetInstanceID(), windowPos, OnWindow, "AutoGear Editor Part #" + (uint)this.part.GetInstanceID());
        }

        private void OnWindow(int windowId)
        {
            GUILayout.BeginHorizontal();
                GUILayout.Label("Lowering Altitude", GUILayout.Width(100f));
                LowerAltitude = (int)GUILayout.HorizontalSlider((float)LowerAltitude, 10, 1000, GUILayout.Width(100f));
                LowerAltitude = int.Parse(GUILayout.TextArea(LowerAltitude + "", 4, GUILayout.Width(40f)));
                GUILayout.Label("m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.Label("Raising Altitude", GUILayout.Width(100f));
                RaiseAltitude = (int)GUILayout.HorizontalSlider((float)RaiseAltitude, 10, 1000, GUILayout.Width(100f));
                RaiseAltitude = int.Parse(GUILayout.TextArea(RaiseAltitude + "", 4, GUILayout.Width(40f)));
                GUILayout.Label("m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.Label("Raise over Ocean");
                raiseOverOcean = GUILayout.Toggle(raiseOverOcean, "");
            GUILayout.EndHorizontal();

            //Debug
 
            GUILayout.BeginVertical();
                GUILayout.Label("Debug-------------------------", GUILayout.Width(300f));
                //GUILayout.Label("Ascending : " + ascending, GUILayout.Width(300f));
                GUILayout.Label("Altitude : " + alt, GUILayout.Width(300f));
                GUILayout.Label("count : " + count, GUILayout.Width(300f));/*
                GUILayout.Label("overOcean : " + oc, GUILayout.Width(300f));      
                GUILayout.Label("Vessel altitu : "+fgavalt, GUILayout.Width(300f));
                GUILayout.Label("Vessel pqsAlt : " + fgavpqsalt, GUILayout.Width(300f));
                */
            GUILayout.EndVertical();
  
            GUI.DragWindow();
        }
        #endregion

        #region tweakable
        public override string GetInfo()
        {
            return "\nAutomatic toggle link to the altitude\n";
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName = "Turn AutoGear off")] 
        public void ToggleAutoGear()// toggleable through action group
        {
            AutoGearOn = !AutoGearOn;
            Events["ToggleAutoGear"].guiName = (AutoGearOn ? "Turn AutoGear off" : "Turn AutoGear on");
            Events["ToggleEditor"].active = AutoGearOn;
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
            ToggleAutoGear();
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
            Events["ToggleAutoGear"].guiName = (AutoGearOn ? "Turn AutoGear off" : "Turn AutoGear on");
            if (state == StartState.Editor) 
            {
            }
            else
            {
                this.part.force_activate();
                switch ((int)((ModuleLandingGear)this.part.Modules["ModuleLandingGear"]).gearState)
                {
                    case 0:
                    case 3:
                    case 4: isLow = false; break;
                    case 1:
                    case 2: isLow = true; break;
                }
                onGround = true;
                // verifier Modules["ModuleLandingGear"] existe, sinon shutdown?  this.enabled = false ???
                //this.part.RemoveModule();
            }
            if (LowerAltitude == 0 && RaiseAltitude == 0)
            {
                LowerAltitude = 150;
                RaiseAltitude = 20;
            }
            
        }


        public override void OnFixedUpdate()//check every 10 update ?
        {
            bool onFlight = alt > LowerAltitude;
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
            switch ((int)((ModuleLandingGear)this.part.Modules["ModuleLandingGear"]).gearState)
            {
                case 0:
                case 3:
                case 4: isLow = false; break;
                case 1:
                case 2: isLow = true; break;
            }

            if ((FlightUIController.fetch.gears.currentState == 1 && AutoGearOn))
            {
                if ((int)this.vessel.situation < 3)
                    onGround = true;

                if (raiseOverOcean && overOcean())
                {
                    if (isLow)
                    {
                        ((ModuleLandingGear)this.part.Modules["ModuleLandingGear"]).RaiseLandingGear();
                        isLow = false;
                        onGround = false;
                    }
                }
                else
                {
                    //essayer de ne pas prendre en compte les petites variations entre lowalt et raialt i.e  more fuzzyness
                    if (isLow && alt > RaiseAltitude && !this.part.ShieldedFromAirstream)
                    {
                        if (onGround || alt > LowerAltitude || count > 150)
                        {
                            ((ModuleLandingGear)this.part.Modules["ModuleLandingGear"]).RaiseLandingGear();
                            isLow = false;
                            onGround = false;
                            count = 0;
                        }
                        else if(lastAlt < alt)//if ascending
                        {
                            count++;
                        }
                        else
                        {
                            count = 0;
                        }
                    }
                    else if (!isLow && alt < LowerAltitude)
                    {
                        if (lastAlt > alt)// if descendingss
                        {
                            count++;
                            if (count > 75 || onFlight)
                            {
                                ((ModuleLandingGear)this.part.Modules["ModuleLandingGear"]).LowerLandingGear();
                                isLow = true;
                                count = 0;
                            }
                        }
                        else
                        {
                            count = 0;
                        }
                    }
                }
            }
        }

#endregion

    }
}