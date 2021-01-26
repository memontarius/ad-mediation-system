using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace Virterix.AdMediation.Editor
{
    public class AdMediatorView
    {
        private AdMediationSettingsWindow _settingsWindow;
        private SerializedProperty _mediatorProp;
        private SerializedProperty _tierListProp;
        private Dictionary<string, ReorderableList> _unitListDict = new Dictionary<string, ReorderableList>();
        private ReorderableList _tierReorderableList;
        private AnimBool _showMediator;
        private int _index;

        private SerializedProperty _mediatorNameProp;
        private SerializedProperty _mediatorFetchStrategyProp;

        private string[] FetchStratageTypes
        {
            get; set;
        }

        public AdMediatorView(AdMediationSettingsWindow settingsWindow, int mediatorId, SerializedObject serializedSettings, 
            SerializedProperty mediatorProp, UnityAction repaint)
        {
            _settingsWindow = settingsWindow;
            SetProperty(mediatorId, mediatorProp);
            _showMediator = new AnimBool(true);
            _showMediator.valueChanged.AddListener(repaint);
            _tierReorderableList = new ReorderableList(serializedSettings, _tierListProp);
            _tierReorderableList.headerHeight = 1;
            FetchStratageTypes = Enum.GetNames(typeof(FetchStrategyType));

            _tierReorderableList.onAddCallback = (ReorderableList list) =>
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);
                var addedElement = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);
                var units = addedElement.FindPropertyRelative("_units");
                units.arraySize = 0;
            };
            _tierReorderableList.drawHeaderCallback = rect =>
            {
            };
            _tierReorderableList.drawElementCallback = (elementRect, elementIndex, elementActive, elementFocused) =>
            {
                var element = _tierListProp.GetArrayElementAtIndex(elementIndex);
                var unitsProp = element.FindPropertyRelative("_units");
                string listKey = element.propertyPath;

                ReorderableList unitReorderableList;
                if (_unitListDict.ContainsKey(listKey))
                {
                    unitReorderableList = _unitListDict[listKey];
                }
                else
                {
                    unitReorderableList = new ReorderableList(element.serializedObject, unitsProp);
                    _unitListDict[listKey] = unitReorderableList;

                    unitReorderableList.drawElementCallback = (unitRect, unitIndex, unitActive, unitFocused) =>
                    {
                        SerializedProperty unitElement = unitReorderableList.serializedProperty.GetArrayElementAtIndex(unitIndex);
                        float elementWidth = unitRect.width;
                        float width = Mathf.Clamp(elementWidth - 215, 180, 2800);

                        unitRect.y += 1.5f;
                        //EditorGUI.LabelField(new Rect(unitRect.x, unitRect.y, 100, EditorGUIUtility.singleLineHeight), "Name");
                        /*
                        EditorGUI.PropertyField(
                            new Rect(unitRect.x + 80, unitRect.y, width, EditorGUIUtility.singleLineHeight),
                            unitElement.FindPropertyRelative("_networkName"),
                            GUIContent.none
                        );*/

                        var networkProp = unitElement.FindPropertyRelative("_networkName");

                        unitRect.width = 180f;
                        EditorGUI.Popup(unitRect, 0, _settingsWindow.ActiveNetworks);

                    };
                    unitReorderableList.drawHeaderCallback = (rect) =>
                    {
                        EditorGUI.LabelField(rect, "Tier " + (elementIndex + 1).ToString());
                    };
                }

                // Setup the inner list
                var height = (unitsProp.arraySize + 3) * EditorGUIUtility.singleLineHeight;
                unitReorderableList.DoList(new Rect(elementRect.x, elementRect.y, elementRect.width, height));
            };
            _tierReorderableList.elementHeightCallback = (int index) =>
            {
                var element = _tierListProp.GetArrayElementAtIndex(index);
                var unitsProp = element.FindPropertyRelative("_units");
                return Mathf.Clamp((unitsProp.arraySize - 1) * 21 + 75, 75, Mathf.Infinity);
            };
        }

        public void SetProperty(int mediatorId, SerializedProperty mediatorProp)
        {
            _index = mediatorId;
            _mediatorProp = mediatorProp;
            _tierListProp = _mediatorProp.FindPropertyRelative("_tiers");
            _mediatorNameProp = _mediatorProp.FindPropertyRelative("_name");
            _mediatorFetchStrategyProp = _mediatorProp.FindPropertyRelative("_fetchStrategyType");

            if (_tierReorderableList != null)
            {
                _tierReorderableList.serializedProperty = _tierListProp;
            }
        }

        private bool Collapsed { get; set; }

        public bool DrawView()
        {
            EditorGUILayout.BeginVertical("helpbox");
            Collapsed = EditorGUILayout.BeginFoldoutHeaderGroup(Collapsed, "Mediator " + (_index + 1).ToString());
            _showMediator.target = Collapsed;
            bool isDelitionPerform = false;

            if (EditorGUILayout.BeginFadeGroup(_showMediator.faded))
            {
                EditorGUILayout.Space(4);
        
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name", GUILayout.Width(150));
                _mediatorNameProp.stringValue = EditorGUILayout.TextField(_mediatorNameProp.stringValue, GUILayout.Width(180));

                GUILayout.FlexibleSpace();
                isDelitionPerform = GUILayout.Button("Delete");
 
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Fetch strategy", GUILayout.Width(150));
                _mediatorFetchStrategyProp.intValue = EditorGUILayout.Popup(_mediatorFetchStrategyProp.intValue, FetchStratageTypes, GUILayout.Width(180));
                
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(4);
                _tierReorderableList.DoLayoutList();
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
            return isDelitionPerform;
        }

    }
} // Virterix.AdMediation.Editor
