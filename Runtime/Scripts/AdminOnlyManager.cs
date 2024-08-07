using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AdminOnlyManager : UdonSharpBehaviour
    {
        [HideInInspector] public Collider[] toggleColliders;
        [HideInInspector] public Renderer[] toggleRenderers;
        [HideInInspector] public Canvas[] toggleCanvases;
        [HideInInspector] public CanvasGroup[] toggleCanvasGroups;
        [HideInInspector] [SerializeField] private UdonBehaviour self;
        [Tooltip("Optional. When provided, the referenced toggle must send the custom event "
            + "'OnIsAdminToggleValueChanged' to this script.")]
        public Toggle isAdminUIToggle;
        public bool adminListIsCaseInsensitive = true;
        [Tooltip("Leave empty to just use Admin List instead.\n"
            + "Create a github gist at: https://gist.github.com\n"
            + "Make sure to remove the second sha256 part of the github gist url, that way it always points "
            + "the latest version of the file. Example url:\n"
            + "https://gist.githubusercontent.com/JanSharp/b9d9fc71311472f4ba9b9c56c0cb6bcc/raw/admin-list.txt")]
        public VRCUrl adminListUrl;
        [Tooltip("When retrieving the admin list from the provided url fails an error message gets written "
            + "the log file which by default contains the url which was attempted to be accessed. When this "
            + "is enabled however then the error message will not contain the url.")]
        public bool hideUrlInErrorLogMessages;
        [Tooltip("Leading and trailing whitespace is always ignored, both for names defined in this list as "
            + "well as for player display names.\nUsed if no Admin List Url is provided, as well as if there "
            + "was an error retrieving the list from the provided url.")]
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
            if (!string.IsNullOrWhiteSpace(adminListUrl.Get())) // Cannot actually be null, just checking whitespace.
                VRCStringDownloader.LoadUrl(adminListUrl, self);
            else
                CheckIfLocalPlayerIsAdmin(); // Use the admin list from the inspector.
        }

        private void CheckIfLocalPlayerIsAdmin()
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

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            string list = result.Result;
            string[] loadedAdminList = list.Replace('\r', '\n').Split('\n');
            int count = 0;
            foreach (string adminName in loadedAdminList)
                if (!string.IsNullOrWhiteSpace(adminName)) // Cannot actually be null, just checking for whitespace.
                    count++;
            adminList = new string[count];
            int i = 0;
            foreach (string adminName in loadedAdminList)
            {
                string trimmed = adminName.Trim();
                if (trimmed == "")
                    continue;
                adminList[i++] = trimmed;
            }
            CheckIfLocalPlayerIsAdmin();
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.Log($"[AdminOnlyObjects] Failed to load admin list"
                + (hideUrlInErrorLogMessages ? $", " : $" from {result.Url}, ")
                + $"error code: {result.ErrorCode}, error message: {result.Error}");
            CheckIfLocalPlayerIsAdmin(); // Just use the default one provided from the inspector.
        }

        public void BecomeAdmin() => IsAdmin = true;

        public void BecomeNonAdmin() => IsAdmin = false;

        public void ToggleIsAdmin() => IsAdmin = !IsAdmin;

        public override void Interact() => ToggleIsAdmin();

        public void OnIsAdminToggleValueChanged()
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
