/*
Integration Example:
Opsive Third Person Controller + Opsive Behavior Designer + 
Pixel Crushers Dialogue System for Unity

This example demonstrates:

- Basic integration of these three assets.
- Saving and loading TPC characters.
- Combat barks.


To play this example, you must import:

- Third Person Controller
- Behavior Designer
- Behavior Designer Movement Pack
- Dialogue System for Unity
- Dialogue System > Third Person Controller Support
- Dialogue System > Behavior Designer Support


These steps were taken to set up the scene:

- Created a dialogue database with two bark conversations:
  - Engage: Used when Top Guard starts attacking.
  - Damage: Used when Top Guard is hit.

- Added a Dialogue Manager and the Example Menu from the Dialogue System's
  Third Person Controller Support example.

- Doug: 
  - Added & configured a Dialogue System Third Person Controller Bridge.
  - Configured Doug's Pistol Hitscan to send the damage event "Damaged".

- Top Guard:
  - Added & configured a Dialogue System Third Person Controller Bridge.
  - Character Health: Ticked Deactivate on Death.
  - Added a bark UI child GameObject.
  - Added a Bark Trigger.
  - Added a Damaged Event and configured it to use Bark Trigger.
  - Added a Persistent Destructible.
  - Edited the Behavior. Changed the reference to the "See" external behavior
    to a custom copy named "See with Bark" that also barks the Engage bark.

*/