using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace GrapplingHook
{
    public class modOptions : ThunderScript
    {
        [ModOption("Shot Speed", tooltip = "Speed the hook is shot out", valueSourceName = nameof(speed1), category = "Shooting Settings", interactionType = ModOption.InteractionType.Slider, order = 1)]
        public static float speed = 40;
        internal static ModOptionFloat[] speed1()
        {
            return ModOptionFloat.CreateArray(1, 100, 1);
        }
        [ModOption("Hook drop physics", tooltip = "Disables the gravity on the hook in order to make it not drop", category = "Shooting Settings", order = 2)]
        public static bool dropPhysics = true;
        [ModOption("Aim Assist",tooltip = "Lock onto nearby surfaces, allowing for easy aiming", category = "Aim Settings", order = 3)]
        public static bool aimAssist = false;
        [ModOption("Aim Assist Sensitivity", tooltip = "The radius in which aim assist locks onto nearby surfaces", category = "Aim Settings", valueSourceName = nameof(aimSense1), interactionType = ModOption.InteractionType.Slider, order = 4)]
        public static float aimSense = 0.50f;
        internal static ModOptionFloat[] aimSense1()
        {
            return ModOptionFloat.CreateArray(0.25f, 5, 0.25f);
        }
        [ModOption("Show Highlighter", tooltip = "Enables/Disables the aiming Highlight Sphere", category = "Aim Settings", order = 5)]
        public static bool showHighlight = true;
        [ModOption("Distance Highlight Scaling", "The further away you are from the highlight the bigger it gets", category = "Aim Settings", order = 5)]
        public static bool distanceScaling = false;
        [ModOption("Highlight Size", "Base highlight size", category = "Aim Settings", valueSourceName = nameof(highlightSize1), interactionType = ModOption.InteractionType.Slider, order = 6)]
        public static float highlightSize = 0.05f;
        internal static ModOptionFloat[] highlightSize1()
        {
            return ModOptionFloat.CreateArray(0.05f, 1, 0.15f);
        }
        [ModOption("Retract Speed", tooltip = "Speed in which in the winch pulls you up", interactionType = ModOption.InteractionType.Slider, valueSourceName = nameof(retractSpeed1), category = "Swing Settings", order = 7)]
        public static float retractSpeed = 1;
        internal static ModOptionFloat[] retractSpeed1()
        {
            return ModOptionFloat.CreateArray(1, 15, 1);
        }
        [ModOption("Spring", interactionType = ModOption.InteractionType.Slider, valueSourceName = nameof(Spring1), category = "Swing Settings", order = 8)]
        public static float Spring = 500;
        internal static ModOptionFloat[] Spring1()
        {
            return ModOptionFloat.CreateArray(1, 1000, 1);
        }
        [ModOption("Damper", interactionType = ModOption.InteractionType.Slider, valueSourceName = nameof(Damper1), category = "Swing Settings", order = 9)]
        public static float Damper = 130;
        internal static ModOptionFloat[] Damper1()
        {
            return ModOptionFloat.CreateArray(1, 1000, 1);
        }
        [ModOption("Mass Scale", interactionType = ModOption.InteractionType.Slider, valueSourceName = nameof(Mass1), category = "Swing Settings", order = 10)]
        public static float MassScale = 35;
        internal static ModOptionFloat[] Mass1()
        {
            return ModOptionFloat.CreateArray(1, 1000, 1);
        }
        [ModOption("Shoot Audio", tooltip = "Disables/Enables the shoot audio", category = "Audio", order = 11)]
        public static bool shootAudio = true;
        [ModOption("Retract Audio", tooltip = "Disables/Enables the winch audio", category = "Audio", order = 12)]
        public static bool retractAudio = true;
    }
    public class ItemModuleGrapplingHook : ItemModule
    {
        public override void OnItemLoaded(ThunderRoad.Item item)
        {
            item.gameObject.AddComponent<Mono1>();
        }
    }
    public class Mono1 : MonoBehaviour
    {
        public ThunderRoad.Item item;
        public Transform shootTransform;
        public Transform attachPoint;
        private Animator animator;
        public LineRenderer linerender;
        public bool canShoot = true;
        public Item hook1;
        public bool canPull = false;
        public SpringJoint joint;
        public SpringJoint creatureJoint;
        public AudioSource audioSource;
        public AudioSource source;
        public LineRenderer lineRenderer2;
        public GameObject sphere;
        public Renderer render1;
        public bool isCreature;
        bool shot = false;
        public Collision collision;
        public bool allowCreaturePull = false;
        public bool detectingYank = false;
        void Start()
        {
            item = GetComponent<ThunderRoad.Item>();
            shootTransform = item.GetCustomReference("ShootTransform");
            source = shootTransform.GetComponent<AudioSource>();
            item.mainHandleRight.OnHeldActionEvent += MainHandleRight_OnHeldActionEvent;
            animator = GetComponent<Animator>();
            audioSource = item.GetComponent<AudioSource>();
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.rotation = Quaternion.identity;
            sphere.transform.localScale = new Vector3(modOptions.highlightSize, modOptions.highlightSize, modOptions.highlightSize);
            sphere.SetActive(false);
            Renderer renderer = sphere.GetComponent<Renderer>();
            render1 = renderer;
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            Destroy(sphere.GetComponent<Collider>());
            item.OnGrabEvent += Item_OnGrabEvent;
        }

        IEnumerator detectYank()
        {
            Creature creature = creatureJoint.gameObject.transform.root.GetComponentInParent<Creature>();
            while (true)
            {
                if (creatureJoint != null)
                {
                    if(item.physicBody.velocity.magnitude > 4 || creature.ragdoll.state == Ragdoll.State.Destabilized)
                    {
                        allowCreaturePull = true;
                        creature.ragdoll.physicTogglePlayerRadius = 1000000;
                        creature.ragdoll.physicToggleRagdollRadius = 1000000;
                        creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                        creature.brain.instance.Stop();
                        yield break;
                    }
                    yield return null;
                }
                yield return null;
            }
        }

        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            if (isCreature)
            {
                creatureJoint.gameObject.transform.root.GetComponentInParent<Creature>().ragdoll.physicTogglePlayerRadius = 1000000;
                creatureJoint.gameObject.transform.root.GetComponentInParent<Creature>().ragdoll.physicToggleRagdollRadius = 1000000;
                creatureJoint.gameObject.transform.root.GetComponentInParent<Creature>().ragdoll.SetState(Ragdoll.State.Destabilized);
                creatureJoint.maxDistance = Vector3.Distance(item.transform.position, hook1.transform.position) + 0.2f;
                creatureJoint.gameObject.transform.root.GetComponentInParent<Creature>().brain.instance.Stop();
                joint.spring = 0;
                joint.damper = 0;
                joint.massScale = 0;
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = hook1.transform.position;
                joint.minDistance = 0.1f;
                joint.spring = 0;
            }
        }

        void Update()
        {
            if(creatureJoint != null && !allowCreaturePull && !canShoot && !detectingYank)
            {
                detectingYank = true;
                GameManager.local.StartCoroutine(detectYank());
            }
            if (!shot && item.handlers.Count > 0 && modOptions.aimAssist && Physics.SphereCast(item.transform.position, modOptions.aimSense, item.transform.forward, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("TouchObject", "MovingItem", "DroppedItem", "Zone", "LightProbeVolume", "BodyLocomotion")))
            {
                sphere.SetActive(true);
                sphere.transform.position = hit.point;
            } 
            else if(!modOptions.showHighlight || shot || item.handlers.Count == 0)
            {
                sphere.SetActive(false);
            }

            if(modOptions.aimAssist && !Physics.SphereCast(item.transform.position, modOptions.aimSense, item.transform.forward, out RaycastHit hit1, Mathf.Infinity, ~LayerMask.GetMask("TouchObject", "MovingItem", "DroppedItem", "Zone", "LightProbeVolume", "BodyLocomotion")))
            {
                sphere.SetActive(false);
            }

            if (!modOptions.aimAssist && !Physics.Raycast(item.transform.position, item.transform.forward, out RaycastHit hit3, Mathf.Infinity, ~LayerMask.GetMask("TouchObject", "MovingItem", "DroppedItem", "Zone", "LightProbeVolume", "BodyLocomotion")))
            {
                sphere.SetActive(false);
            }

            if (!shot && !modOptions.aimAssist && item.handlers.Count > 0 && Physics.Raycast(item.transform.position, item.transform.forward, out RaycastHit hit2, Mathf.Infinity, ~LayerMask.GetMask("TouchObject", "MovingItem", "DroppedItem", "Zone", "LightProbeVolume", "BodyLocomotion")))
            {
                sphere.SetActive(true);
                sphere.transform.position = hit2.point;
            } 
            else if(!modOptions.showHighlight || shot || item.handlers.Count == 0)
            {
                sphere.SetActive(false); 
            }
            if (modOptions.showHighlight == false)
            {
                render1.enabled = false;
            }
            else
            {
                render1.enabled = true;
            }
            if (modOptions.distanceScaling)
            {
                sphere.transform.localScale = new Vector3(modOptions.highlightSize + Vector3.Distance(item.transform.position, sphere.transform.position) / 75, modOptions.highlightSize + Vector3.Distance(item.transform.position, sphere.transform.position) / 75, modOptions.highlightSize + Vector3.Distance(item.transform.position, sphere.transform.position) / 75);
            }
            else
            {
                sphere.transform.localScale = new Vector3(modOptions.highlightSize, modOptions.highlightSize, modOptions.highlightSize);
            }
        }
        bool test = false;

        private void MainHandleRight_OnHeldActionEvent(RagdollHand ragdollHand, Interactable.Action action)
        {
            if(action == Interactable.Action.Ungrab)
            {
                if (creatureJoint != null)
                {
                    creatureJoint.gameObject.transform.root.GetComponentInParent<Creature>().brain.instance.Start();
                    creatureJoint.spring = 0;
                    creatureJoint.maxDistance = Mathf.Infinity;
                    joint.spring = modOptions.Spring;
                    joint.damper = modOptions.Damper;
                    joint.massScale = modOptions.MassScale;
                    joint.autoConfigureConnectedAnchor = false;
                    joint.connectedAnchor = hook1.transform.position;
                    joint.minDistance = 0.1f;
                    isCreature = true;
                }
            }
            if (action == Interactable.Action.UseStart && canShoot)
            {
                if (animator != null) 
                {
                    animator.Play("Shoot");
                }
                if (audioSource != null && modOptions.shootAudio)
                {
                    audioSource.Play();
                }
                Catalog.GetData<ItemData>("HookItem").SpawnAsync(hook =>
                {
                    shot = true;
                    hook.DisallowDespawn = true;
                    LineRenderer linerender = hook.gameObject.AddComponent<LineRenderer>();
                    linerender.startWidth = 0.01f;
                    linerender.endWidth = 0.01f;
                    linerender.material = new Material(Shader.Find("Sprites/Default"));
                    linerender.material.color = new Color(0, 0, 0, 1);
                    linerender.positionCount = 2;
                    hook.IgnoreItemCollision(item);
                    hook.transform.position = shootTransform.position;
                    hook.transform.rotation = item.transform.rotation;
                    if (modOptions.aimAssist)
                    {
                        hook.physicBody.AddForce((sphere.transform.position - item.transform.position).normalized * modOptions.speed, ForceMode.VelocityChange);
                        hook.transform.LookAt(sphere.transform);
                    }
                    else
                    {
                        hook.physicBody.AddForce(item.transform.forward * modOptions.speed, ForceMode.VelocityChange);
                    }
                    Transform attachPoint = hook.GetCustomReference("attachPoint");
                    GameManager.local.StartCoroutine(Line(hook, linerender, attachPoint, item));
                    Mono2 mono2 = hook.gameObject.AddComponent<Mono2>();
                    mono2.grappleItem = item;
                    hook1 = hook;
                    canShoot = false;
                    canPull = true;
                    if (!modOptions.dropPhysics)
                    {
                        hook.physicBody.useGravity = false;
                    }
                });
            }

            if (action == Interactable.Action.AlternateUseStart && !canShoot)
            {
                hook1.DisallowDespawn = false;
                GameManager.local.StopCoroutine(Line(hook1, linerender, attachPoint, item));
                GameManager.local.StopCoroutine(Line(hook1, linerender, attachPoint, item));
                UnityEngine.GameObject.Destroy(hook1.GetComponent<LineRenderer>());
                UnityEngine.GameObject.Destroy(item.GetComponent<SpringJoint>());
                if(creatureJoint != null)
                {
                    creatureJoint.gameObject.transform.root.GetComponentInParent<Creature>().brain.instance.Start();
                    UnityEngine.GameObject.Destroy(creatureJoint);
                }
                hook1.Despawn();
                animator.Play("Idle");
                canShoot = true;
                canPull = false;
                source.Stop();
                shot = false;
                isCreature = false;
                allowCreaturePull = false;
                detectingYank = false;
            }

            if (action == Interactable.Action.UseStart && canPull)
            {
                if(creatureJoint == null)
                {
                    test = true;
                    if (source != null && modOptions.retractAudio)
                    {
                        source.Play();
                    }
                    GameManager.local.StartCoroutine(pull(hook1, item));
                } else if(creatureJoint != null && allowCreaturePull)
                {
                    test = true;
                    if (source != null && modOptions.retractAudio)
                    {
                        source.Play();
                    }
                    GameManager.local.StartCoroutine(pull(hook1, item));
                }
            }

            if (action == Interactable.Action.UseStop && canPull)
            {
                test = false;
                GameManager.local.StopCoroutine(pull(hook1, item));
                if (source != null)
                {
                    source.Stop();
                }
            }
        }

        IEnumerator pull(Item Hook, Item item)
        {
            while (test == true)
            {
                if (item.handlers.Count == 0)
                {
                    test = false;
                    GameManager.local.StopCoroutine(pull(hook1, item));
                    source.Stop();
                }
                if (Player.local.locomotion.isGrounded && creatureJoint == null)
                {
                    Player.local.locomotion.Jump(true);
                }
                if (joint != null && item != null && Hook != null)
                {
                    joint.maxDistance = Vector3.Distance(item.transform.position, Hook.transform.position) - modOptions.retractSpeed;
                    if(creatureJoint != null)
                    {
                        creatureJoint.maxDistance = Vector3.Distance(item.transform.position, Hook.transform.position) - modOptions.retractSpeed;
                    }
                }
                else
                {
                    break;
                }
                yield return null;
            }
        }

        private IEnumerator Line(ThunderRoad.Item hook, LineRenderer line, Transform attachPoint, Item Grapple)
        {
            joint = Grapple.gameObject.AddComponent<SpringJoint>();
            bool setmax = false;
            joint.spring = 0;
            while (true)
            {
                if (attachPoint != null && shootTransform != null)
                {
                    line.SetPosition(0, attachPoint.transform.position);
                    line.SetPosition(1, shootTransform.transform.position);
                }
                else
                {
                    break;
                }
                if (hook.physicBody.isKinematic)
                {
                    if(creatureJoint != null)
                    {
                        creatureJoint.autoConfigureConnectedAnchor = false;
                        creatureJoint.connectedAnchor = Grapple.transform.position;
                        creatureJoint.minDistance = 0.1f;
                        //Configurations
                        creatureJoint.spring = modOptions.Spring;
                        creatureJoint.damper = modOptions.Damper;
                        creatureJoint.massScale = modOptions.MassScale;
                        if (!setmax)
                        {
                            creatureJoint.maxDistance = Vector3.Distance(Grapple.transform.position, hook.transform.position) + 0.2f;
                            joint.maxDistance = Vector3.Distance(Grapple.transform.position, hook.transform.position) + 0.2f;
                            setmax = true;
                        }
                    } else
                    {
                        joint.autoConfigureConnectedAnchor = false;
                        joint.connectedAnchor = hook.transform.position;
                        joint.minDistance = 0.1f;
                        if (!setmax)
                        {
                            joint.maxDistance = Vector3.Distance(Grapple.transform.position, hook.transform.position) + 0.2f;
                            setmax = true;
                        }
                        //Configurations
                        joint.spring = modOptions.Spring;
                        joint.damper = modOptions.Damper;
                        joint.massScale = modOptions.MassScale;
                    }
                }
                yield return null;
            }
        }
    }

    public class Mono2 : MonoBehaviour
    {
        public ThunderRoad.Item Item;
        public Item grappleItem;
        public bool isFrozen = false;
        public bool hitItem = false;
        void Start()
        {
            Item = GetComponent<ThunderRoad.Item>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(!isFrozen)
            {
                if (collision.gameObject.GetComponentInParent<Golem>())
                {
                    foreach (ColliderGroup group in Item.colliderGroups)
                    {
                        foreach (Collider collider in group.colliders)
                        {
                            collider.enabled = false;
                        }
                    }
                    Item.physicBody.isKinematic = true;
                    Item.transform.SetParent(collision.transform);
                    Item.physicBody.isEnabled = false;
                    Item.physicBody.mass = 0;
                    Item.physicBody.centerOfMass = collision.transform.position;
                }
                else if (collision.gameObject.transform.root.GetComponentInParent<Creature>())
                {
                    grappleItem.GetComponent<Mono1>().collision = collision;
/*                    collision.gameObject.transform.root.GetComponentInParent<Creature>().ragdoll.physicTogglePlayerRadius = 1000000;
                    collision.gameObject.transform.root.GetComponentInParent<Creature>().ragdoll.physicToggleRagdollRadius = 1000000;
                    collision.gameObject.transform.root.GetComponentInParent<Creature>().ragdoll.SetState(Ragdoll.State.Destabilized);
                    collision.gameObject.transform.root.GetComponentInParent<Creature>().brain.instance.Stop();*/
                    grappleItem.GetComponent<Mono1>().creatureJoint = collision.gameObject.GetComponentInParent<RagdollPart>().gameObject.AddComponent<SpringJoint>();
                    foreach (ColliderGroup group in Item.colliderGroups)
                    {
                        foreach (Collider collider in group.colliders)
                        {
                            collider.enabled = false;
                        }
                    }
                    Item.physicBody.isKinematic = true;
                    Item.transform.SetParent(collision.transform);
                }
                else
                {
                    Item.physicBody.isKinematic = true;
                }
            }
            isFrozen = true;
        }
    }
}

