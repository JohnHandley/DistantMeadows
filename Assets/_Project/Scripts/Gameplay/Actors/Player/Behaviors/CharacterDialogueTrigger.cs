using UnityEngine;

using DistantMeadows.Actors.Core.Behaviors;

public class CharacterDialogueTrigger : MonoBehaviour {
    private string dialogueTargetName;

    #region Unity States
    private void OnTriggerEnter ( Collider other ) {
        if ( other.CompareTag( "Villager" ) ) {
            CharacterStateManager dialogueTarget = other.GetComponent<CharacterStateManager>();
            if ( dialogueTarget && dialogueTarget.characterInfo ) {
                dialogueTargetName = dialogueTarget.characterInfo.name;
            }
        }
    }

    private void OnTriggerExit ( Collider other ) {
        if ( other.CompareTag( "Villager" ) ) {
            dialogueTargetName = null;
        }
    }
    #endregion

    public string GetDialogueTarget ( ) {
        return dialogueTargetName;
    }
}
