using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AdminOnlyManager : UdonSharpBehaviour
    {
        [HideInInspector] public Collider[] toggleColliders;
        [HideInInspector] public Renderer[] toggleRenderers;
        [HideInInspector] public Canvas[] toggleCanvases;
        [HideInInspector] public CanvasGroup[] toggleCanvasGroups;
        public bool adminListIsCaseInsensitive = true;
        [Tooltip("Leading and trailing whitespace is always ignored, both for names defined in this list as "
            + "well as for player display names.")]
        public string[] adminList;
        public Toggle uiToggle;
        private bool localPlayerIsAdmin;

        private void Start()
        {
            string localPlayerName = Networking.LocalPlayer.displayName.Trim();
            if (adminListIsCaseInsensitive)
                localPlayerName = localPlayerName.ToLower();
            localPlayerIsAdmin = false;
            foreach (string adminName in adminList)
            {
                string adminNameForCompare = adminName.Trim();
                if (adminListIsCaseInsensitive)
                    adminNameForCompare = adminNameForCompare.ToLower();
                if (adminNameForCompare == localPlayerName)
                {
                    localPlayerIsAdmin = true;
                    break;
                }
            }

            UpdateAdminOnlyComponents();
            uiToggle.isOn = localPlayerIsAdmin;
        }

        public void OnToggleValueChanged()
        {
            localPlayerIsAdmin = uiToggle.isOn;
            UpdateAdminOnlyComponents();
        }

        private void UpdateAdminOnlyComponents()
        {
            foreach (Collider collider in toggleColliders)
                if (collider != null)
                    collider.enabled = localPlayerIsAdmin;
            foreach (Renderer renderer in toggleRenderers)
                if (renderer != null)
                    renderer.enabled = localPlayerIsAdmin;
            foreach (Canvas canvas in toggleCanvases)
                if (canvas != null)
                    canvas.enabled = localPlayerIsAdmin;
            foreach (CanvasGroup canvasGroup in toggleCanvasGroups)
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = localPlayerIsAdmin ? 1f : 0f;
                    canvasGroup.interactable = localPlayerIsAdmin;
                    canvasGroup.blocksRaycasts = localPlayerIsAdmin;
                }
        }
    }
}
