using System.Collections;
using ThunderRoad;
using UnityEngine;
namespace GrapplingHookSystem
{
    public class JointSwing : GrappleJoint
    {
        private bool pulling = false;
        private bool extending = false;

        private GameObject shootTransform;
        private Item grappler;
        private Item hook;
        private GameObject hookAttackPoint;
        private SpringJoint joint;
        public override void SetUp(Item grappler, GameObject shootTransform, Item hook, GameObject hookAttackPoint)
        {
            this.grappler = grappler;
            this.shootTransform = shootTransform;
            this.hook = hook;
            this.hookAttackPoint = hookAttackPoint;
            joint = grappler.gameObject.AddComponent<SpringJoint>();
            joint.anchor = joint.transform.InverseTransformPoint(shootTransform.transform.position);
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = hook.transform.position;
            joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) + 0.2f;
            joint.spring = modOptions.Spring;
            joint.damper = modOptions.Damper;
            joint.massScale = modOptions.MassScale;
            StartCoroutine(PlayerRotation());
        }

        public override void Extend()
        {
            extending = true;
            StartCoroutine(extend());
        }
        public override void Retract()
        {
            StartCoroutine(pull());
        }
        public override void Pause()
        {
            pulling = false;
            extending = false;
        }

        public override void Break()
        {
            StopAllCoroutines();
            pulling = false;
            hook.Despawn();
            GameObject.Destroy(joint);
            GameObject.Destroy(this);
            Player.local.autoAlign = true;
            Player.local.autoAlignDirection = Vector3.up;
        }

        public override void SilentBreak()
        {
            StopAllCoroutines();
            pulling = false;
            GameObject.Destroy(joint);
            GameObject.Destroy(this);
            Player.local.autoAlign = true;
            Player.local.autoAlignDirection = Vector3.up;
        }

        private IEnumerator pull()
        {
            pulling = true;
            while (pulling)
            {
                if (modOptions.hapticFeedback) PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 5);
                joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) - modOptions.retractSpeed;

                if (Player.local.locomotion.isGrounded == true)
                {
                    Player.local.locomotion.physicBody.AddForce(Vector3.up * 10, ForceMode.Impulse);
                }
                yield return null;
            }
            yield break;
        }

        public IEnumerator extend()
        {
            while (extending)
            {
                if (modOptions.hapticFeedback) PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 4);
                joint.maxDistance = Vector3.Distance(grappler.transform.position, hook.transform.position) + modOptions.extendSpeed;
                yield return null;
            }
            yield break;
        }

        public IEnumerator PlayerRotation()
        {
            while (modOptions.playerRotation && hook != null)
            {
                if (grappler.handlers.Count > 0 && !Player.local.locomotion.isGrounded)
                {
                    if (Player.local.locomotion.physicBody.rigidBody.velocity.magnitude > 3)
                    {
                        Player.local.autoAlign = false;
                        Quaternion quaternion = Quaternion.FromToRotation(Player.local.locomotion.transform.up, (hook.transform.position - grappler.transform.position));
                        Player.local.transform.rotation = Quaternion.Slerp(Player.local.locomotion.transform.rotation, quaternion * Player.local.locomotion.transform.rotation, 10f * Time.fixedDeltaTime);
                    }
                    else
                    {
                        Player.local.autoAlign = true;
                        Player.local.autoAlignDirection = Vector3.up;
                    }
                }
                else
                {
                    Player.local.autoAlign = true;
                    Player.local.autoAlignDirection = Vector3.up;
                }
                yield return null;
            }
            Player.local.autoAlign = true;
            Player.local.autoAlignDirection = Vector3.up;
            yield break;
        }
    }
}
