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
        public string[] adminList;
        public Toggle uiToggle;
        private bool localPlayerIsAdmin;

        private void Start()
        {
            string localPlayerName = Networking.LocalPlayer.displayName.Trim().ToLower();
            localPlayerIsAdmin = false;
            foreach (string adminName in adminList)
            {
                if (adminName.Trim().ToLower() == localPlayerName)
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
