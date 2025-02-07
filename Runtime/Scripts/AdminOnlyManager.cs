﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using VRC.SDK3.Data;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using TMPro;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
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
        [Tooltip("Optional. When provided, the initial text of this input field is ignored.\n"
            + "The textfield must be a Multi Line Newline text field.\n"
            + "The textfield's OnValueChanged (Not OnEndEdit, that's bugged) event must be send custom event "
            + "'OnAdminListFieldValueChanged' to this script.\n"
            + "Changes made to this textfield are synced.\n"
            + "The text field can be marked to be hidden for non admins. Just keep in mind that if someone "
            + "were to set it to an empty string, nobody would be able to access the field anymore.")]
        public TMP_InputField adminListField;
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
        private string[] defaultAdminList;
        private bool didReceiveData = false;
        private bool isInitialUrlRequest = false;
        [UdonSynced] private string syncedAdminList;
        private bool isAdmin = false;
        public bool IsAdmin
        {
            get => isAdmin;
            set
            {
                if (value == isAdmin)
                    return;
                isAdmin = value;
                if (isAdminUIToggle != null && isAdminUIToggle.isOn != isAdmin)
                    isAdminUIToggle.isOn = isAdmin;
                UpdateAdminOnlyComponents();
            }
        }
        private bool isOverridden = false;
        private bool IsOverridden
        {
            get => isOverridden;
            set
            {
                if (value == isOverridden)
                    return;
                isOverridden = value;
                if (!isOverridden)
                    CheckIfLocalPlayerIsAdmin();
            }
        }

        private float retryBackoff = 1f;

        private bool HasAdminListUrl => !string.IsNullOrWhiteSpace(adminListUrl.Get()); // Cannot actually be null, just checking whitespace.

        private void Start()
        {
            UpdateAdminOnlyComponents(); // Disable everything by default.
            if (isAdminUIToggle != null && isAdminUIToggle.isOn)
                isAdminUIToggle.isOn = false;

            CleanAdminList(); // Also updates the admin list text field if it is provided.
            defaultAdminList = adminList;

            if (HasAdminListUrl)
                SendCustomEventDelayedSeconds(nameof(LoadAdminListDelayed), 1f);
            else
                CheckIfLocalPlayerIsAdmin(); // Use the admin list from the inspector.
        }

        public void LoadAdminListDelayed()
        {
            if (didReceiveData)
                return;

            if (HasAdminListUrl)
            {
                isInitialUrlRequest = true;
                LoadAdminListFromUrl();
            }
            else
                CheckIfLocalPlayerIsAdmin(); // Use the admin list from the inspector.
        }

        public void LoadAdminListFromUrl()
        {
            VRCStringDownloader.LoadUrl(adminListUrl, self);
        }

        private void CheckIfLocalPlayerIsAdmin()
        {
            if (IsOverridden)
                return;

            string localPlayerName = Networking.LocalPlayer.displayName.Trim();
            if (adminListIsCaseInsensitive)
                localPlayerName = localPlayerName.ToLower();
            bool newIsAdmin = false;
            foreach (string adminName in adminList)
            {
                string adminNameForCompare = adminName.Trim();
                if (adminListIsCaseInsensitive)
                    adminNameForCompare = adminNameForCompare.ToLower();
                if (adminNameForCompare == localPlayerName)
                {
                    newIsAdmin = true;
                    break;
                }
            }
            IsAdmin = newIsAdmin;
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            if (isInitialUrlRequest && didReceiveData) // Received data takes precedence of the string loaded list.
            {
                isInitialUrlRequest = false;
                return;
            }
            LoadAdminListFromString(result.Result);

            // Make it so only one player has to load the list from the url... unless there's more than 1
            // second delay between Start and OnDeserialization then they send the request anyway.
            if (Networking.IsOwner(this.gameObject))
                RequestSerialization();
            else if (!isInitialUrlRequest)
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                RequestSerialization();
            }
            isInitialUrlRequest = false;
        }

        private void CleanAdminList()
        {
            int count = 0;
            foreach (string adminName in adminList)
                if (!string.IsNullOrWhiteSpace(adminName)) // Cannot actually be null, just checking for whitespace.
                    count++;
            string[] cleanAdminList = new string[count];
            int i = 0;
            foreach (string adminName in adminList)
            {
                string trimmed = adminName.Trim();
                if (trimmed == "")
                    continue;
                cleanAdminList[i++] = trimmed;
            }
            SetAdminList(cleanAdminList);
        }

        private void SetAdminList(string[] newAdminList)
        {
            adminList = newAdminList;
            syncedAdminList = string.Join('\n', adminList);
            if (adminListField != null)
                adminListField.text = syncedAdminList;
        }

        private void LoadAdminListFromString(string adminListString)
        {
            adminList = adminListString.Replace('\r', '\n').Split('\n');
            CleanAdminList();
            CheckIfLocalPlayerIsAdmin();
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.Log($"[AdminOnlyManager] Failed to load admin list"
                + (hideUrlInErrorLogMessages ? $", " : $" from {result.Url}, ")
                + $"error code: {result.ErrorCode}, error message: {result.Error}");
            isInitialUrlRequest = false;
            CheckIfLocalPlayerIsAdmin(); // Just use the default one provided from the inspector.
        }

        public void BecomeAdmin()
        {
            IsOverridden = true;
            IsAdmin = true;
        }

        public void BecomeNonAdmin()
        {
            IsOverridden = true;
            IsAdmin = false;
        }

        public void ToggleIsAdmin()
        {
            IsOverridden = true;
            IsAdmin = !IsAdmin;
        }

        public override void Interact() => ToggleIsAdmin();

        public void RemoveAllOverrides()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(RemoveLocalOverride));
        }

        public void RemoveLocalOverride()
        {
            IsOverridden = false;
        }

        public void OnIsAdminToggleValueChanged()
        {
            if (isAdminUIToggle == null || isAdminUIToggle.isOn == IsAdmin)
                return;
            IsOverridden = true;
            IsAdmin = isAdminUIToggle.isOn;
        }

        public void OnAdminListFieldValueChanged()
        {
            if (adminListField == null)
                return;
            string fieldText = adminListField.text;
            if (fieldText == syncedAdminList) // To prevent recursion.
                return;
            syncedAdminList = fieldText;
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
            OnDeserialization();
        }

        public void ResetAdminListToDefault()
        {
            SetAdminList(defaultAdminList);
            CheckIfLocalPlayerIsAdmin();
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }

        private void UpdateAdminOnlyComponents()
        {
            foreach (Collider collider in toggleColliders)
                if (collider != null)
                    collider.enabled = isAdmin;
            foreach (Renderer renderer in toggleRenderers)
                if (renderer != null)
                    renderer.enabled = isAdmin;
            foreach (Canvas canvas in toggleCanvases)
                if (canvas != null)
                    canvas.enabled = isAdmin;
            foreach (CanvasGroup canvasGroup in toggleCanvasGroups)
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = isAdmin ? 1f : 0f;
                    canvasGroup.interactable = isAdmin;
                    canvasGroup.blocksRaycasts = isAdmin;
                }
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (result.success)
            {
                retryBackoff = 1f;
                return;
            }
            // If the owner changes while this is happening, I don't really care nor do I think it matters.
            SendCustomEventDelayedSeconds(nameof(RequestSerializationDelayed), retryBackoff);
            retryBackoff = Mathf.Min(16f, retryBackoff * 2f);
        }

        public void RequestSerializationDelayed() => RequestSerialization();

        public override void OnDeserialization()
        {
            didReceiveData = true;
            LoadAdminListFromString(syncedAdminList);
        }
    }
}
