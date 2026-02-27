using ThunderRoad;
using UnityEngine;
namespace GrapplingHookSystem
{
    public class AimSystem : MonoBehaviour
    {
        public GameObject aimSphere;
        private Grappler grappleModule;
        private Item grappler;
        private Renderer renderer;
        public Vector3 hitPoint;
        public GameObject aimedObject;
        public void Start()
        {
            grappler = GetComponent<Item>();
            grappleModule = GetComponent<Grappler>();
            aimSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aimSphere.transform.localScale = new Vector3(modOptions.highlightSize, modOptions.highlightSize, modOptions.highlightSize);
            aimSphere.SetActive(false);
            renderer = aimSphere.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            Destroy(aimSphere.GetComponent<Collider>());
        }

        public void OnDestroy()
        {
            Destroy(aimSphere);
        }

        public void Update()
        {
            foreach (Creature creature in Creature.allActive)
            {
                float dot = Vector3.Dot((creature.ragdoll.rootPart.transform.position - grappler.transform.position).normalized, grappler.transform.forward);
                if (dot > 0.5f)
                {
                    creature.ragdoll.physicToggle = true;
                    creature.ragdoll.physicTogglePlayerRadius = 1000;
                    creature.ragdoll.physicToggleRagdollRadius = 1000;

                }
                else
                {
                    creature.ragdoll.physicTogglePlayerRadius = 3;
                    creature.ragdoll.physicToggleRagdollRadius = 3;
                }
            }

            if (modOptions.aimAssist)
            {
                if (!grappleModule.getHasShot() && grappler.handlers.Count > 0 && Physics.SphereCast(grappler.transform.position, modOptions.aimSense, grappler.transform.forward, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("TouchObject", "Zone", "LightProbeVolume", "BodyLocomotion", "PlayerHandAndFoot", "Avatar")))
                {
                    aimSphere.SetActive(true);
                    aimSphere.transform.position = hit.point;
                    hitPoint = hit.point;
                    aimedObject = hit.transform.gameObject;
                }
                else if (!modOptions.showHighlight || grappleModule.getHasShot() || grappler.handlers.Count == 0)
                {
                    aimSphere.SetActive(false);
                }
            }
            else
            {
                if (!grappleModule.getHasShot() && grappler.handlers.Count > 0 && Physics.Raycast(grappler.transform.position, grappler.transform.forward, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("TouchObject", "Zone", "LightProbeVolume", "BodyLocomotion", "Avatar", "NPC", "PlayerHandAndFoot", "Avatar")))
                {
                    aimSphere.SetActive(true);
                    aimSphere.transform.position = hit.point;
                    hitPoint = hit.point;
                    aimedObject = hit.transform.gameObject;
                    string layerName = LayerMask.LayerToName(hit.transform.gameObject.layer);
                    Debug.Log(layerName);
                }
                else if (!modOptions.showHighlight || grappleModule.getHasShot() || grappler.handlers.Count == 0)
                {
                    aimSphere.SetActive(false);
                }
            }

            if (modOptions.showHighlight == false)
            {
                renderer.enabled = false;
            }
            else
            {
                renderer.enabled = true;
            }
            if (modOptions.distanceScaling)
            {
                aimSphere.transform.localScale = new Vector3(modOptions.highlightSize + Vector3.Distance(grappler.transform.position, aimSphere.transform.position) / 75, modOptions.highlightSize + Vector3.Distance(grappler.transform.position, aimSphere.transform.position) / 75, modOptions.highlightSize + Vector3.Distance(grappler.transform.position, aimSphere.transform.position) / 75);
            }
            else
            {
                aimSphere.transform.localScale = new Vector3(modOptions.highlightSize, modOptions.highlightSize, modOptions.highlightSize);
            }
        }
    }
}
