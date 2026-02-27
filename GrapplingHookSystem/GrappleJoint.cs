using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;
namespace GrapplingHookSystem
{
    public class GrappleJoint : MonoBehaviour
    {
        public virtual void Retract() { }
        public virtual void Extend() { }
        public virtual void Break() { }
        public virtual void SilentBreak() { }

        public virtual void Pause() { }

        public virtual void SetUp(Item grappler, GameObject shootTransform, Item hook, GameObject hookAttackPoint) { }

        public virtual void SetUp(Item grappler, GameObject shootTransform, Item hook, GameObject hookAttackPoint, Item attactchedItem) { }
        public virtual void SetUp(Item grappler, GameObject shootTransform, Item hook, GameObject hookAttackPoint, Creature creature) { }

    }
}
