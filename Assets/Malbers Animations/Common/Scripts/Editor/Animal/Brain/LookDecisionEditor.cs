using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Controller.AI
{
    [CustomEditor(typeof(LookDecision))]
    [CanEditMultipleObjects]
    public class LookDecisionEditor : Editor
    {
        SerializedProperty
            Description, UnityTag, debugColor, zoneType, ZoneID, tags, LookRange, LookAngle, lookFor, MessageID, send, interval, ObstacleLayer, MoveToTarget, AssignTarget, GameObjectName,RemoveTarget;



        MonoScript script;
        private void OnEnable()
        {
            script = MonoScript.FromScriptableObject((ScriptableObject)target);

            Description = serializedObject.FindProperty("Description");
            tags = serializedObject.FindProperty("tags");
            RemoveTarget = serializedObject.FindProperty("RemoveTarget");
            GameObjectName = serializedObject.FindProperty("GameObjectName");
            UnityTag = serializedObject.FindProperty("UnityTag");
            LookRange = serializedObject.FindProperty("LookRange");
            zoneType = serializedObject.FindProperty("zoneType");
            lookFor = serializedObject.FindProperty("lookFor");
            MessageID = serializedObject.FindProperty("MessageID");
            send = serializedObject.FindProperty("send");
            interval = serializedObject.FindProperty("interval");
            LookAngle = serializedObject.FindProperty("LookAngle");
            ObstacleLayer = serializedObject.FindProperty("ObstacleLayer");
            AssignTarget = serializedObject.FindProperty("AssignTarget");
            MoveToTarget = serializedObject.FindProperty("MoveToTarget");
            debugColor = serializedObject.FindProperty("debugColor");
            ZoneID = serializedObject.FindProperty("ZoneID");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Look Decision for the AI Brain");

            EditorGUI.BeginChangeCheck();
            MalbersEditor.DrawScript(script);

            //   EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            {
                EditorGUILayout.PropertyField(Description);
                EditorGUILayout.PropertyField(MessageID);
                EditorGUILayout.PropertyField(send);
                EditorGUILayout.PropertyField(interval);
                EditorGUILayout.PropertyField(LookRange);
                EditorGUILayout.PropertyField(LookAngle);

                EditorGUILayout.PropertyField(lookFor);
                EditorGUILayout.PropertyField(ObstacleLayer);



                LookDecision.LookFor lookforval = (LookDecision.LookFor)lookFor.intValue;

                switch (lookforval)
                {
                    case LookDecision.LookFor.AnimalPlayer:
                        break;
                    case LookDecision.LookFor.Tags:
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(tags, true);
                        EditorGUI.indentLevel--;
                        break;
                    case LookDecision.LookFor.UnityTags:
                        EditorGUILayout.PropertyField(UnityTag);
                        break;
                    case LookDecision.LookFor.Zones:
                        EditorGUILayout.PropertyField(zoneType, new GUIContent("Zone Type", "Choose between a Mode or a State for the Zone"));
                        EditorGUILayout.PropertyField(ZoneID);
                        if (ZoneID.intValue < 1)
                        {
                            EditorGUILayout.HelpBox("If ID is set to Zero, it will search for any "+ ((ZoneType) zoneType.intValue).ToString() +" Zone.", MessageType.Info);
                        }
                        break;
                    case LookDecision.LookFor.GameObject:
                        EditorGUILayout.PropertyField(GameObjectName, new GUIContent("GameObject"));
                        break;
                    case LookDecision.LookFor.ClosestWayPoint:
                        break;
                    default:
                        break;
                }



                EditorGUILayout.PropertyField(AssignTarget);
                EditorGUILayout.PropertyField(MoveToTarget);

                if (!AssignTarget.boolValue)
                {
                    EditorGUILayout.PropertyField(RemoveTarget);
                }
                else
                {
                    RemoveTarget.boolValue = false;
                }

               
                EditorGUILayout.PropertyField(debugColor);

            }
            // EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Look Decision Inspector");
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}