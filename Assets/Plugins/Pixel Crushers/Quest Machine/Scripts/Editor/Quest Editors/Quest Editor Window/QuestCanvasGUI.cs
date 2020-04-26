// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace PixelCrushers.QuestMachine
{

    /// <summary>
    /// This class draws the GUI canvas for a Quest in the Quest Editor window.
    /// </summary>
    public class QuestCanvasGUI
    {

        #region Private Fields

        private const float CanvasWidth = 10000;
        private const float CanvasHeight = 10000;

        private float m_panX = 0;
        private float m_panY = 0;
        private float m_zoom = 1;
        private Vector2 m_mousePos;
        private Vector2 m_prevMousePos;
        private bool m_connecting = false;
        private bool m_dragging = false;

        private Quest m_quest;
        private SerializedObject m_questSerializedObject;
        private SerializedProperty m_nodeListProperty;
        private QuestEditorWizard m_wizard = null;
        private string jsonFilename;

        public SerializedObject questSerializedObject { get { return m_questSerializedObject; } }

        #endregion

        #region Setup

        public void AssignQuest(Quest quest)
        {
            m_quest = quest;
            m_questSerializedObject = (quest != null) ? new SerializedObject(quest) : null;
            m_nodeListProperty = (m_questSerializedObject != null) ? m_questSerializedObject.FindProperty("m_nodeList") : null;
        }

        public bool IsQuestAssigned()
        {
            return m_questSerializedObject != null && m_questSerializedObject.targetObject != null;
        }

        #endregion

        #region High Level Drawing

        public virtual void Draw(Rect position)
        {
            if (!AreReferencesValid()) return;
            m_questSerializedObject.Update();
            DrawQuestTitle();
            if (m_wizard != null)
            {
                DrawWizard();
            }
            else
            {
                DrawQuestCanvas();
            }
            m_questSerializedObject.ApplyModifiedProperties();
            DrawGearMenu(position);
        }

        protected bool AreReferencesValid()
        {
            UnityEngine.Assertions.Assert.IsNotNull(m_questSerializedObject, "Quest Machine: Internal error - m_questSerializedObject is null.");
            if (m_questSerializedObject == null) return false;
            UnityEngine.Assertions.Assert.IsNotNull(m_questSerializedObject.targetObject, "Quest Machine: Internal error - m_questSerializedObject target object is null.");
            if (m_questSerializedObject.targetObject == null) return false;
            UnityEngine.Assertions.Assert.IsNotNull(m_nodeListProperty, "Quest Machine: Internal error - m_nodeList property is null.");
            if (m_nodeListProperty == null) return false;
            UnityEngine.Assertions.Assert.IsNotNull(QuestEditorWindow.instance, "Quest Machine: Internal error - QuestEditorWindow.instance is null.");
            if (QuestEditorWindow.instance == null) return false;
            return true;
        }

        private void DrawQuestTitle()
        {
            var titleProperty = m_questSerializedObject.FindProperty("m_title");
            UnityEngine.Assertions.Assert.IsNotNull(titleProperty, "Quest Machine: Internal error - m_title property is null.");
            if (titleProperty == null) return;
            var displayName = StringFieldDrawer.GetStringFieldValue(titleProperty);
            if (string.IsNullOrEmpty(displayName))
            {
                var idProperty = m_questSerializedObject.FindProperty("m_id");
                if (idProperty != null) displayName = StringFieldDrawer.GetStringFieldValue(idProperty);
                if (string.IsNullOrEmpty(displayName)) displayName = m_questSerializedObject.targetObject.name;
            }
            if (m_quest.isInstance) displayName += " (runtime: " + m_quest.GetState() + ")";
            EditorGUILayout.LabelField(displayName, QuestEditorStyles.questNameGUIStyle);
        }

        private void DrawQuestCanvas()
        {
            var screenCoordsArea = new Rect(m_panX, m_panY, CanvasWidth, CanvasHeight);
            EditorGUIZoomArea.Begin(m_zoom, screenCoordsArea);
            try
            {
                HandleInput();
                DrawConnectionArrows();
                DrawNodes();
            }
            finally
            {
                EditorGUIZoomArea.End();
            }
        }

        #endregion

        #region Draw Content

        private void DrawConnectionArrows()
        {
            ValidateStartNode();
            for (int i = 0; i < m_nodeListProperty.arraySize; i++)
            {
                var nodeProperty = m_nodeListProperty.GetArrayElementAtIndex(i);
                if (nodeProperty == null) continue;
                var nodeRect = GetCanvasRect(nodeProperty);
                var parentRect = new Rect(nodeRect.xMin, nodeRect.yMax - EditorGUIUtility.singleLineHeight, nodeRect.width, EditorGUIUtility.singleLineHeight);
                var childIndexListProperty = nodeProperty.FindPropertyRelative("m_childIndexList");
                DrawConnectionsToChildren(parentRect, childIndexListProperty);
            }
        }

        private void DrawConnectionsToChildren(Rect sourceRect, SerializedProperty childIndexListProperty)
        {
            UnityEngine.Assertions.Assert.IsNotNull(childIndexListProperty, "Quest Machine: Internal error - m_childIndexList property is null.");
            if (childIndexListProperty == null) return;
            for (int i = 0; i < childIndexListProperty.arraySize; i++)
            {
                var childIndex = childIndexListProperty.GetArrayElementAtIndex(i).intValue;
                if (0 <= childIndex && childIndex < m_nodeListProperty.arraySize)
                {
                    var destNodeRect = GetCanvasRect(m_nodeListProperty.GetArrayElementAtIndex(childIndex));
                    var destRect = new Rect(destNodeRect.x, destNodeRect.y, destNodeRect.width, EditorGUIUtility.singleLineHeight);
                    DrawNodeCurve(sourceRect, destRect, QuestEditorStyles.ConnectorColor);
                }
            }
        }

        private void ValidateStartNode()
        {
            if (m_nodeListProperty.arraySize > 0) return;
            m_questSerializedObject.ApplyModifiedProperties();
            var quest = m_questSerializedObject.targetObject as Quest;
            if (quest != null) quest.Initialize();
            m_questSerializedObject.Update();
        }

        private void DrawNodes()
        {
            QuestEditorWindow.instance.BeginWindows();
            try
            {
                for (int i = 0; i < m_nodeListProperty.arraySize; i++)
                {
                    DrawNode(i, m_nodeListProperty.GetArrayElementAtIndex(i));
                }
            }
            finally
            {
                QuestEditorWindow.instance.EndWindows();
            }
        }

        private void DrawNode(int nodeIndex, SerializedProperty nodeProperty)
        {
            if (nodeProperty == null) return;
            var stateProperty = nodeProperty.FindPropertyRelative("m_state");
            UnityEngine.Assertions.Assert.IsNotNull(stateProperty, "Quest Machine: Internal error - quest node m_state property is null.");
            if (stateProperty == null) return;
            var nodeState = (QuestNodeState)stateProperty.enumValueIndex;
            var prevContentColor = GUI.contentColor;
            var prevBkColor = GUI.backgroundColor;
            switch (nodeState)
            {
                case QuestNodeState.Inactive:
                    // Use default color; don't set GUI.backgroundColor.
                    break;
                case QuestNodeState.Active:
                    GUI.backgroundColor = QuestEditorStyles.ActiveNodeColor;
                    break;
                case QuestNodeState.True:
                    GUI.backgroundColor = QuestEditorStyles.TrueNodeColor;
                    break;
            }
            var nodeRectProperty = GetCanvasRectProperty(nodeProperty);
            if (nodeRectProperty != null)
            {
                nodeRectProperty.rectValue = GUI.Window(nodeIndex, nodeRectProperty.rectValue, DrawNodeWindow, string.Empty, QuestEditorStyles.questNodeWindowGUIStyle);
            }
            GUI.contentColor = prevContentColor;
            GUI.backgroundColor = prevBkColor;
        }

        private void DrawNodeWindow(int id)
        {
            //UnityEngine.Assertions.Assert.IsTrue(0 <= id && id < m_nodeListProperty.arraySize, "Quest Machine: Internal error - node ID is outside m_nodeList range.");
            if (!(0 <= id && id < m_nodeListProperty.arraySize)) return;
            try
            {
                var rect = EditorGUILayout.GetControlRect();
                var nodeProperty = m_nodeListProperty.GetArrayElementAtIndex(id);
                UnityEngine.Assertions.Assert.IsNotNull(nodeProperty, "Quest Machine: Internal error - m_nodeList[id] property is null.");
                if (nodeProperty == null) return;
                var canvasRect = GetCanvasRect(nodeProperty);
                var nodeType = GetNodeType(nodeProperty);
                var text = GetNodeText(nodeProperty);
                var barWidth = rect.width + (EditorGUIUtility.isProSkin ? 5 : 7); // A bit of pixel fine-tuning.
                var barRect = new Rect(rect.x - 3, rect.y - 3, barWidth, QuestEditorStyles.nodeBarHeight);
                var textRect = new Rect(barRect.x - 1, barRect.y - 1, barRect.width, barRect.height);
                GUI.Label(barRect, text, QuestEditorStyles.GetNodeBarGUIStyle(nodeType));
                GUI.Label(textRect, text, QuestEditorStyles.nodeTextGUIStyle);
                if (nodeType != QuestNodeType.Start)
                {
                    GUI.DrawTexture(new Rect(rect.x + (rect.width / 2) - (QuestEditorStyles.connectorImage.width / 2), rect.y - 16, QuestEditorStyles.connectorImage.width, QuestEditorStyles.connectorImage.height), QuestEditorStyles.connectorImage);
                }
                if (nodeType != QuestNodeType.Success && nodeType != QuestNodeType.Failure)
                {
                    GUI.DrawTexture(new Rect(rect.x + (rect.width / 2) - (QuestEditorStyles.connectorImage.width / 2), rect.y + canvasRect.height - 23 - QuestEditorStyles.connectorImage.height, QuestEditorStyles.connectorImage.width, QuestEditorStyles.connectorImage.height), QuestEditorStyles.connectorImage);
                }
            }
            finally
            {
                if (!m_connecting) GUI.DragWindow();
            }
        }

        private string GetNodeText(SerializedProperty nodeProperty)
        {
            if (nodeProperty == null) return string.Empty;
            var internalNameProperty = nodeProperty.FindPropertyRelative("m_internalName");
            UnityEngine.Assertions.Assert.IsNotNull(internalNameProperty, "Quest Machine: Internal error - m_internalName property is null.");
            if (internalNameProperty == null) return string.Empty;
            var text = StringFieldDrawer.GetStringFieldValue(internalNameProperty);
            if (string.IsNullOrEmpty(text))
            {
                var idProperty = nodeProperty.FindPropertyRelative("m_id");
                UnityEngine.Assertions.Assert.IsNotNull(idProperty, "Quest Machine: Internal error - m_id property is null.");
                if (idProperty == null) return string.Empty;
                text = StringFieldDrawer.GetStringFieldValue(idProperty);
            }
            return text;
        }

        private QuestNodeType GetNodeType(SerializedProperty nodeProperty)
        {
            if (nodeProperty == null) return QuestNodeType.Start;
            var nodeTypeProperty = nodeProperty.FindPropertyRelative("m_nodeType");
            UnityEngine.Assertions.Assert.IsNotNull(nodeTypeProperty, "Quest Machine: Internal error - m_nodeType property is null.");
            if (nodeTypeProperty == null) return QuestNodeType.Start;
            return (QuestNodeType)nodeTypeProperty.enumValueIndex;
        }

        private SerializedProperty GetCanvasRectProperty(SerializedProperty nodeProperty)
        {
            if (nodeProperty == null) return null;
            var canvasRectProperty = nodeProperty.FindPropertyRelative("m_canvasRect");
            UnityEngine.Assertions.Assert.IsNotNull(canvasRectProperty, "Quest Machine: Internal error - m_canvasRect property is null.");
            if (canvasRectProperty == null) return null;
            if (canvasRectProperty.rectValue.width < QuestEditorStyles.nodeWidth)
            {
                var nodeType = GetNodeType(nodeProperty);
                var isShortNode = (nodeType == QuestNodeType.Success || nodeType == QuestNodeType.Failure);
                var nodeHeight = isShortNode ? QuestEditorStyles.shortNodeHeight : QuestEditorStyles.nodeHeight;
                canvasRectProperty.rectValue = new Rect(canvasRectProperty.rectValue.x, canvasRectProperty.rectValue.y, QuestEditorStyles.nodeWidth, nodeHeight);
            }
            return canvasRectProperty;
        }

        private Rect GetCanvasRect(SerializedProperty parentProperty)
        {
            var canvasRectProperty = GetCanvasRectProperty(parentProperty);
            return (canvasRectProperty != null) ? canvasRectProperty.rectValue : new Rect(50, 30, QuestEditorStyles.nodeWidth, QuestEditorStyles.nodeHeight);
        }

        #endregion

        #region Handle Input

        private void HandleInput()
        {
            m_prevMousePos = m_mousePos;
            m_mousePos = Event.current.mousePosition;

            var inputClickNotConnecting = (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt && !m_connecting);
            var inputEndConnecting = (Event.current.type == EventType.MouseUp && Event.current.button == 0 && m_connecting);
            var inputCancelConnecting = (Event.current.type == EventType.MouseDown && Event.current.button == 1 && m_connecting);
            var inputBeginDragging = (Event.current.type == EventType.MouseDown && ((Event.current.button == 2) || (Event.current.button == 0 && Event.current.alt)));
            var inputEndDragging = (Event.current.type == EventType.MouseUp && ((Event.current.button == 2) || (Event.current.button == 0)) && m_dragging);
            var inputContextMenu = ((Event.current.type == EventType.MouseDown) && ((Event.current.button == 1) || (Event.current.button == 0 && Event.current.control)));
            var inputWheelZoom = (Event.current.type == EventType.ScrollWheel && !QuestEditorPrefs.zoomLock);
            var deleteKey = (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete && QuestEditorWindow.selectedNodeListIndex != -1);

            if (inputCancelConnecting)
            {
                m_connecting = false;
            }
            else if (inputContextMenu)
            {
                ShowContextMenu();
                Event.current.Use();
            }
            else if (inputEndConnecting)
            {
                EndConnection();
                Event.current.Use();
            }
            else if (inputClickNotConnecting)
            {
                ClickOnCanvas();
            }
            else if (inputBeginDragging)
            {
                m_dragging = true;
            }
            else if (inputEndDragging)
            {
                m_dragging = false;
            }
            else if (inputWheelZoom)
            {
                ZoomWithWheel();
                Event.current.Use();
            }
            else if (deleteKey)
            {
                DeleteNode(QuestEditorWindow.selectedNodeListIndex);
            }
            if (m_dragging)
            {
                PanWithMouse();
            }
            if (m_connecting && QuestEditorWindow.selectedNodeListIndex != -1)
            {
                DrawInProgressConnectorLine();
            }
        }

        private void ZoomWithWheel()
        {
            m_zoom -= Event.current.delta.y / 100f;
        }

        private void PanWithMouse()
        {
            var dX = (m_mousePos.x - m_prevMousePos.x) * m_zoom;
            var dY = (m_mousePos.y - m_prevMousePos.y) * m_zoom;
            if (Mathf.Abs(dX) > 0 || Mathf.Abs(dY) > 0)
            {
                m_panX += dX;
                m_panY += dY;
                QuestEditorWindow.RepaintNow();
            }
        }

        private void ClickOnCanvas()
        {
            QuestEditorWindow.selectedNodeListIndex = -1;
            for (int i = 0; i < m_nodeListProperty.arraySize; i++)
            {
                var nodeProperty = m_nodeListProperty.GetArrayElementAtIndex(i);
                var nodeRect = GetCanvasRect(nodeProperty);
                if (nodeRect.Contains(m_mousePos))
                {
                    QuestEditorWindow.selectedNodeListIndex = i;
                    var connectRect = new Rect(nodeRect.x, nodeRect.y + nodeRect.height - QuestEditorStyles.connectorImage.height - 8, nodeRect.width, QuestEditorStyles.connectorImage.height + 8);
                    if (connectRect.Contains(m_mousePos)) m_connecting = true;
                    break;
                }
            }
            QuestEditorWindow.SetSelectionToQuest();
            QuestEditorWindow.RepaintCurrentEditorNow();
        }

        private void DrawInProgressConnectorLine()
        {
            Rect mouseRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 10, 10);
            var selectedNodeProperty = m_nodeListProperty.GetArrayElementAtIndex(QuestEditorWindow.selectedNodeListIndex);
            var selectedNodeRect = GetCanvasRect(selectedNodeProperty);
            var sourceRect = new Rect(selectedNodeRect.x, selectedNodeRect.y + selectedNodeRect.height - EditorGUIUtility.singleLineHeight, selectedNodeRect.width, EditorGUIUtility.singleLineHeight);
            DrawNodeCurve(sourceRect, mouseRect, QuestEditorStyles.NewConnectorColor);
            QuestEditorWindow.RepaintNow();
        }

        private void EndConnection()
        {
            int clickedIndex = -1;
            for (int i = 0; i < m_nodeListProperty.arraySize; i++)
            {
                var nodeProperty = m_nodeListProperty.GetArrayElementAtIndex(i);
                var nodeRect = GetCanvasRect(nodeProperty);
                if (nodeRect.Contains(m_mousePos))
                {
                    clickedIndex = i;
                    break;
                }
            }
            if (clickedIndex != -1 && clickedIndex != QuestEditorWindow.selectedNodeListIndex)
            {
                AddConnection(QuestEditorWindow.selectedNodeListIndex, clickedIndex);
            }
            m_connecting = false;
        }

        private void AddConnection(int sourceIndex, int destIndex)
        {
            if (sourceIndex == destIndex) return; // Disallow parenting to self.
            if (!(0 <= sourceIndex && sourceIndex < m_nodeListProperty.arraySize && 0 <= destIndex && destIndex < m_nodeListProperty.arraySize)) return;
            var nodeProperty = m_nodeListProperty.GetArrayElementAtIndex(sourceIndex);
            UnityEngine.Assertions.Assert.IsNotNull(nodeProperty, "Quest Machine: Internal error - node property is null in AddConnection().");
            if (nodeProperty == null) return;
            var childIndexListProperty = nodeProperty.FindPropertyRelative("m_childIndexList");
            UnityEngine.Assertions.Assert.IsNotNull(childIndexListProperty, "Quest Machine: Internal error - m_childIndexList property is null in AddConnection().");
            if (childIndexListProperty == null) return;
            for (int i = 0; i < childIndexListProperty.arraySize; i++)
            {
                if (childIndexListProperty.GetArrayElementAtIndex(i).intValue == destIndex)
                {
                    return; // Don't allow duplicates.
                }
            }
            childIndexListProperty.arraySize++;
            childIndexListProperty.GetArrayElementAtIndex(childIndexListProperty.arraySize - 1).intValue = destIndex;
        }

        private void ClearConnections(int sourceIndex)
        {
            ClearConnections(m_nodeListProperty.GetArrayElementAtIndex(sourceIndex));
        }

        private void ClearConnections(SerializedProperty nodeProperty)
        {
            if (nodeProperty == null) return;
            var childIndexListProperty = nodeProperty.FindPropertyRelative("m_childIndexList");
            UnityEngine.Assertions.Assert.IsNotNull(childIndexListProperty, "Quest Machine: Internal error - m_childIndexList property is null in ClearConnections().");
            if (childIndexListProperty == null) return;
            childIndexListProperty.arraySize = 0;
        }

        private void ShowContextMenu()
        {
            int clickedIndex = -1;
            m_connecting = false;
            for (int i = 0; i < m_nodeListProperty.arraySize; i++)
            {
                var nodeProperty = m_nodeListProperty.GetArrayElementAtIndex(i);
                var nodeRect = GetCanvasRect(nodeProperty);
                if (nodeRect.Contains(m_mousePos))
                {
                    clickedIndex = i;
                    QuestEditorWindow.selectedNodeListIndex = i;
                    break;
                }
            }
            GenericMenu menu = new GenericMenu();
            if (Application.isPlaying && m_quest.isInstance)
            {
                menu.AddItem(new GUIContent("Set State/Inactive"), false, ContextCallback, new CallbackArgs(CallbackType.SetState, clickedIndex, QuestNodeState.Inactive));
                menu.AddItem(new GUIContent("Set State/Active"), false, ContextCallback, new CallbackArgs(CallbackType.SetState, clickedIndex, QuestNodeState.Active));
                menu.AddItem(new GUIContent("Set State/True"), false, ContextCallback, new CallbackArgs(CallbackType.SetState, clickedIndex, QuestNodeState.True));
            }
            else
            {
                if (clickedIndex >= 0)
                {
                    menu.AddItem(new GUIContent("Clear Connections"), false, ContextCallback, new CallbackArgs(CallbackType.ClearConnections, clickedIndex));
                    if (QuestEditorWindow.selectedNodeListIndex > 0)
                    {
                        menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, new CallbackArgs(CallbackType.Delete, clickedIndex));
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Delete Node"));
                    }
                    menu.AddSeparator("");
                }
                menu.AddItem(new GUIContent("New Node/Passthrough"), false, ContextCallback, new CallbackArgs(CallbackType.Add, QuestNodeType.Passthrough, Event.current.mousePosition, clickedIndex));
                menu.AddItem(new GUIContent("New Node/Condition"), false, ContextCallback, new CallbackArgs(CallbackType.Add, QuestNodeType.Condition, Event.current.mousePosition, clickedIndex));
                menu.AddItem(new GUIContent("New Node/Success"), false, ContextCallback, new CallbackArgs(CallbackType.Add, QuestNodeType.Success, Event.current.mousePosition, clickedIndex));
                menu.AddItem(new GUIContent("New Node/Failure"), false, ContextCallback, new CallbackArgs(CallbackType.Add, QuestNodeType.Failure, Event.current.mousePosition, clickedIndex));
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Wizard/Counter Requirement..."), false, OpenCounterRequirementWizard, clickedIndex);
            menu.AddItem(new GUIContent("Wizard/Message Requirement..."), false, OpenMessageRequirementWizard, clickedIndex);
            menu.AddItem(new GUIContent("Wizard/Return to QuestGiver..."), false, OpenReturnToQuestGiverWizard, clickedIndex);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Arrange Nodes"), false, ContextCallback, new CallbackArgs(CallbackType.ArrangeNodes, 0));
            menu.ShowAsContext();
        }

        private enum CallbackType { Add, Delete, ClearConnections, SetState, ArrangeNodes }

        private struct CallbackArgs
        {
            public CallbackType callbackType;
            public int clickedIndex;
            public QuestNodeType questNodeType;
            public Vector2 mousePosition;
            public QuestNodeState newState;

            public CallbackArgs(CallbackType callbackType, int clickedIndex)
            {
                this.callbackType = callbackType;
                this.clickedIndex = clickedIndex;
                this.questNodeType = QuestNodeType.Success;
                this.mousePosition = Vector2.zero;
                this.newState = QuestNodeState.Inactive;
            }

            public CallbackArgs(CallbackType callbackType, QuestNodeType questNodeType, Vector2 mousePosition, int clickedIndex)
            {
                this.callbackType = callbackType;
                this.clickedIndex = clickedIndex;
                this.questNodeType = questNodeType;
                this.mousePosition = mousePosition;
                this.newState = QuestNodeState.Inactive;
            }

            public CallbackArgs(CallbackType callbackType, int clickedIndex, QuestNodeState newState)
            {
                this.callbackType = callbackType;
                this.clickedIndex = -1;
                this.questNodeType = QuestNodeType.Success;
                this.mousePosition = Vector2.zero;
                this.newState = newState;
            }
        }

        void ContextCallback(object obj)
        {
            if (obj == null || obj.GetType() != typeof(CallbackArgs) || m_nodeListProperty == null) return;

            var args = (CallbackArgs)obj;

            m_nodeListProperty.serializedObject.Update();

            switch (args.callbackType)
            {
                case CallbackType.Add:
                    AddNode(args.questNodeType, args.mousePosition, args.clickedIndex);
                    break;
                case CallbackType.ClearConnections:
                    ClearConnections(args.clickedIndex);
                    break;
                case CallbackType.Delete:
                    DeleteNode(args.clickedIndex);
                    break;
                case CallbackType.SetState:
                    var quest = m_questSerializedObject.targetObject as Quest;
                    if (quest != null && quest.nodeList != null && 0 <= QuestEditorWindow.selectedNodeListIndex && QuestEditorWindow.selectedNodeListIndex < quest.nodeList.Count)
                    {
                        var node = quest.nodeList[QuestEditorWindow.selectedNodeListIndex];
                        if (node == null) break;
                        node.SetState(args.newState, Application.isPlaying);
                    }
                    break;
                case CallbackType.ArrangeNodes:
                    ConfirmAndArrangeNodes();
                    break;
            }
            m_nodeListProperty.serializedObject.ApplyModifiedProperties();
        }

        private void ConfirmAndArrangeNodes()
        {
            if (!EditorUtility.DisplayDialog("Arrange Nodes", "Auto-layout nodes?", "OK", "Cancel")) return;
            QuestEditorUtility.ArrangeNodes(m_questSerializedObject.targetObject as Quest);
            QuestEditorWindow.RepaintNow();
        }

        private void AddNode(QuestNodeType questNodeType, Vector2 mousePosition, int parentIndex)
        {
            var parentNodeProperty = (parentIndex >= 0) ? m_nodeListProperty.GetArrayElementAtIndex(parentIndex) : null;
            m_nodeListProperty.arraySize++;
            QuestEditorWindow.selectedNodeListIndex = m_nodeListProperty.arraySize - 1;
            var childIndex = m_nodeListProperty.arraySize - 1;
            var nodeProperty = m_nodeListProperty.GetArrayElementAtIndex(childIndex);
            UnityEngine.Assertions.Assert.IsNotNull(nodeProperty, "Quest Machine: Internal error - node property is null in AddNode().");
            if (nodeProperty == null) return;
            var idProperty = nodeProperty.FindPropertyRelative("m_id");
            UnityEngine.Assertions.Assert.IsNotNull(idProperty, "Quest Machine: Internal error - m_id property is null in AddNode().");
            if (idProperty == null) return;
            var internalNameProperty = nodeProperty.FindPropertyRelative("m_internalName");
            UnityEngine.Assertions.Assert.IsNotNull(internalNameProperty, "Quest Machine: Internal error - m_internalName property is null in AddNode().");
            if (internalNameProperty == null) return;
            var stateProperty = nodeProperty.FindPropertyRelative("m_state");
            UnityEngine.Assertions.Assert.IsNotNull(stateProperty, "Quest Machine: Internal error - m_state property is null in AddNode().");
            if (stateProperty == null) return;
            var nodeTypeProperty = nodeProperty.FindPropertyRelative("m_nodeType");
            UnityEngine.Assertions.Assert.IsNotNull(nodeTypeProperty, "Quest Machine: Internal error - m_nodeType property is null in AddNode().");
            if (nodeTypeProperty == null) return;
            var isOptionalProperty = nodeProperty.FindPropertyRelative("m_isOptional");
            UnityEngine.Assertions.Assert.IsNotNull(isOptionalProperty, "Quest Machine: Internal error - m_isOptional property is null in AddNode().");
            if (isOptionalProperty == null) return;
            var stateInfoListProperty = nodeProperty.FindPropertyRelative("m_stateInfoList");
            UnityEngine.Assertions.Assert.IsNotNull(stateInfoListProperty, "Quest Machine: Internal error - m_stateInfoList property is null in AddNode().");
            if (stateInfoListProperty == null) return;
            var conditionSetProperty = nodeProperty.FindPropertyRelative("m_conditionSet");
            UnityEngine.Assertions.Assert.IsNotNull(conditionSetProperty, "Quest Machine: Internal error - m_conditionSet property is null in AddNode().");
            if (conditionSetProperty == null) return;
            var conditionListProperty = conditionSetProperty.FindPropertyRelative("m_conditionList");
            UnityEngine.Assertions.Assert.IsNotNull(conditionListProperty, "Quest Machine: Internal error - m_conditionList property is null in AddNode().");
            if (conditionListProperty == null) return;
            var conditionCountModeProperty = conditionSetProperty.FindPropertyRelative("m_conditionCountMode");
            UnityEngine.Assertions.Assert.IsNotNull(conditionCountModeProperty, "Quest Machine: Internal error - m_conditionCountMode property is null in AddNode().");
            if (conditionCountModeProperty == null) return;
            var canvasRectProperty = nodeProperty.FindPropertyRelative("m_canvasRect");
            UnityEngine.Assertions.Assert.IsNotNull(canvasRectProperty, "Quest Machine: Internal error - m_canvasRect property is null in AddNode().");
            if (canvasRectProperty == null) return;
            var initialName = (questNodeType == QuestNodeType.Success) ? "Success"
                : ((questNodeType == QuestNodeType.Failure) ? "Failure"
                   : questNodeType.ToString() + " " + (m_nodeListProperty.arraySize - 1));
            StringFieldDrawer.SetStringFieldValue(idProperty, initialName);
            StringFieldDrawer.SetStringFieldValue(internalNameProperty, string.Empty);
            stateProperty.enumValueIndex = (int)QuestState.WaitingToStart;
            nodeTypeProperty.enumValueIndex = (int)questNodeType;
            isOptionalProperty.boolValue = false;
            stateInfoListProperty.ClearArray();
            conditionListProperty.ClearArray();
            conditionCountModeProperty.enumValueIndex = (int)ConditionCountMode.All;
            ClearConnections(nodeProperty);
            var height = (questNodeType == QuestNodeType.Success || questNodeType == QuestNodeType.Failure) ? QuestEditorStyles.shortNodeHeight : QuestEditorStyles.nodeHeight;
            var rect = (parentNodeProperty != null) ? GetRectForNewChild(parentNodeProperty, height)
                : new Rect(mousePosition.x, mousePosition.y, QuestEditorStyles.nodeWidth, height);
            canvasRectProperty.rectValue = rect;
            if (parentNodeProperty != null) AddConnection(parentIndex, childIndex);
        }

        private Rect GetRectForNewChild(SerializedProperty parentNodeProperty, float height)
        {
            var canvasRectProperty = parentNodeProperty.FindPropertyRelative("m_canvasRect");
            UnityEngine.Assertions.Assert.IsNotNull(canvasRectProperty, "Quest Machine: Internal error - parent's m_canvasRect property is null in AddNode().");
            if (canvasRectProperty == null) return new Rect(0, 0, QuestEditorStyles.nodeWidth, height);
            return new Rect(canvasRectProperty.rectValue.x, canvasRectProperty.rectValue.y + canvasRectProperty.rectValue.height + 20f, QuestEditorStyles.nodeWidth, height);
        }

        private void DeleteNode(int index)
        {
            if (index <= 0) return;
            if (!EditorUtility.DisplayDialog("Delete Quest Node", "Are you sure you want to delete this quest node?", "OK", "Cancel")) return;
            RemoveParentConnectionsTo(index);
            DeleteNodeSubassets(index);
            m_nodeListProperty.DeleteArrayElementAtIndex(index);
            QuestEditorWindow.selectedNodeListIndex = -1;
            QuestEditorWindow.RepaintNow();
        }

        private void RemoveParentConnectionsTo(int childIndex)
        {
            if (m_nodeListProperty == null) return;
            for (int i = 0; i < m_nodeListProperty.arraySize; i++)
            {
                var nodeProperty = m_nodeListProperty.GetArrayElementAtIndex(i);
                if (nodeProperty == null) continue;
                var childIndexListProperty = nodeProperty.FindPropertyRelative("m_childIndexList");
                UnityEngine.Assertions.Assert.IsNotNull(childIndexListProperty, "Quest Machine: Internal error - m_childIndexList property is null in RemoveParentConnections().");
                if (childIndexListProperty == null) continue;

                if (childIndexListProperty.arraySize > 0)
                {
                    for (int j = childIndexListProperty.arraySize - 1; j >= 0; j--)
                    {
                        if (childIndexListProperty.GetArrayElementAtIndex(j).intValue == childIndex)
                        {
                            childIndexListProperty.DeleteArrayElementAtIndex(j);
                        }
                    }
                }
            }
        }

        private void DrawNodeCurve(Rect start, Rect end, Color color)
        {
            Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
            Vector3 startTan = startPos + 24 * Vector3.up;
            Vector3 endTan = endPos + -24 * Vector3.up;
            Color shadowCol = new Color(0, 0, 0, .06f);
            for (int i = 0; i < 3; i++)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 2);
        }

        #endregion

        #region Wizards

        private void OpenCounterRequirementWizard(object data)
        {
            m_wizard = new QuestEditorCounterWizard((int)data);
        }

        private void OpenMessageRequirementWizard(object data)
        {
            m_wizard = new QuestEditorMessageWizard((int)data);
        }

        private void OpenReturnToQuestGiverWizard(object data)
        {
            m_wizard = new QuestEditorReturnWizard((int)data);
        }

        private void DrawWizard()
        {
            if (m_questSerializedObject == null)
            {
                m_wizard = null;
                return;
            }
            m_questSerializedObject.ApplyModifiedProperties();
            if (!m_wizard.Draw())
            {
                m_wizard = null;
            }
            m_questSerializedObject.Update();
        }

        #endregion

        #region Gear Menu

        protected virtual void DrawGearMenu(Rect position)
        {
            if (MoreEditorGuiUtility.DoGearMenu(new Rect(position.width - 2 - MoreEditorGuiUtility.GearWidth, 2, MoreEditorGuiUtility.GearWidth, MoreEditorGuiUtility.GearHeight)))
            {
                var menu = new GenericMenu();
                AddCanvasControlGearMenuItems(menu);
                AddExtraGearMenuItems(menu);
                if (Application.isPlaying) AddRuntimeGearMenuItems(menu);
                AddQuestListGearMenuItems(menu);
                menu.ShowAsContext();
            }
        }

        protected virtual void AddCanvasControlGearMenuItems(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Pan/Top Left"), false, PanTopLeft, null);
            menu.AddItem(new GUIContent("Zoom/Lock"), QuestEditorPrefs.zoomLock, ToggleZoomLock, null);
            menu.AddItem(new GUIContent("Zoom/25%"), false, Zoom, 0.25f);
            menu.AddItem(new GUIContent("Zoom/50%"), false, Zoom, 0.5f);
            menu.AddItem(new GUIContent("Zoom/100%"), false, Zoom, 1f);
            menu.AddItem(new GUIContent("Zoom/150%"), false, Zoom, 1.5f);
            menu.AddItem(new GUIContent("Zoom/200%"), false, Zoom, 2f);
        }

        protected virtual void AddExtraGearMenuItems(GenericMenu menu)
        {
            if (m_quest == null)
            {
                menu.AddDisabledItem(new GUIContent("Text/Tags to Text Table..."));
                menu.AddDisabledItem(new GUIContent("Text/Move Text to Text Table..."));
                menu.AddDisabledItem(new GUIContent("Export\u2215Import/Export to JSON..."));
                menu.AddDisabledItem(new GUIContent("Export\u2215Import/Import from JSON..."));
            }
            else
            {
                menu.AddItem(new GUIContent("Text/Tags to Text Table..."), false, OpenTagsToTextTableWizard);
                menu.AddItem(new GUIContent("Text/Move Text to Text Table..."), false, OpenTextToTextTableWizard);
                menu.AddItem(new GUIContent("Export\u2215Import/Export to JSON..."), false, ExportToJson);
                menu.AddItem(new GUIContent("Export\u2215Import/Import from JSON..."), false, ImportFromJson);
                if (!Application.isPlaying && m_wizard == null)
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Wizard/Counter Requirement..."), false, OpenCounterRequirementWizard, -1);
                    menu.AddItem(new GUIContent("Wizard/Message Requirement..."), false, OpenMessageRequirementWizard, -1);
                    menu.AddItem(new GUIContent("Wizard/Return to QuestGiver..."), false, OpenReturnToQuestGiverWizard, -1);
                }
            }
            if (m_quest == null || m_quest.isInstance)
            {
                menu.AddDisabledItem(new GUIContent("Debug/Delete Unused Subassets"));
            }
            else
            {
                menu.AddItem(new GUIContent("Debug/Delete Unused Subassets"), false, DeleteUnusedSubassets);
            }
        }

        protected virtual void AddRuntimeGearMenuItems(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Refresh Frequency/0.5 sec", "Refresh window every 0.5 second."),
                Mathf.Approximately(0.5f, QuestEditorPrefs.runtimeRepaintFrequency), SetRuntimeRepaintFrequency, 0.5f);
            menu.AddItem(new GUIContent("Refresh Frequency/1 sec", "Refresh window every 1 second."),
                Mathf.Approximately(1f, QuestEditorPrefs.runtimeRepaintFrequency), SetRuntimeRepaintFrequency, 1f);
            menu.AddItem(new GUIContent("Refresh Frequency/5 sec", "Refresh window every 5 seconds."),
                Mathf.Approximately(5f, QuestEditorPrefs.runtimeRepaintFrequency), SetRuntimeRepaintFrequency, 5f);
            menu.AddItem(new GUIContent("Refresh Frequency/10 sec", "Refresh window every 10 seconds."),
                Mathf.Approximately(10f, QuestEditorPrefs.runtimeRepaintFrequency), SetRuntimeRepaintFrequency, 10f);
            menu.AddItem(new GUIContent("Refresh Frequency/Never", "Never refresh at runtime."),
                Mathf.Approximately(0f, QuestEditorPrefs.runtimeRepaintFrequency), SetRuntimeRepaintFrequency, 0f);
            if (m_quest != null && m_quest.isProcedurallyGenerated)
            {
                menu.AddItem(new GUIContent("Save As Asset...", "Save this procedurally-generated quest as an asset."), false, SaveGeneratedQuestAsAsset);
            }
        }

        protected struct SwitchQuestData
        {
            public QuestListContainer questListContainer;
            public int questIndex;
            public SwitchQuestData(QuestListContainer questListContainer, int questIndex)
            {
                this.questListContainer = questListContainer;
                this.questIndex = questIndex;
            }
        }

        private void AddQuestListGearMenuItems(GenericMenu menu)
        {
            if (QuestEditorWindow.selectedQuestListContainer == null) return;
            var questListContainer = QuestEditorWindow.selectedQuestListContainer;
            if (questListContainer.questList == null) return;
            menu.AddSeparator(string.Empty);
            for (int i = 0; i < questListContainer.questList.Count; i++)
            {
                var quest = questListContainer.questList[i];
                if (quest == null)
                {
                    menu.AddDisabledItem(new GUIContent("(unassigned)"));
                }
                else
                {
                    var s = StringField.GetStringValue(quest.title);
                    if (string.IsNullOrEmpty(s)) s = StringField.GetStringValue(quest.id);
                    if (string.IsNullOrEmpty(s)) s = quest.name;
                    menu.AddItem(new GUIContent(s), quest == m_quest, SwitchToQuest, new SwitchQuestData(questListContainer, i));
                }
            }
        }

        private void SwitchToQuest(object data)
        {
            var switchQuestData = (SwitchQuestData)data;
            QuestEditorWindow.instance.SelectQuest(switchQuestData.questListContainer, switchQuestData.questIndex);
        }

        protected void Pan(float x, float y)
        {
            m_panX = x;
            m_panY = y;
        }

        protected void Zoom(float zoom)
        {
            m_zoom = zoom;
        }

        protected void PanTopLeft(object data)
        {
            Pan(0, 0);
        }

        protected void Zoom(object data)
        {
            m_zoom = (float)data;
        }

        protected void ToggleZoomLock(object data)
        {
            QuestEditorPrefs.zoomLock = !QuestEditorPrefs.zoomLock;
        }

        protected void SetRuntimeRepaintFrequency(object data)
        {
            QuestEditorPrefs.runtimeRepaintFrequency = (float)data;
        }

        private void SaveGeneratedQuestAsAsset()
        {
            var filename = EditorUtility.SaveFilePanelInProject("Save Quest As", "New Quest", "asset", "Save quest as");
            if (string.IsNullOrEmpty(filename)) return;
            QuestEditorAssetUtility.SaveQuestAsAsset(m_quest, filename, true);
        }

        private void OpenTagsToTextTableWizard()
        {
            QuestTagsToTextTableWizard.Open();
        }

        private void OpenTextToTextTableWizard()
        {
            QuestTextToTextTableWizard.Open();
        }

        private void ExportToJson()
        {
            var newJsonFilename = EditorUtility.SaveFilePanel("Export to JSON", System.IO.Path.GetDirectoryName(jsonFilename), System.IO.Path.GetFileName(jsonFilename), "json");
            if (string.IsNullOrEmpty(newJsonFilename)) return;
            jsonFilename = newJsonFilename;
            var originalIncludeCanvasRect = QuestProxy.includeCanvasRect;
            QuestProxy.includeCanvasRect = true;
            var proxy = new QuestProxy(m_quest);
            QuestProxy.includeCanvasRect = originalIncludeCanvasRect;
            System.IO.File.WriteAllText(jsonFilename, JsonUtility.ToJson(proxy));
            Debug.Log(m_quest.title + " saved to " + jsonFilename);
        }

        private void ImportFromJson()
        {
            var newJsonFilename = EditorUtility.OpenFilePanel("Import from JSON", System.IO.Path.GetDirectoryName(jsonFilename), "json");
            if (string.IsNullOrEmpty(newJsonFilename)) return;
            var json = System.IO.File.ReadAllText(newJsonFilename);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError(newJsonFilename + " is empty.");
                return;
            }
            var proxy = JsonUtility.FromJson<QuestProxy>(json);
            if (proxy == null)
            {
                Debug.LogError(newJsonFilename + " is not a valid JSON format.");
                return;
            }
            if (!EditorUtility.DisplayDialog("Import " + System.IO.Path.GetFileName(newJsonFilename), "This will replace the quest in the editor with the contents of the JSON file. Proceed?", "Replace", "Cancel"))
            {
                return;
            }
            jsonFilename = newJsonFilename;
            var originalIncludeCanvasRect = QuestProxy.includeCanvasRect;
            QuestProxy.includeCanvasRect = true;
            proxy.CopyTo(m_quest);
            QuestProxy.includeCanvasRect = originalIncludeCanvasRect;
            AssignQuest(m_quest);
            Debug.Log("Imported " + jsonFilename);
        }

        #endregion

        #region Delete Subassets

        // When deleting a node, need to delete all its subassets or they will be left orphaned in the quest asset:
        private void DeleteNodeSubassets(int index)
        {
            // Serialized changes have been applied at this point, so we can work directly with m_quest:
            if (m_quest == null || !(0 <= index && index < m_quest.nodeList.Count)) return;
            var node = m_quest.nodeList[index];
            DeleteNodeConditionSubassets(node);
            DeleteNodeStateInfoListSubassets(node);
        }

        private void DeleteNodeConditionSubassets(QuestNode node)
        {
            if (node == null) return;
            DeleteSubassetsFromList(node.conditionSet.conditionList);
        }

        private void DeleteNodeStateInfoListSubassets(QuestNode node)
        {
            if (node == null) return;
            for (int i = 0; i < node.stateInfoList.Count; i++)
            {
                var stateInfo = node.stateInfoList[i];
                if (stateInfo == null) continue;
                DeleteSubassetsFromList(stateInfo.actionList);
                DeleteCategorizedContentListSubassets(stateInfo.categorizedContentList);
            }
        }

        private void DeleteCategorizedContentListSubassets(List<QuestContentSet> categorizedContentList)
        {
            if (categorizedContentList == null) return;
            for (int i = 0; i < categorizedContentList.Count; i++)
            {
                var contentSet = categorizedContentList[i];
                if (contentSet == null) continue;
                DeleteSubassetsFromList(contentSet.contentList);
            }
        }

        private void DeleteSubassetsFromList<T>(List<T> subassetList) where T : QuestSubasset
        {
            if (subassetList == null) return;
            for (int i = 0; i < subassetList.Count; i++)
            {
                var subasset = subassetList[i];
                if (subasset == null) continue;
                DeleteNestedSubassetsFromSubasset(subasset);
                AssetUtility.DeleteFromAsset(subasset, m_quest);
            }
            subassetList.Clear();
        }

        protected virtual void DeleteNestedSubassetsFromSubasset(ScriptableObject subasset)
        {
            // Special handling for subassets that can have child subassets:
            if (subasset is ButtonQuestContent)
            {
                DeleteSubassetsFromList((subasset as ButtonQuestContent).actionList);
            }
            if (subasset is AlertQuestAction)
            {
                DeleteSubassetsFromList((subasset as AlertQuestAction).contentList);
            }
        }

        // A bug in 1.0.0 - 1.0.2 orphaned subassets when deleting a node. 
        // This method scrubs them from the quest asset:
        private void DeleteUnusedSubassets()
        {
            QuestEditorAssetUtility.DeleteUnusedSubassets(m_quest);
        }

        #endregion

        #region Quest Relations in Quest Database

        [Serializable]
        private class QuestRelationRecord
        {
            public Quest quest;
            public Rect rect;
            public string label;
            public List<int> parentQuests;
            public List<int> childQuests;
        }
        private List<QuestRelationRecord> questRelationRecords = new List<QuestRelationRecord>();
        private QuestDatabase m_currentDatabase = null;
        private Rect m_lastWindowSize = new Rect(0, 0, 0, 0);

        public void DrawQuestRelations(QuestDatabase database)
        {
            if (database == null) return;
            if (database != m_currentDatabase || m_lastWindowSize.width != QuestEditorWindow.instance.position.width)
            {
                SetupQuestRelations(database);
            }
            EditorGUILayout.LabelField(database.name, QuestEditorStyles.questNameGUIStyle);
            for (int i = 0; i < questRelationRecords.Count; i++)
            {
                var record = questRelationRecords[i];
                var edit = GUI.Button(record.rect, record.label, QuestEditorStyles.questNodeWindowGUIStyle);
                if (edit)
                {
                    QuestEditorWindow.instance.SelectQuest(record.quest);
                    return;
                }
                for (int j = 0; j < record.childQuests.Count; j++)
                {
                    var otherRecord = questRelationRecords[record.childQuests[j]];
                    DrawNodeCurve(record.rect, otherRecord.rect, QuestEditorStyles.ChildRelationConnectorColor);
                }
                for (int j = 0; j < record.parentQuests.Count; j++)
                {
                    var otherRecord = questRelationRecords[record.childQuests[j]];
                    DrawNodeCurve(record.rect, otherRecord.rect, QuestEditorStyles.ParentRelationConnectorColor);
                }
            }
        }

        private void SetupQuestRelations(QuestDatabase database)
        {
            m_currentDatabase = database;
            m_lastWindowSize = QuestEditorWindow.instance.position;
            var windowWidth = m_lastWindowSize.width;
            int numColumns = Mathf.Max(1, (int)(windowWidth / (QuestNode.DefaultNodeWidth * 2)));

            // Set quest record positions:
            questRelationRecords.Clear();
            for (int i = 0; i < database.questAssets.Count; i++)
            {
                var record = new QuestRelationRecord();
                var quest = database.questAssets[i];
                if (quest == null) continue;
                string label = StringField.GetStringValue(quest.id);
                var pos = new Vector2(QuestNode.DefaultNodeWidth + (i % numColumns) * (QuestNode.DefaultNodeWidth * 2), (1 + (i / numColumns)) * (QuestNode.DefaultNodeHeight * 2));
                var rect = new Rect(pos.x, pos.y, QuestNode.DefaultNodeWidth, QuestNode.DefaultNodeHeight);
                record.quest = quest;
                record.rect = rect;
                record.label = label;
                record.childQuests = new List<int>();
                record.parentQuests = new List<int>();
                questRelationRecords.Add(record);
            }

            // Determine links:
            for (int i = 0; i < questRelationRecords.Count; i++)
            {
                var record = questRelationRecords[i];
                for (int j = 0; j < questRelationRecords.Count; j++)
                {
                    if (i == j) continue;
                    var otherQuest = questRelationRecords[j].quest;
                    if (IsQuestChild(record.quest, otherQuest))
                    {
                        record.childQuests.Add(j);
                    }
                    if (IsQuestParent(record.quest, otherQuest))
                    {
                        record.parentQuests.Add(j);
                    }
                }
            }
        }

        private bool IsQuestChild(Quest quest, Quest other)
        {
            if (DoesConditionSetReferenceMe(quest, other.autostartConditionSet))
            {
                return true;
            }
            if (DoesConditionSetReferenceMe(quest, other.offerConditionSet))
            {
                return true;
            }
            foreach (var node in other.nodeList)
            {
                if (DoesConditionSetReferenceMe(quest, node.conditionSet))
                {
                    return true;
                }
            }
            return false;
        }

        private bool DoesConditionSetReferenceMe(Quest quest, QuestConditionSet conditionSet)
        {
            foreach (var condition in conditionSet.conditionList)
            {
                if (condition is QuestStateQuestCondition)
                {
                    if (StringField.Equals((condition as QuestStateQuestCondition).requiredQuestID, quest.id))
                    {
                        return true;
                    }
                }
                else if (condition is QuestNodeStateQuestCondition)
                {
                    if (StringField.Equals((condition as QuestNodeStateQuestCondition).requiredQuestID, quest.id))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsQuestParent(Quest quest, Quest other)
        {
            foreach (var stateInfo in other.stateInfoList)
            {
                if (DoesActionListReferenceMe(quest, stateInfo.actionList))
                {
                    return true;
                }
            }
            foreach (var node in other.nodeList)
            {
                foreach (var stateInfo in node.stateInfoList)
                {
                    if (DoesActionListReferenceMe(quest, stateInfo.actionList))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool DoesActionListReferenceMe(Quest quest, List<QuestAction> actionList)
        {
            foreach (var action in actionList)
            {
                if (action is SetQuestStateQuestAction)
                {
                    if (StringField.Equals((action as SetQuestStateQuestAction).questID, quest.id))
                    {
                        return true;
                    }
                }
                else if (action is SetQuestNodeStateQuestAction)
                {
                    if (StringField.Equals((action as SetQuestNodeStateQuestAction).questID, quest.id))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
