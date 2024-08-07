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
        public Toggle isAdminUIToggle;
        public bool adminListIsCaseInsensitive = true;
        [Tooltip("Leading and trailing whitespace is always ignored, both for names defined in this list as "
            + "well as for player display names.")]
        public string[] adminList;
        private bool isAdminInternal;
        public bool IsAdmin
        {
            get => isAdminInternal;
            set
            {
                if (value == isAdminInternal)
                    return;
                isAdminInternal = value;
                if (isAdminUIToggle != null && isAdminUIToggle.isOn != isAdminInternal)
                    isAdminUIToggle.isOn = isAdminInternal;
                UpdateAdminOnlyComponents();
            }
        }

        private void Start()
        {
            string localPlayerName = Networking.LocalPlayer.displayName.Trim();
            if (adminListIsCaseInsensitive)
                localPlayerName = localPlayerName.ToLower();
            isAdminInternal = false;
            foreach (string adminName in adminList)
            {
                string adminNameForCompare = adminName.Trim();
                if (adminListIsCaseInsensitive)
                    adminNameForCompare = adminNameForCompare.ToLower();
                if (adminNameForCompare == localPlayerName)
                {
                    isAdminInternal = true;
                    break;
                }
            }

            UpdateAdminOnlyComponents();
            if (isAdminUIToggle != null)
                isAdminUIToggle.isOn = isAdminInternal;
        }

        public void BecomeAdmin() => IsAdmin = true;

        public void BecomeNonAdmin() => IsAdmin = false;

        public void ToggleIsAdmin() => IsAdmin = !IsAdmin;

        public override void Interact() => ToggleIsAdmin();

        public void OnToggleValueChanged()
        {
            if (isAdminUIToggle == null)
                return;
            IsAdmin = isAdminUIToggle.isOn;
        }

        private void UpdateAdminOnlyComponents()
        {
            foreach (Collider collider in toggleColliders)
                if (collider != null)
                    collider.enabled = isAdminInternal;
            foreach (Renderer renderer in toggleRenderers)
                if (renderer != null)
                    renderer.enabled = isAdminInternal;
            foreach (Canvas canvas in toggleCanvases)
                if (canvas != null)
                    canvas.enabled = isAdminInternal;
            foreach (CanvasGroup canvasGroup in toggleCanvasGroups)
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = isAdminInternal ? 1f : 0f;
                    canvasGroup.interactable = isAdminInternal;
                    canvasGroup.blocksRaycasts = isAdminInternal;
                }
        }
    }
}
