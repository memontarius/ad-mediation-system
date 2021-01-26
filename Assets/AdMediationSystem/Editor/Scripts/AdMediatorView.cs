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
        private SerializedProperty _tierListProp;
        private Dictionary<string, ReorderableList> _unitListDict = new Dictionary<string, ReorderableList>();
        private ReorderableList _tierReorderableList;
        private AnimBool _showMediator;
        private int _index;
        
        private string[] FetchStratageTypes
        {
            get; set;
        }

        public AdMediatorView(int mediatorId, SerializedObject serializedSettings, SerializedProperty tierListProp, UnityAction repaint)
        {
            _index = mediatorId;
            _showMediator = new AnimBool(true);
            _showMediator.valueChanged.AddListener(repaint);
            _tierListProp = tierListProp;
            _tierReorderableList = new ReorderableList(serializedSettings, _tierListProp);
            _tierReorderableList.headerHeight = 1;
            FetchStratageTypes = new string[] { "random", "sequence" };

            _tierReorderableList.onAddCallback = (ReorderableList list) =>
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);
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

        public void SetTierListProperty(int mediatorId, SerializedProperty tierListProp)
        {
            _index = mediatorId;
            _tierListProp = tierListProp;
            _tierReorderableList.serializedProperty = _tierListProp;
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
                EditorGUILayout.LabelField("Name", GUILayout.Width(90));
                EditorGUILayout.TextField("", GUILayout.Width(180));

                GUILayout.FlexibleSpace();
                isDelitionPerform = GUILayout.Button("Delete");
 
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Fetch strategy", GUILayout.Width(90));
                EditorGUILayout.Popup(0, FetchStratageTypes, GUILayout.Width(180));

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
