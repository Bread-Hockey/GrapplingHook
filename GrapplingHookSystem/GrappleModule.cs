using System.Collections;
using System.Linq;
using ThunderRoad;
using ThunderRoad.AI.Get;
using UnityEngine;
namespace GrapplingHookSystem
{
    public class GrappleModule : ItemModule
    {
        public string hookId;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<Grappler>();
        }
    }
    public class Grappler : MonoBehaviour
    {
        private string hookId;
        private Item grappler;
        private GameObject shootTransform;
        private Item hook;
        private GameObject hookAttackPoint;
        private GrappleJoint joint;
        private LineRenderer linerender;
        private float clickTime = 0f;
        private const float doubleClickThreshold = 0.5f;
        private int clickCount = 0;
        public bool hasShot = false;
        private AimSystem aimSystem;
        public AudioSource audioSource;
        public AudioSource source;
        public Animator animator;
        public void Start()
        {
            grappler = GetComponent<Item>();
            hookId = grappler.data.modules.OfType<GrappleModule>().FirstOrDefault()?.hookId;

            foreach (Transform trans in grappler.GetComponentsInChildren<Transform>())
            {
                if (trans.name == "Shoot")
                {
                    shootTransform = trans.gameObject;
                }
            }

            grappler.OnHeldActionEvent += Item_OnHeldActionEvent;
            grappler.OnSnapEvent += Grappler_OnSnapEvent;
            grappler.OnUnSnapEvent += Grappler_OnUnSnapEvent;
            aimSystem = grappler.gameObject.AddComponent<AimSystem>();
            source = shootTransform.GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            audioSource = grappler.GetComponent<AudioSource>();
        }

        private void Grappler_OnSnapEvent(Holder holder)
        {
            if(holder.creature == Player.currentCreature && joint is JointSwing)
            {
                StopCoroutine(lineCoroutine);
                lineCoroutine = StartCoroutine(lineRender(Player.currentCreature.ragdoll.rootPart.gameObject));
                joint.SilentBreak();
                joint = new JointPlayer();
                joint.SetUp(grappler, shootTransform, hook, hookAttackPoint);
            }
        }

        private void Grappler_OnUnSnapEvent(Holder holder)
        {
            if(joint is JointPlayer)
            {
                StopCoroutine(lineCoroutine);
                lineCoroutine = StartCoroutine(lineRender(shootTransform.gameObject));
                joint.SilentBreak();
                hook.physicBody.isKinematic = true;
                joint = hook.gameObject.AddComponent<JointSwing>();
                joint.SetUp(grappler, shootTransform, hook, hookAttackPoint);
            }
        }


        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.UseStart && hook == null)
            {
                hasShot = true;
                animator.Play("Shoot");
                audioSource.Play();
                Catalog.GetData<ItemData>(hookId).SpawnAsync(hook =>
                {
                    hook.Throw(flyDetection: Item.FlyDetection.Forced); 
                    this.hook = hook;
                    hook.DisallowDespawn = true;
                    hook.transform.position = shootTransform.transform.position;
                    hook.transform.rotation = grappler.transform.rotation;
                    linerender = hook.gameObject.AddComponent<LineRenderer>();
                    linerender.startWidth = 0.01f;
                    linerender.endWidth = 0.01f;
                    linerender.material = new Material(Shader.Find("Sprites/Default"));
                    linerender.material.color = new Color(0, 0, 0, 1);
                    linerender.positionCount = 2;
                    linerender.useWorldSpace = true;
                    hookAttackPoint = hook.GetCustomReference("attachPoint").gameObject;


                    hook.IgnoreItemCollision(grappler);

                    lineCoroutine = StartCoroutine(lineRender(shootTransform.gameObject));

                    if (modOptions.hitMode == "Projectile")
                    {
                        hook.mainCollisionHandler.OnCollisionStartEvent += Projectile;
                        if (modOptions.aimAssist)
                        {
                            hook.physicBody.AddForce((aimSystem.aimSphere.transform.position - grappler.transform.position).normalized * modOptions.speed, ForceMode.VelocityChange);
                            hook.transform.LookAt(aimSystem.aimSphere.transform);
                        }
                        else
                        {
                            hook.physicBody.AddForce(grappler.transform.forward * modOptions.speed, ForceMode.VelocityChange);
                        }
                        if (!modOptions.dropPhysics)
                        {
                            hook.physicBody.useGravity = false;
                        }
                    } else
                    {
                        hitScan();
                    }
                });
            }
            else if (action == Interactable.Action.UseStart && hook != null && joint != null)
            {
                joint.Retract();
                if (source != null && modOptions.retractAudio)
                {
                    source.Play();
                }
            }
            else if (action == Interactable.Action.UseStop && hook != null && joint != null)
            {
                joint.Pause();
                source.Stop();
            }
            else if (action == Interactable.Action.AlternateUseStart && hook != null)
            {
                if (joint != null)
                {
                    if (source != null && modOptions.retractAudio)
                    {
                        source.Play();
                    }
                    joint.Extend();
                }
                float timeSinceLastClick = Time.time - clickTime;
                if (timeSinceLastClick <= doubleClickThreshold)
                {
                    clickCount++;
                }
                else
                {
                    clickCount = 1;
                }
                if (clickCount == 2)
                {
                    source.Stop();
                    hasShot = false;
                    animator.Play("Idle");
                    if (modOptions.hapticFeedback) PlayerControl.GetHand(grappler.mainHandler.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected, 5);
                    if (joint != null)
                    {
                        joint.Break();
                    }
                    else
                    {
                        hook.Despawn();
                    }
                }
                clickTime = Time.time;
            }
            else if (action == Interactable.Action.AlternateUseStop && hook != null && joint != null)
            {
                joint.Pause();
                source.Stop();
            }
        }

        private void hitScan()
        {
            hook.physicBody.isKinematic = true;
            hook.transform.position = aimSystem.hitPoint;
            if (aimSystem.aimedObject.transform.root.GetComponentInParent<Item>() is Item attatchedItem)
            {
                if(modOptions.itemPulling)
                {
                    joint = hook.gameObject.AddComponent<JointItem>();
                    joint.SetUp(grappler, shootTransform, hook, hookAttackPoint, attatchedItem);
                }
                hook.physicBody.isKinematic = true;
                hook.transform.SetParent(aimSystem.aimedObject.transform);
                DisableHookColliders();
            }
            else if (aimSystem.aimedObject.transform.root.GetComponent<Creature>() is Creature creature)
            {
                if (creature != null)
                {
                    RagdollPart closestPart = null;
                    float closestDistance = Mathf.Infinity;
                    foreach (RagdollPart part in creature.ragdoll.parts)
                    {
                        float distance = Vector3.Distance(part.transform.position, hook.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestPart = part;
                        }
                    }

                    if (modOptions.creaturePull)
                    {
                        joint = hook.gameObject.AddComponent<JointCreature>();
                        joint.SetUp(grappler, shootTransform, hook, closestPart.gameObject, creature);
                    }
                    hook.physicBody.isKinematic = true;
                    hook.transform.position = aimSystem.hitPoint;
                    hook.transform.SetParent(closestPart.transform);
                    DisableHookColliders();
                }
            }
            else if (aimSystem.aimedObject.gameObject.GetComponentInParent<Golem>() is Golem golem)
            {
                hook.physicBody.isKinematic = true;
                hook.transform.position = aimSystem.hitPoint;
                hook.transform.SetParent(aimSystem.aimedObject.transform);
                joint = hook.gameObject.AddComponent<JointSwing>();
                joint.SetUp(grappler, shootTransform, hook, hookAttackPoint);
            }
            else
            {
                hook.physicBody.isKinematic = true;
                joint = hook.gameObject.AddComponent<JointSwing>();
                joint.SetUp(grappler, shootTransform, hook, hookAttackPoint);
            }
        }

        private void DisableHookColliders()
        {
            foreach (ColliderGroup group in hook.colliderGroups)
            {
                foreach (Collider collider in group.colliders)
                {
                    collider.enabled = false;
                }
            }
        }


        public bool getHasShot()
        {
            return hasShot;
        }

        private Coroutine lineCoroutine;
        private IEnumerator lineRender(GameObject attatchPoint)
        {
            while (hook != null)
            {
                linerender.SetPosition(0, attatchPoint.transform.position);
                linerender.SetPosition(1, hookAttackPoint.transform.position);
                yield return null;
            }
            yield break;
        }

        private void Projectile(CollisionInstance collisionInstance)
        {
            if (joint == null)
            {
                if (collisionInstance.targetCollider.gameObject.transform.root.GetComponentInParent<Item>() is Item attatchedItem)
                {
                    if(modOptions.itemPulling)
                    {
                        joint = hook.gameObject.AddComponent<JointItem>();
                        joint.SetUp(grappler, shootTransform, hook, hookAttackPoint, attatchedItem);
                    }
                    hook.physicBody.isKinematic = true;
                    hook.transform.SetParent(collisionInstance.targetCollider.transform);
                    DisableHookColliders();
                }
                else if(collisionInstance.targetCollider.gameObject.transform.root.GetComponentInParent<Creature>() is Creature creature)
                {
                    if(creature != null && collisionInstance.targetCollider.gameObject.GetComponentInParent<RagdollPart>().gameObject != null)
                    {
                        if (modOptions.creaturePull)
                        {
                            joint = hook.gameObject.AddComponent<JointCreature>();
                            joint.SetUp(grappler, shootTransform, hook, collisionInstance.targetCollider.gameObject.GetComponentInParent<RagdollPart>().gameObject, creature);
                        }
                        hook.physicBody.isKinematic = true;
                        hook.transform.position = collisionInstance.contactPoint;
                        hook.transform.SetParent(collisionInstance.targetCollider.transform);
                        DisableHookColliders();
                    }
                } else if(collisionInstance.targetCollider.gameObject.GetComponentInParent<Golem>() is Golem golem)
                {
                    hook.physicBody.isKinematic = true;
                    hook.transform.position = collisionInstance.contactPoint;
                    hook.transform.SetParent(collisionInstance.targetCollider.transform);
                    joint = hook.gameObject.AddComponent<JointSwing>();
                    joint.SetUp(grappler, shootTransform, hook, hookAttackPoint);
                }
                else
                {
                    hook.physicBody.isKinematic = true;
                    joint = hook.gameObject.AddComponent<JointSwing>();
                    joint.SetUp(grappler, shootTransform, hook, hookAttackPoint);
                }
            }
        }
    }
}
