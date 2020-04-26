using UnityEngine;
using System.Collections;

namespace DistantMeadows.Actors.Core.Controllers {
    public class RaycastAbilityTriggerable : MonoBehaviour {

        // Set the number of hitpoints that this gun will take away from shot objects with a health script.
        [HideInInspector] public int damage = 1;

        // Distance in unity units over which the player can fire.
        [HideInInspector] public float range = 50f;

        // Amount of force which will be added to objects with a rigidbody shot by the player.
        [HideInInspector] public float hitForce = 100f;

        // Reference to the LineRenderer component which will display our laserline.
        [HideInInspector] public LineRenderer laserLine;

        // Holds a reference to the end object, marking the area to emit ability
        public Transform emitter;

        // Holds a reference to the current camera.
        private Camera cam;

        // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible.
        private WaitForSeconds duration = new WaitForSeconds( .07f );

        public void Initialize ( ) {
            //Get and store a reference to our LineRenderer component
            laserLine = GetComponent<LineRenderer>();

            //Get and store a reference to our Camera
            cam = GetComponentInParent<Camera>();
        }

        public void Fire ( ) {
            //Create a vector at the center of our camera's near clip plane.
            Vector3 rayOrigin = cam.ViewportToWorldPoint( new Vector3( .5f, .5f, 0 ) );

            //Draw a debug line which will show where our ray will eventually be
            Debug.DrawRay( rayOrigin, cam.transform.forward * range, Color.green );

            //Declare a raycast hit to store information about what our raycast has hit.
            RaycastHit hit;

            //Start our AbilityEffect coroutine to turn our laser line on and off
            StartCoroutine( AbilityEffect() );

            //Set the start position for our visual effect for our laser to the position of the emitter
            laserLine.SetPosition( 0, emitter.position );

            //Check if our raycast has hit anything
            if ( Physics.Raycast( rayOrigin, cam.transform.forward, out hit, range ) ) {
                //Set the end position for our laser line 
                laserLine.SetPosition( 1, hit.point );

                //Get a reference to a health script attached to the collider we hit
                // ShootableBox health = hit.collider.GetComponent<ShootableBox>(); TODO: Build Out Health System

                //If there was a health script attached
                //if ( health != null ) {
                //    //Call the damage function of that script, passing in our gunDamage variable
                //    health.Damage( damage );
                //}

                //Check if the object we hit has a rigidbody attached
                if ( hit.rigidbody != null ) {
                    //Add force to the rigidbody we hit, in the direction it was hit from
                    hit.rigidbody.AddForce( -hit.normal * hitForce );
                }
            } else {
                //if we did not hit anything, set the end of the line to a position directly away from
                laserLine.SetPosition( 1, cam.transform.forward * range );
            }
        }

        private IEnumerator AbilityEffect ( ) {
            //Turn on our line renderer
            laserLine.enabled = true;
            //Wait for .07 seconds
            yield return duration;

            //Deactivate our line renderer after waiting
            laserLine.enabled = false;
        }
    }
}
