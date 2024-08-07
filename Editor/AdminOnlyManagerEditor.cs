using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor.Build;

namespace JanSharp
{
    ///cSpell:ignore IVRCSDK, VRCSDK

    public class VRCAdminOnlyManagerEditorOnBuild : IVRCSDKBuildRequestedCallback
    {
        int IOrderedCallback.callbackOrder => 0;

        bool IVRCSDKBuildRequestedCallback.OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType != VRCSDKRequestedBuildType.Scene)
                return true;
            return AdminOnlyManagerEditor.RunOnBuild();
        }
    }

    [InitializeOnLoad]
    [DefaultExecutionOrder(-1000)]
    public static class AdminOnlyManagerEditor
    {
        static AdminOnlyManagerEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange data)
        {
            if (data == PlayModeStateChange.ExitingEditMode)
                RunOnBuild();
        }

        public static void SetArrayProperty<T>(SerializedProperty property, ICollection<T> newValues, System.Action<SerializedProperty, T> setValue)
        {
            property.ClearArray();
            property.arraySize = newValues.Count;
            int i = 0;
            foreach (T value in newValues)
                setValue(property.GetArrayElementAtIndex(i++), value);
        }

        public static bool RunOnBuild()
        {
            AdminOnlyMarker[] toggles = Object.FindObjectsByType<AdminOnlyMarker>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            List<Canvas> canvases = toggles.SelectMany(t => t.GetComponents<Canvas>()).ToList();
            foreach (Canvas canvas in canvases)
            {
                // If there is no UI Shape then it is a non interactive UI, just displaying information.
                if (canvas.GetComponent<VRC_UiShape>() == null)
                    continue;
                if (canvas.GetComponent<BoxCollider>() != null)
                    continue;
                BoxCollider collider = Undo.AddComponent<BoxCollider>(canvas.gameObject);
                SerializedObject colliderProxy = new SerializedObject(collider);
                colliderProxy.FindProperty("m_Size").vector3Value = new Vector3(canvas.pixelRect.size.x, canvas.pixelRect.size.y, 1);
                colliderProxy.ApplyModifiedProperties();
            }

            AdminOnlyManager manager = Object.FindAnyObjectByType<AdminOnlyManager>(FindObjectsInactive.Include);
            SerializedObject managerProxy = new SerializedObject(manager);
            SetArrayProperty(managerProxy.FindProperty(nameof(manager.toggleColliders)),
                toggles.SelectMany(t => t.GetComponents<Collider>()).ToList(),
                (p, v) => p.objectReferenceValue = v);
            SetArrayProperty(managerProxy.FindProperty(nameof(manager.toggleRenderers)),
                toggles.SelectMany(t => t.GetComponents<Renderer>()).ToList(),
                (p, v) => p.objectReferenceValue = v);
            SetArrayProperty(managerProxy.FindProperty(nameof(manager.toggleCanvases)),
                canvases,
                (p, v) => p.objectReferenceValue = v);
            SetArrayProperty(managerProxy.FindProperty(nameof(manager.toggleCanvasGroups)),
                toggles.SelectMany(t => t.GetComponents<CanvasGroup>()).ToList(),
                (p, v) => p.objectReferenceValue = v);
            managerProxy.ApplyModifiedProperties();

            return true;
        }
    }
}
