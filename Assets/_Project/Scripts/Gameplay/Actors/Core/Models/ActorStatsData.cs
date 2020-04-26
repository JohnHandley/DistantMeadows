using UnityEngine;

[CreateAssetMenu( fileName = "ActorStatsData", menuName = "_Project/ScriptableObjects/ActorStatsDataScriptableObject", order = 1 )]
public class ActorStatsData : ScriptableObject {
    public float frontRayOffset = .5f;
    public float movementSpeed = 3.5f;
    public float RunSpeedModifier = 3;
    public float rollSpeed = 1;
    public float adaptSpeed = 1;
    public float rotationSpeed = 10;
    public float attackRotationSpeed = 3;
    public float groundDownDistanceOnAir = .4f;
    public float groundedSpeed = 0.1f;
    public float groundedDistanceRay = .5f;
    public float slopeLimit = 45f;
    public float slopeInfluence = 5f;
    public float jumpPower = 5f;
    public float jumpFalloff = 2f;
    public float rotateSpeed = 10f;
    public AnimationCurve locomotionSpeedCurve = AnimationCurve.Linear( 0, 0, 1, 1 );
}
