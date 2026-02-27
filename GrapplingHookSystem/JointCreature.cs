using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;
using System.Collections;
namespace GrapplingHookSystem
{
    public class JointCreature : GrappleJoint
    {
        private GameObject shootTransform;
        private Item grappler;
        private Item hook;
        private GameObject hookAttatchPoint;
        private SpringJoint joint;
        private bool pulling;
        private bool extending;
        private Creature creature;
        public override void SetUp(Item grappler, GameObject shootTransform, Item hook, GameObject hookAttatchPoint, Creature creature)
        {
            this.grappler = grappler;
            this.shootTransform = shootTransform;
            this.hook = hook;
            this.hookAttatchPoint = hookAttatchPoint;
            this.creature = creature;
            joint = hookAttatchPoint.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grappler.transform.position;
            joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) + 0.2f;
            joint.spring = modOptions.creatureSpring;
            joint.damper = modOptions.creatureDamper;
            joint.massScale = modOptions.creatureMassScale;

            creature.ragdoll.physicToggle = true;
            foreach (ColliderGroup group in hook.colliderGroups)
            {
                foreach (Collider collider in group.colliders)
                {
                    collider.enabled = false;
                }
            }
        }

        public override void Retract()
        {
            StartCoroutine(pull());
        }

        public override void Break()
        {
            StopAllCoroutines();
            pulling = false;
            extending = false;
            hook.Despawn();
            GameObject.Destroy(joint);
            GameObject.Destroy(this);
        }

        public override void Pause()
        {
            pulling = false;
            extending = false;
        }

        public override void Extend()
        {
            StartCoroutine(extend());
        }

        public IEnumerator extend()
        {
            extending = true;
            while (extending)
            {
                if (modOptions.hapticFeedback) PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 4);
                joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) + modOptions.creatureExtendSpeed;
                yield return null;
            }
            yield break;
        }
        private IEnumerator pull()
        {
            pulling = true;
            while (pulling)
            {
                if (modOptions.hapticFeedback) PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 4);
                joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) - modOptions.creatureRetractSpeed;
                yield return null;
            }
            yield break;
        }

        public void Update()
        {
            if (joint != null)
            {
                joint.connectedAnchor = grappler.transform.position;
                joint.spring = modOptions.creatureSpring;
                joint.damper = modOptions.creatureDamper;
                joint.massScale = modOptions.creatureMassScale;
                if (grappler.physicBody.velocity.magnitude > 4 && creature.ragdoll.state != Ragdoll.State.Destabilized || Vector3.Distance(creature.ragdoll.rootPart.transform.position, creature.locomotion.transform.position) > 1)
                {
                    creature.ragdoll.physicToggle = true;
                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                    if (modOptions.hapticFeedback) PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 5);
                }
            }
        }
    }
}
