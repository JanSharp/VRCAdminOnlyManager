using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedGameObjectsToggle : UdonSharpBehaviour
    {
        [Tooltip("Upon interaction, all theses GameObjects get toggled - they flip their active state. "
            + "This means that these GameObjects can be a mixture of active and inactive ones, they all "
            + "simply get their active state inverted.")]
        public GameObject[] gameObjects;
        // Having 2 states here enables comparing previous with new states when syncing happens.
        // This could also be done with a field changed event, however the syntax of that in C# + UdonSharp is
        // on the more complex side, so this should be easier to grasp.
        [UdonSynced]
        private bool syncedState = false;
        private bool currentState = false;

        public override void Interact() // Only runs on the client which interacted with the object.
        {
            syncedState = !syncedState; // Invert the syncedState.
            // LocalPlayer must be the owner in order to be able to sync/send the new state to everyone else.
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            // Tell VRChat that this script would like to have its synced variables synced/sent - which is
            // just the isOpen boolean in this script.
            RequestSerialization();
            // Also run OnDeserialization locally since that is what actually updates the TextMeshPro text.
            // This is effectively how VRChat's networking is designed to work, to ensure that every client
            // eventually has the same state. Luckily we can actually do it this way in this script, there's
            // many occasions where we can't just call OnDeserialization after calling RequestSerialization.
            OnDeserialization();
        }

        /// <summary>
        /// <para>Gets run by VRChat when the local client has received data sent by another client. This also
        /// gets run when a client joins the instance late. By the end every client's
        /// <see cref="syncedState"/> state is identical. Now there could be multiple people spamming the same
        /// toggle at the same time, in which case their <see cref="currentState"/> gets toggled much more
        /// frequently than it actually gets synced, and once both stop they should both end on the same
        /// state. Their <see cref="syncedState"/> is guaranteed to be the same eventually, but since the
        /// toggling of GameObjects is implemented by inverting their active state we must make sure that
        /// everybody inverted this state the same amount of times... Or rather an even or odd amount of
        /// times. That's where <see cref="currentState"/> comes in, it ensures that everybody has inverted
        /// the GameObject's state an even amount of time, or everybody did so an odd amount of times.</para>
        /// </summary>
        public override void OnDeserialization()
        {
            if (syncedState == currentState)
                return; // If our current state already matches the synced state, do nothing.
            currentState = syncedState;

            foreach (GameObject go in gameObjects)
            {
                // Allow and ignore null game objects. This both allows objects to be destroyed at runtime,
                // but also prevents accidents, such as deleting a GameObject in the scene and forgetting to
                // update a toggle that was toggling it.
                if (go != null)
                    go.SetActive(!go.activeSelf); // Invert each GameObject's active state.
            }
        }
    }
}
