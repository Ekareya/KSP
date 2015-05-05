using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoSmartParts
{
    public class AutoGear : PartModule
    {
        /* You if you had the autogear module to a part, it needs to have the modulelandinggear module as well. otherwise, bad thing may happen =p.
         * if you know how to hardcode the dependency, i'm all ears.
         */
        
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
            RaiseAltitude = int.Parse(GUILayout.TextArea(RaiseAltitude+"",4,GUILayout.Width(40f)));
            GUILayout.Label("m");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Raise over Ocean");
            Ocean = GUILayout.Toggle(Ocean, "" );
            GUILayout.EndHorizontal();
            
            //Debug
            GUILayout.BeginVertical();

            GUILayout.Label("Ascending : "+ascending,GUILayout.Width(300f));
            GUILayout.Label("Altitude : "+alt,GUILayout.Width(300f));
            GUILayout.Label("isLow : "+isLow,GUILayout.Width(300f));
            GUILayout.Label("onGround : "+onGround,GUILayout.Width(300f));
            GUILayout.Label("",GUILayout.Width(100f));
            GUILayout.Label("",GUILayout.Width(100f));
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
        
        [KSPField(isPersistant = true, guiName = "AutoGear")]
        public bool AutoGearOn = true;
        public bool EditorOn = false;
        public bool Ocean = false;
        //public bool InfoOn = false;

        [KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName = "Turn AutoGear off")] // make it togglabe through action group
        public void ToggleAutoGear()
        {
            AutoGearOn = !AutoGearOn;
            Events["ToggleAutoGear"].guiName = (AutoGearOn ? "Turn AutoGear off" : "Turn AutoGear on");
            Events["ToggleInfo"].active = AutoGearOn;
            Events["ToggleEditor"].active = AutoGearOn;
        }
        
        /*[KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName= "    Show AGInfo")]
        public void ToggleInfo()
        {
            InfoOn = !InfoOn;
            Events["ToggleInfo"].guiName = (InfoOn ? "    Hide AGInfo" : "    Show AGInfo");
            Message("L=" + Fields["AltitudeL"].guiActive);
            Message("R=" + Fields["AltitudeR"].guiActive);
            Fields["AltitudeL"].guiActive = InfoOn;
            Fields["AltitudeR"].guiActive = InfoOn;
            Fields["AltitudeL"].guiActiveEditor = InfoOn;
            Fields["AltitudeR"].guiActiveEditor = InfoOn;
        }*/

        [KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName = "    Turn AGEditor on" )]
        public void ToggleEditor()
        {
            EditorOn = !EditorOn;
            Events["ToggleEditor"].guiName = (EditorOn ? "    Turn AGEditor off" : "    Turn AGEditor on");
           
            if (EditorOn)
                RenderingManager.AddToPostDrawQueue(0, OnDraw);
            else
                RenderingManager.RemoveFromPostDrawQueue(0, OnDraw);
        }

        [KSPField(isPersistant = true, guiName = "AltitudeL")]
        public int LowerAltitude = 0;

        [KSPField(isPersistant = true, guiName = "AltitudeR", guiUnits = " m")]
        public int RaiseAltitude = 0;
       


        [KSPField(isPersistant = true, guiName = "Altitude", guiActive = true , guiUnits="m")]
        private double alt = 0;

        private double lastAlt = 0;

        
        [KSPField(isPersistant = true)]
         private Boolean ascending = false;

        [KSPField(isPersistant = true)]
        private Boolean isLow = true;

        [KSPField(isPersistant = true)]
        private Boolean onGround = true;
        
        public override string GetInfo()
        {
            return "\nContains the TAC Example - Simple Part Module\n";
        }

        private void Message(String message)
        {
            ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_CENTER);
        }

        public override void OnStart(StartState state)
        {
            
            isLow = true; //<= a definir mieux que ca avec le "start retracted " field
            onGround = true;
            // verifier Modules["ModuleLandingGear"] existe, sinon shutdown?
            //if altitude = 0
            if (LowerAltitude == 0 && RaiseAltitude == 0)
            {
                LowerAltitude = 150;
                RaiseAltitude = 20;
            }

            if (state == StartState.Editor) 
            {
                
            }
            
        }


        public override void OnFixedUpdate()
        {           
            if (FlightUIController.fetch.gears.currentState == 1 && AutoGearOn)
            {              
                lastAlt = alt;
                alt = dist2ground();
                 //coupler isLow et var truc = ((ModuleLandingGear)this.part.Modules["ModuleLandingGear"]).gearState; pour gerer quand on le bouge a la main.
               ascending = (lastAlt < alt ? true : false);
                    //essayer de ne pas prendre en compte les petites variations entre lowalt et raialt i.e  more fuzzyness
                if (isLow && /*ascending &&*/ alt > RaiseAltitude)
                {
                    if (onGround || alt > LowerAltitude) // what if you change the value inflight in the editor?
                    {
                        Message("Raising gear");
                        ((ModuleLandingGear)this.part.Modules["ModuleLandingGear"]).RaiseLandingGear();
                        isLow = false;
                        onGround = false;
                    }
                }
                else if (!ascending && !isLow && alt < LowerAltitude)
                {
                    Message("Lowering gear");
                    ((ModuleLandingGear)this.part.Modules["ModuleLandingGear"]).LowerLandingGear();
                    isLow = true;
                }
                if (alt < 1)
                     onGround = true;
            }
        }

        private double dist2ground()
        {
            RaycastHit pHit;
            Vector3 partEdge = this.part.collider.ClosestPointOnBounds(FlightGlobals.currentMainBody.position);
            Physics.Raycast(partEdge, FlightGlobals.ActiveVessel.mainBody.position, out pHit);
            double landHeight = pHit.distance;

            if (FlightGlobals.ActiveVessel.mainBody.ocean && landHeight > FlightGlobals.ActiveVessel.altitude) //if mainbody has ocean we land on water before the seabed
            {
                    landHeight = FlightGlobals.ActiveVessel.altitude;
            }

            return landHeight;
        }




        
        
        
    
       


        

    }
}
