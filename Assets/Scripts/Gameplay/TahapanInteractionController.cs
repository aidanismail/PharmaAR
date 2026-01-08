using System;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class TahapanInteractionController : MonoBehaviour
    {
        [SerializeField] private TahapanInteractionData[] interactionData;
        
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private TextMeshProUGUI titleText; // ToDo :Move this to UI Manager
        [SerializeField] private TextMeshProUGUI descriptionText; // ToDo :Move this to UI Manager
        
        private int currentInteractionIndex;
        private bool isPlaying;
        private TahapanInteractionPlayer interactionPlayer;
        
        public event Action OnInteractionComplete;
        
        void Awake()
        {
            interactionPlayer = new TahapanInteractionPlayer();
            interactionPlayer.Initialize(titleText, descriptionText, this, animator, audioSource);
        }
        
        public void StartInteraction()
        {
            currentInteractionIndex = 0;
            EnterInteractionState(currentInteractionIndex);
        }
        
        private void EnterInteractionState(int targetIndex)
        {
            if (!CanEnterState(targetIndex))
            {
                return;
            }
            currentInteractionIndex = targetIndex;
            isPlaying = true;
            MainInteractionState();
        }
        
        private void MainInteractionState()
        {
            interactionPlayer.Play(interactionData[currentInteractionIndex], () =>
            {
                ExitInteractionState();
            });
        }
        
        private void ExitInteractionState()
        {
            isPlaying = false;
            currentInteractionIndex++;
            if (currentInteractionIndex >= interactionData.Length)
            {
                OnInteractionComplete?.Invoke();
            }
        }
        
        private bool CanEnterState(int targetIndex)
        {
            if (targetIndex >= interactionData.Length)
            {
                return false;
            }
            if (targetIndex < 0)
            {
                return false;
            }
            if (interactionData[targetIndex] == null)
            {
                return false;
            }
            return true;
        }
    }
}