
using UnityEngine;
using VRC.SDK3.Components;

namespace UdonSharp.Examples.Utilities
{
    /// <summary>
    /// A Basic example class that demonstrates how to toggle a list of object on and off when someone interacts with the UdonBehaviour
    /// This toggle only works locally
    /// </summary>
    [AddComponentMenu("Udon Sharp/Utilities/Interact Toggle")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class InteractToggle : UdonSharpBehaviour
    {
        [Tooltip("List of objects to toggle on and off")]
        public GameObject[] toggleObjects;
        public GameObject[] ensureOffObjects;
        public bool intialState = false;

        public void Start()
        {
            // Ensure all toggle objects are off at the start
            foreach (GameObject toggleObject in toggleObjects)
            {
                if (toggleObject != null)
                {
                    toggleObject.SetActive(intialState);
                }
            }

            // Ensure all ensureOffObjects are off at the start
            foreach (GameObject ensureOffObject in ensureOffObjects)
            {
                if (ensureOffObject != null)
                {
                    ensureOffObject.SetActive(false);
                }
            }
        }

        public override void Interact()
        {
            var newState = !toggleObjects[0].activeSelf;
            foreach (GameObject toggleObject in toggleObjects)
            {
                if (toggleObject != null)
                {
                    toggleObject.SetActive(newState);
                }
            }

            if (newState)
            {
                foreach (GameObject ensureOffObject in ensureOffObjects)
                {
                    if (ensureOffObject != null)
                    {
                        ensureOffObject.SetActive(false);
                    }
                }
            }
        }
    }
}
