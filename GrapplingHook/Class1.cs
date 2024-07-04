using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace GrapplingHook
{
    public class modOptions : ThunderScript
    {
        [ModOption("Shot Speed", tooltip = "Speed the hook is shot out", defaultValueIndex = 39, valueSourceName = nameof(speed1), category = "Shooting Settings", interactionType = ModOption.InteractionType.Slider, order = 1)]
        public static float speed;
        internal static ModOptionFloat[] speed1()
        {
            return ModOptionFloat.CreateArray(1, 100, 1);
        }
        [ModOption("Hook drop physics", tooltip = "Disables the gravity on the hook in order to make it not drop", category = "Shooting Settings", defaultValueIndex = 1, order = 2)]
        public static bool dropPhysics;
        [ModOption("Aim Assist",tooltip = "Lock onto nearby surfaces, allowing for easy aiming" ,defaultValueIndex = 0, category = "Aim Settings", order = 3)]
        public static bool aimAssist;
        [ModOption("Aim Assist Sensitivity", tooltip = "The radius in which aim assist locks onto nearby surfaces", defaultValueIndex = 2, category = "Aim Settings", valueSourceName = nameof(aimSense1), interactionType = ModOption.InteractionType.Slider, order = 4)]
        public static float aimSense;
        internal static ModOptionFloat[] aimSense1()
        {
            return ModOptionFloat.CreateArray(0.25f, 5, 0.25f);
        }
        [ModOption("Show Highlighter", tooltip = "Enables/Disables the aiming Highlight Sphere", defaultValueIndex = 1, category = "Aim Settings", order = 5)]
        public static bool showHighlight;
        [ModOption("Distance Highlight Scaling", "The further away you are from the highlight the bigger it gets", defaultValueIndex = 0, category = "Aim Settings", order = 5)]
        public static bool distanceScaling;
        [ModOption("Highlight Size", "Base highlight size", defaultValueIndex = 0, category = "Aim Settings", valueSourceName = nameof(highlightSize1), interactionType = ModOption.InteractionType.Slider, order = 6)]
        public static float highlightSize;
        internal static ModOptionFloat[] highlightSize1()
        {
            return ModOptionFloat.CreateArray(0.05f, 1, 0.15f);
        }
        [ModOption("Retract Speed", tooltip = "Speed in which in the winch pulls you up", defaultValueIndex = 0, interactionType = ModOption.InteractionType.Slider, valueSourceName = nameof(retractSpeed1), category = "Swing Settings", order = 7)]
        public static float retractSpeed;
        internal static ModOptionFloat[] retractSpeed1()
        {
            return ModOptionFloat.CreateArray(1, 15, 1);
        }
        [ModOption("Spring", defaultValueIndex = 499, interactionType = ModOption.InteractionType.Slider, valueSourceName = nameof(Spring1), category = "Swing Settings", order = 8)]
        public static float Spring;
        internal static ModOptionFloat[] Spring1()
        {
            return ModOptionFloat.CreateArray(1, 1000, 1);
        }
        [ModOption("Damper", defaultValueIndex = 129, interactionType = ModOption.InteractionType.Slider, valueSourceName = nameof(Damper1), category = "Swing Settings", order = 9)]
        public static float Damper;
        internal static ModOptionFloat[] Damper1()
        {
            return ModOptionFloat.CreateArray(1, 1000, 1);
        }
        [ModOption("Mass Scale", defaultValueIndex = 34, interactionType = ModOption.InteractionType.Slider, valueSourceName = nameof(Mass1), category = "Swing Settings", order = 10)]
        public static float MassScale;
        internal static ModOptionFloat[] Mass1()
        {
            return ModOptionFloat.CreateArray(1, 1000, 1);
        }
        [ModOption("Shoot Audio", tooltip = "Disables/Enables the shoot audio", category = "Audio", defaultValueIndex = 1, order = 11)]
        public static bool shootAudio;
        [ModOption("Retract Audio", tooltip = "Disables/Enables the winch audio", category = "Audio", defaultValueIndex = 1, order = 12)]
        public static bool retractAudio;
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
        public AudioSource audioSource;
        public AudioSource source;
        public LineRenderer lineRenderer2;
        public GameObject sphere;
        public Renderer render1;
        bool shot = false;
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
        }

        void Update()
        {
            if (!shot && item.handlers.Count > 0 && modOptions.aimAssist && Physics.SphereCast(item.transform.position, modOptions.aimSense, item.transform.forward, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("TouchObject", "MovingItem", "DroppedItem", "Zone", "LightProbeVolume")))
            {
                sphere.SetActive(true);
                sphere.transform.position = hit.point;
            } 
            else if(!modOptions.showHighlight || shot || item.handlers.Count == 0)
            {
                sphere.SetActive(false);
            }
            if(!shot && !modOptions.aimAssist && item.handlers.Count > 0 && Physics.Raycast(item.transform.position, item.transform.forward, out RaycastHit hit2, Mathf.Infinity, ~LayerMask.GetMask("TouchObject", "MovingItem", "DroppedItem", "Zone", "LightProbeVolume")))
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
                    hook.disallowDespawn = true;
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
                hook1.disallowDespawn = false;
                GameManager.local.StopCoroutine(Line(hook1, linerender, attachPoint, item));
                GameManager.local.StopCoroutine(Line(hook1, linerender, attachPoint, item));
                UnityEngine.GameObject.Destroy(hook1.GetComponent<LineRenderer>());
                UnityEngine.GameObject.Destroy(item.GetComponent<SpringJoint>());
                hook1.Despawn();
                animator.Play("Idle");
                canShoot = true;
                canPull = false;
                source.Stop();
                shot = false;
            }

            if (action == Interactable.Action.UseStart && canPull)
            {
                test = true;
                if (source != null && modOptions.retractAudio)
                {
                    source.Play();
                }
                GameManager.local.StartCoroutine(pull(hook1, item));
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
                if (Player.local.locomotion.isGrounded)
                {
                    Player.local.locomotion.Jump(true);
                }
                if (joint != null && item != null && Hook != null)
                {
                    joint.maxDistance = Vector3.Distance(item.transform.position, Hook.transform.position) - modOptions.retractSpeed;
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
                    joint.autoConfigureConnectedAnchor = false;
                    joint.connectedAnchor = hook.transform.position;
                    if (!setmax)
                    {
                        joint.maxDistance = Vector3.Distance(Grapple.transform.position, hook.transform.position) + 0.2f;
                        setmax = true;
                    }
                    joint.minDistance = 0.1f;

                    //Configurations
                    joint.spring = modOptions.Spring;
                    joint.damper = modOptions.Damper;
                    joint.massScale = modOptions.MassScale;
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
                /*if (collision.gameObject.GetComponentInParent<Golem>())
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
                }*/
                /*else*/
                {
                    Item.physicBody.isKinematic = true;
                }
            }
            isFrozen = true;
        }
    }
}

