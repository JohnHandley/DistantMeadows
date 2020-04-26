using UnityEngine;
namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Set Stance")]
    public class SetStanceTask : MTask
    {
        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;
        public StanceID stance;
        
        public override void StartTask(MAnimalBrain brain, int index)
        {
            switch (affect)
            {
                case Affected.Self:
                    brain.Animal.Stance_Set(stance);
                    break;
                case Affected.Target:
                    brain.TargetAnimal?.Stance_Set(stance);
                    break;
            }
        }
    }
}
