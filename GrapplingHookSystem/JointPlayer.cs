using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace GrapplingHookSystem
{
    public class JointPlayer : GrappleJoint
    {
        private SpringJoint joint;
        public override void SetUp(Item grappler, GameObject shootTransform, Item hook, GameObject hookAttatchPoint)
        {
            joint = Player.local.locomotion.gameObject.AddComponent<SpringJoint>();
            joint.anchor = joint.transform.InverseTransformPoint(shootTransform.transform.position);
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = hook.transform.position;
            joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) + 0.2f;
            joint.spring = modOptions.Spring;
            joint.damper = modOptions.Damper;
            joint.massScale = modOptions.MassScale;
        }

        public override void SilentBreak()
        {
            GameObject.Destroy(joint);
            GameObject.Destroy(this);
        }
    }
}
