using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace GrapplingHookSystem
{
    public class modOptions
    {

        //---------------Shooting Settings--------------------------------
        [ModOptionFloatValues(1, 100, 1)]
        [ModOption("Shot Speed", tooltip = "Speed the hook is shot out", category = "Shooting Settings", interactionType = ModOption.InteractionType.Slider, order = 1)]
        public static float speed = 40;

        [ModOption("Hook drop physics", tooltip = "Disables the gravity on the hook in order to make it not drop", category = "Shooting Settings", order = 2)]
        public static bool dropPhysics = true;

        public static ModOptionString[] state =
        {
            new ModOptionString("Projectile", "Projectile"),
            new ModOptionString("Hit Scan", "Hit Scan")
        };
        [ModOption("Hit Mode", tooltip = "Mode selection for impact detection", category = "Shooting Settings", valueSourceName = nameof(state))]
        public static string hitMode;


        //------------------------Aim Settings----------------------------

        [ModOption("Aim Assist", tooltip = "Lock onto nearby surfaces, allowing for easy aiming", category = "Aim Settings", order = 3)]
        public static bool aimAssist = false;

        [ModOptionFloatValues(0.25f, 5, 0.25f)]
        [ModOption("Aim Assist Sensitivity", tooltip = "The radius in which aim assist locks onto nearby surfaces", category = "Aim Settings", interactionType = ModOption.InteractionType.Slider, order = 4)]
        public static float aimSense = 0.50f;

        [ModOption("Show Highlighter", tooltip = "Enables/Disables the aiming Highlight Sphere", category = "Aim Settings", order = 5)]
        public static bool showHighlight = true;

        [ModOption("Distance Highlight Scaling", "The further away you are from the highlight the bigger it gets", category = "Aim Settings", order = 5)]
        public static bool distanceScaling = false;

        [ModOptionFloatValues(0.05f, 1, 0.15f)]
        [ModOption("Highlight Size", "Base highlight size", category = "Aim Settings", interactionType = ModOption.InteractionType.Slider, order = 6)]
        public static float highlightSize = 0.05f;

        //------------------------Audio Settings + Haptics -----------------------------

        [ModOption("Shoot Audio", tooltip = "Disables/Enables the shoot audio", category = "Audio + Haptics", order = 11)]
        public static bool shootAudio = true;

        [ModOption("Retract Audio", tooltip = "Disables/Enables the winch audio", category = "Audio + Haptics", order = 12)]
        public static bool retractAudio = true;

        [ModOption("Haptic Feedback", tooltip = "Disables/Enables haptic feedback", category = "Audio + Haptics")]
        public static bool hapticFeedback = true;


        //---------------------------Swing Settings----------------------------------

        [ModOption("Player Rotation", category = "Swing Settings")]
        public static bool playerRotation = false;

        [ModOptionFloatValues(0.1f, 10f, 0.1f)]
        [ModOption("Retract Speed", tooltip = "Speed in which in the winch pulls you up", interactionType = ModOption.InteractionType.Slider, category = "Swing Settings", order = 7)]
        public static float retractSpeed = 1.5f;

        [ModOptionFloatValues(1, 1000 ,1)]
        [ModOption("Spring", interactionType = ModOption.InteractionType.Slider, category = "Swing Settings", order = 8)]
        public static float Spring = 500;

        [ModOptionFloatValues(1, 1000, 1)]
        [ModOption("Damper", interactionType = ModOption.InteractionType.Slider, category = "Swing Settings", order = 9)]
        public static float Damper = 130;

        [ModOptionFloatValues(1, 1000, 1)]
        [ModOption("Mass Scale", interactionType = ModOption.InteractionType.Slider, category = "Swing Settings", order = 10)]
        public static float MassScale = 35;

        [ModOptionFloatValues(0.1f, 10f, 0.1f)]
        [ModOption("Extend Speed", tooltip = "Speed in which in the winch extends", interactionType = ModOption.InteractionType.Slider, category = "Swing Settings", order = 7)]
        public static float extendSpeed = 1.5f;


        //---------------------------Item Pull Settings----------------------------------

        [ModOption("Item Pulling", category = "Item Pull")]
        public static bool itemPulling = true;

        [ModOptionFloatValues(0.1f, 10f, 0.1f)]
        [ModOption("Retract Speed", tooltip = "Speed in which in the winch pulls items", interactionType = ModOption.InteractionType.Slider, category = "Item Pull", order = 7)]
        public static float itemRetractSpeed = 2f;

        [ModOptionFloatValues(1, 1000, 1)]
        [ModOption("Spring", interactionType = ModOption.InteractionType.Slider, category = "Item Pull", order = 8)]
        public static float itemSpring = 1000;

        [ModOptionFloatValues(1, 1000, 1)]
        [ModOption("Damper", interactionType = ModOption.InteractionType.Slider, category = "Item Pull", order = 9)]
        public static float itemDamper = 550;

        [ModOptionFloatValues(1, 1000, 1)]
        [ModOption("Mass Scale", interactionType = ModOption.InteractionType.Slider, category = "Item Pull", order = 10)]
        public static float itemMassScale = 35;


        [ModOptionFloatValues(0.1f, 10f, 0.1f)]
        [ModOption("Extend Speed", tooltip = "Speed in which in the winch extends", interactionType = ModOption.InteractionType.Slider, category = "Item Pull", order = 7)]
        public static float itemExtendSpeed = 2;


        //--------------------------Creature Pull Settings---------------------------------

        [ModOption("Creature Pulling", tooltip = "A work in progress version of creature pulling, I wanted to release the update but I want to work on other mods for a bit.", category = "Creature Pull")]
        public static bool creaturePull = true;

        [ModOptionFloatValues(0.1f, 10f, 0.1f)]
        [ModOption("Retract Speed", tooltip = "Speed in which in the winch pulls creatures", interactionType = ModOption.InteractionType.Slider, category = "Creature Pull", order = 7)]
        public static float creatureRetractSpeed = 2f;

        [ModOptionFloatValues(1, 10000, 1)]
        [ModOption("Spring", interactionType = ModOption.InteractionType.Slider, category = "Creature Pull", order = 8)]
        public static float creatureSpring = 1000;

        [ModOptionFloatValues(1, 10000, 1)]
        [ModOption("Damper", interactionType = ModOption.InteractionType.Slider, category = "Creature Pull", order = 9)]
        public static float creatureDamper = 550;

        [ModOptionFloatValues(1, 1000, 1)]
        [ModOption("Mass Scale", interactionType = ModOption.InteractionType.Slider, category = "Creature Pull", order = 10)]
        public static float creatureMassScale = 35;

        [ModOptionFloatValues(0.1f, 10f, 0.1f)]
        [ModOption("Extend Speed", tooltip = "Speed in which in the winch extends", interactionType = ModOption.InteractionType.Slider, category = "Creature Pull", order = 7)]
        public static float creatureExtendSpeed = 2;
    }
}
