using System.Linq;
using UnityEngine;
namespace AutoSmartParts
{
    class ModuleHinge : PartModule
    {
        #region attribut
        [KSPField]
        public string animationName = "test";

        private Animation anim;
        private ConfigurableJoint joint = null;
        private AttachNode node = null;
        private Part sibling = null;
        private bool partAttached = false;
        private bool isHost = false;
        private bool OnFirstUpdate = true;
        #endregion

        #region tweakable
        public override string GetInfo()
        {
            return "\n\n";
        }

        

        [KSPField(isPersistant = true)]
        public bool ModuleHingeOn = true;
        [KSPEvent(guiActive = true, guiActiveEditor = true, active = true, guiName = "extend")]
        public void ToggleModuleHinge()// toggleable through action group
        {
            ModuleHingeOn = !ModuleHingeOn;
            anim[animationName].speed *= -1f;
            if (anim[animationName].speed < 0 && anim[animationName].time == 0)
                anim[animationName].time = anim[animationName].length;
            anim.Play(animationName);

            Events["ToggleModuleHinge"].guiName = (ModuleHingeOn ? "extend" : "retract");
        }


        #endregion

        #region action
        [KSPAction("Toggle Automation")]
        public void actTg(KSPActionParam kap)
        {
            ToggleModuleHinge();
        }

        #endregion

        #region pipeline
        public override void OnStart(StartState state)
        {
            Events["ToggleModuleHinge"].guiName = (ModuleHingeOn ? "extend" : "retract");
            anim = part.FindModelAnimators(animationName).FirstOrDefault();
            if (anim == null)
                   Events["ToggleModuleHinge"].guiActive = false;
            else
            {
                if (state == StartState.Editor)
                {
                    
                }
                else
                {
                    this.part.force_activate();
                    anim[animationName].speed *= -1f;

                    foreach (var no in part.attachNodes)
                    {
                        if (no.position != Vector3.zero)
                            node = no;

                    }
                    if (node == null)
                        return;
                    partAttached = (node.attachedPart != null);
                    sibling = node.attachedPart;
                }
            }
        }


        private float getL(float r, float theta)
        {
            return 2 * r * Mathf.Sin(theta);
        }

        public override void OnFixedUpdate()
        {
            if (OnFirstUpdate)
            {
                if (partAttached && sibling != null)
                {
                    if ((sibling.attachJoint.Host == sibling || sibling.attachJoint.Host == part) && (sibling.attachJoint.Target == sibling || sibling.attachJoint.Target == part))
                    {
                        joint = sibling.attachJoint.Joint;
                        isHost = (sibling.attachJoint.Host == part);
                    }
                    else if ((part.attachJoint.Host == sibling || part.attachJoint.Host == part) && (part.attachJoint.Target == sibling || part.attachJoint.Target == part))
                    {
                        joint = part.attachJoint.Joint;
                        isHost = (part.attachJoint.Host == part);
                    }
                    else
                    {
                        //d.log(w, "Can't find Joint", d.Level.Warning);
                    }
                }
                OnFirstUpdate = false;
            }

            if (partAttached)
            {

                if (anim.isPlaying && joint != null)
                {
                    float theta = Mathf.Deg2Rad * 7.5f * anim[animationName].normalizedTime;

                    float l = 0.025f;
                    float r = 0.5f;

                    Vector3 v = new Vector3(0, l, 0);
                    v += getL(r, theta) * new Vector3(0, Mathf.Cos(theta / 2), Mathf.Sin(theta / 2));
                    v += 2 * l * new Vector3(0, Mathf.Cos(theta), Mathf.Sin(theta));
                    v += getL(r, theta) * new Vector3(0, Mathf.Cos(3 * theta / 2), Mathf.Sin(3 * theta / 2));
                    v += l * new Vector3(0, Mathf.Cos(2 * theta), Mathf.Sin(2 * theta));
                    v /= part.rescaleFactor;
                    int signe = (Vector3.Dot(transform.TransformDirection(joint.secondaryAxis), part.transform.TransformDirection(part.transform.right)) > 0) ? 1 : -1;

                    Quaternion q = new Quaternion(0, signe * Mathf.Sin(theta), 0, Mathf.Cos(theta));

                    if (isHost)
                    {
                        joint.anchor = v;
                    }
                    else
                    { joint.connectedAnchor = v; }
                    joint.targetRotation = q;
                }
            }
        }
        #endregion
    }
}
