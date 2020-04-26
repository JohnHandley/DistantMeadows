using UnityEngine;
using UnityEditor;

namespace DistantMeadows.Editor {
    public class CreateVillagerData {
        [MenuItem( "Assets/Create/VillagerData" )]
        public static void CreateMyAsset ( ) {
            Actors.NPC.Models.VillagerData asset = ScriptableObject.CreateInstance<Actors.NPC.Models.VillagerData>();

            AssetDatabase.CreateAsset( asset, "Assets/New_Villager.asset" );
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }

}
