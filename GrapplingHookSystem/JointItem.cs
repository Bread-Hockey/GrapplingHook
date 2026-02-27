using UnityEngine;
using ThunderRoad;
using System.Collections;

namespace GrapplingHookSystem
{
    public class JointItem : GrappleJoint
    {
        private Item grappler;
        private Item hook;
        private Item attachedItem;

        private SpringJoint joint;
        private bool pulling;
        private bool extending;
        public override void SetUp(Item grappler, GameObject shootTransform, Item hook, GameObject hookAttackPoint, Item attachedItem)
        {
            this.grappler = grappler;
            this.hook = hook;
            this.attachedItem = attachedItem;
            CreateJoint();
            attachedItem.OnGrabEvent += AttachedItem_OnGrabEvent;
        }

        private void CreateJoint()
        {
            joint = attachedItem.gameObject.AddComponent<SpringJoint>();
            joint.anchor = attachedItem.transform.InverseTransformPoint(hook.transform.position);
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grappler.transform.position;
            joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) + 0.2f;
            joint.spring = modOptions.itemSpring;
            joint.damper = modOptions.itemDamper;
            joint.massScale = modOptions.itemMassScale;
        }

        public void Update()
        {
            joint.connectedAnchor = grappler.transform.position;
            joint.spring = modOptions.itemSpring;
            joint.damper = modOptions.itemDamper;
            joint.massScale = modOptions.itemMassScale;

            attachedItem.physicBody.AddForce(new Vector3(0.0001f,0.0001f,0.0001f), ForceMode.Acceleration);
            if (grappler.physicBody.rigidBody.velocity.magnitude > 4)
            {
                if (attachedItem.handlers.Count > 0)
                {
                    foreach (Handle handle in attachedItem.handles)
                    {
                        handle.Release();
                    }
                    if(modOptions.hapticFeedback) PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 10);

                }
            }
        }


        private void AttachedItem_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            Break();
        }

        public override void Retract()
        {
            if (joint == null) return;
            StartCoroutine(pull());
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

        private IEnumerator pull()
        {
            pulling = true;
            while (pulling)
            {
                if(attachedItem.handlers.Count > 0)
                {
                    foreach (Handle handle in attachedItem.handles)
                    {
                        handle.Release();
                    }
                    PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 10);
                }
                if (modOptions.hapticFeedback) PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 4);
                joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) - modOptions.itemRetractSpeed;
                yield return null;
            }
            yield break;
        }


        public IEnumerator extend()
        {
            extending = true;
            while (extending)
            {
                if (modOptions.hapticFeedback) PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 4);
                joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) + modOptions.itemExtendSpeed;
                yield return null;
            }
            yield break;
        }

        public override void Break()
        {
            pulling = false;
            extending = false;

            if (attachedItem != null)
                attachedItem.OnGrabEvent -= AttachedItem_OnGrabEvent;

            if (joint != null)
                Destroy(joint);

            if (hook != null)
                hook.Despawn();
            Grappler grappleModule = grappler.GetComponent<Grappler>();
            grappleModule.animator.Play("Idle");
            grappleModule.hasShot = false;
            grappleModule.source.Stop();
            Destroy(this);
        }
    }
}
