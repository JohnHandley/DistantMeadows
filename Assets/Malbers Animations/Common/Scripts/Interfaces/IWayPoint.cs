using UnityEngine;

namespace MalbersAnimations
{
    public interface IWayPoint : IAITarget
    {
        /// <summary>Next Transform Target to go to </summary>
        Transform NextTarget();
        Transform WPTransform { get; }

        /// <summary>Wait time to go to the next Waypoint</summary>
        float WaitTime { get; }

        /// <summary>Call this method when someones arrives to the Waypoint</summary>
        void TargetArrived(GameObject target);
    }
}