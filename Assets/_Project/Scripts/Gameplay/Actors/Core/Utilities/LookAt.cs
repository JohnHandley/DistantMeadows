using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer( typeof( ReadOnlyAttribute ) )]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight ( SerializedProperty property, GUIContent label )
    {
        return EditorGUI.GetPropertyHeight( property, label, true );
    }

    public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label )
    {
        GUI.enabled = false;
        EditorGUI.PropertyField( position, property, label, true );
        GUI.enabled = true;
    }
}

public class LookAt : MonoBehaviour
{
    [Header( "Options" )]
    public List<Transform> targets = new List<Transform>();
    [Range( 1, 100 )]
    public float MaxDistance = 5.0f;
    [Range( 0.3f, 5.0f )]
    public float reactionTime = 4.0f;
    [Range( 1, 100 )]
    public float maxStareTime = 15.0f;
    [Range( 1, 25 )]
    public float maxStareVariance = 10.0f;
    [Range( 1, 25 )]
    public float attentionSpan = 5.0f;

    [Header( "Debug Logs" )]
    [SerializeField, ReadOnly]
    private bool _isLooking;
    [SerializeField, ReadOnly]
    private float _currentDistance;
    [SerializeField, ReadOnly]
    private int _currentTargetIndex = 0;
    [SerializeField, ReadOnly]
    private bool _isBored = false;
    [SerializeField, ReadOnly]
    private bool _hasNothingToSee = false;
    [SerializeField, ReadOnly]
    private bool _isTransitioningToNewTarget = false;
    [SerializeField, ReadOnly]
    private float _currentStareTime = 0f;
    [SerializeField, ReadOnly]
    private float _timeSinceTargetMoved = 0f;

    private float _transitionTime = 0;
    private float _weight;
    private float _body_weight;
    private float _head_weight;
    private float _currentStareVariance = 0f;
    private Vector3 _desiredLookAtPosition;
    private Vector3 _currentLookAtPosition;

    private Animator looker;

    private void Start ( )
    {
        Initialize();
    }

    private void Update ( )
    {
        UpdateLooking( Time.deltaTime );
    }

    private void OnAnimatorIK ( )
    {
        Look();
    }

    private void Initialize ( )
    {
        Animator localAnimator = GetComponent<Animator>();
        if ( localAnimator != null )
        {
            looker = localAnimator;
            _currentTargetIndex = UnityEngine.Random.Range( 0, targets.Count );
        }
    }

    private void UpdateLooking ( float delta )
    {
        if ( looker == null || targets == null || targets.Count == 0 )
        {
            return;
        }

        if ( _currentStareVariance == 0 )
        {
            _currentStareVariance = UnityEngine.Random.Range( 0f, maxStareVariance );
        }

        if ( _isTransitioningToNewTarget )
        {
            _transitionTime += delta;
            if ( _transitionTime > attentionSpan )
            {
                _transitionTime = 0;
                _isTransitioningToNewTarget = false;
            }
        }

        if ( TargetMoved() )
        {
            _timeSinceTargetMoved = 0;
            _hasNothingToSee = false;
            _isBored = false;
        }
        else
        {
            _timeSinceTargetMoved += delta;
        }

        _isLooking = ShouldLook();

        float lookSpeed = delta * reactionTime; // Following Player
        float lookAwaySpeed = delta * ( 2 * ( reactionTime / 5 ) ); // Losing focus from player
        float adjustSpeed = _isLooking ? lookSpeed : lookAwaySpeed;
        _weight = Mathf.Lerp( _weight, _isLooking ? 1f : 0f, adjustSpeed );
        _body_weight = Mathf.Lerp( _body_weight, _isLooking ? .1f : 0f, adjustSpeed );
        _head_weight = Mathf.Lerp( _head_weight, _isLooking ? 1f : 0f, adjustSpeed );
        _currentLookAtPosition = Vector3.Lerp( _currentLookAtPosition, _desiredLookAtPosition, adjustSpeed );

        if ( _isLooking )
        {
            _currentStareTime += delta;
            bool starredTooLong = _currentStareTime > ( maxStareTime + _currentStareVariance );
            _isBored = ( _timeSinceTargetMoved > attentionSpan );
            if ( starredTooLong || _isBored )
            {
                ChangeTargetAndStare();
            }
        }
        else
        {
            ChangeTargetAndStare();
        }
    }

    private void Look ( )
    {
        if ( looker == null || _currentLookAtPosition == null )
        {
            return;
        }

        Vector3 _currentTargetIndexPosition = _currentLookAtPosition;
        _currentTargetIndexPosition.y += 1.5f;

        looker.SetLookAtWeight( _weight, _body_weight, _head_weight, 1f, 0.45f );
        looker.SetLookAtPosition( _currentTargetIndexPosition );

        Debug.DrawRay(
            looker.GetBoneTransform( HumanBodyBones.Head ).position,
            _currentTargetIndexPosition - looker.GetBoneTransform( HumanBodyBones.Head ).position,
            Color.cyan
        );
    }

    private bool ShouldLook ( )
    {
        if ( looker == null || _desiredLookAtPosition == null || !TargetInFront() || _hasNothingToSee || _isTransitioningToNewTarget )
        {
            return false;
        }

        Vector3 headPosition = looker.GetBoneTransform( HumanBodyBones.Head ).position;
        _currentDistance = ( headPosition - _desiredLookAtPosition ).magnitude;
        if ( _currentDistance > MaxDistance )
        {
            return false;
        }

        return true;
    }

    private void ChangeTargetAndStare ( )
    {
        _currentStareTime = 0f;
        _currentStareVariance = UnityEngine.Random.Range( 0f, maxStareVariance );

        if ( _isBored || targets.Count == 1 )
        {
            _hasNothingToSee = true;
        }
        else
        {
            // Lets look at someone else
            int oldTarget = _currentTargetIndex;
            do
            {
                _currentTargetIndex = UnityEngine.Random.Range( 0, targets.Count );
            } while ( oldTarget == _currentTargetIndex ); // Guarantee we aren't switching to the same person
            _isTransitioningToNewTarget = true;
        }
    }

    private bool TargetInFront ( )
    {
        Vector3 directionToTarget = _currentLookAtPosition - transform.position;
        float angle = Vector3.Angle( transform.forward, directionToTarget );
        return Mathf.Abs( angle ) < 110;
    }

    private bool TargetMoved ( )
    {
        Vector3 oldPosition = _desiredLookAtPosition;
        _desiredLookAtPosition = targets[ _currentTargetIndex ].position;
        return _desiredLookAtPosition != oldPosition;
    }
}