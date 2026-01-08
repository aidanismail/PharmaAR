using TMPro;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Config;

namespace Gameplay
{
    public class TahapanInteractionPlayer
    {
        private Animator animator;
        private AudioSource audioSource;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI descriptionText;
        private AnimationConfig animationConfig;
        private MonoBehaviour coroutineRunner;

        private bool isFinished;
        
        public void Initialize(TextMeshProUGUI titleText, TextMeshProUGUI descriptionText, MonoBehaviour coroutineRunner, Animator animator = null, AudioSource audioSource = null, AnimationConfig animationConfig = null)
        {
            this.coroutineRunner = coroutineRunner;
            this.animator = animator;
            this.audioSource = audioSource;
            this.titleText = titleText;
            this.descriptionText = descriptionText;
            this.animationConfig = animationConfig;
        }

        public void Play(TahapanInteractionData data, Action onFinishedCallback = null)
        {
            if (data == null)
            {
                return;
            }

            isFinished = false;
            titleText.SetText(data.Title);
            descriptionText.SetText(data.Description);

            bool isAnimationFinished = data.AnimationClip == null;
            bool isAudioClipFinished = data.AudioNaration == null;
            
            if (isAnimationFinished && isAudioClipFinished)
            {
                onFinishedCallback?.Invoke();
                return;
            }
            if (data.AnimationClip != null)
            {
                animator.SetTrigger(animationConfig.PlayAnimationParamName); 
                coroutineRunner.StartCoroutine(WaitForDuration(data.AnimationClip.length, () => // This is only valid if animation speed is constant
                    { 
                        isAnimationFinished = true;
                        TryFinish(isAnimationFinished, isAudioClipFinished, () =>
                        {
                            onFinishedCallback?.Invoke();
                        }); 
                    }
                ));
            }
            if (data.AudioNaration != null)
            {
                audioSource.PlayOneShot(data.AudioNaration);
                coroutineRunner.StartCoroutine(WaitForDuration(data.AudioNaration.length, () => 
                    { 
                        isAudioClipFinished = true;
                        TryFinish(isAnimationFinished, isAudioClipFinished, () =>
                        {
                            onFinishedCallback?.Invoke();
                        }); 
                    }
                ));
            }
        }

        public void Stop()
        {
            if (animator != null)
            {
                if (animationConfig != null)
                {
                    animator.SetTrigger(animationConfig.StopAnimationParamName);
                }
            }
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private void TryFinish(bool isAnimationFinished, bool isAudioClipFinished, Action onFinishedCallback)
        {
            if (isFinished)
            {
                return;
            }
            if (!isAnimationFinished || !isAudioClipFinished) return;

            isFinished = true;
            onFinishedCallback?.Invoke();
        }

        private IEnumerator WaitForDuration(float duration, Action onFinishedCallback)
        {
            yield return new WaitForSeconds(duration);
            onFinishedCallback?.Invoke();
        }
    }
}