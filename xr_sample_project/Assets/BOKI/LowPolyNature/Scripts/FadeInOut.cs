using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BOKI.LowPolyNature.Scripts
{
    public class FadeInOut : MonoBehaviour
    {
        private const float ShowValue = 1.0f;
        private const float HideValue = 0.0f;

        [SerializeField] private float delay;
        [SerializeField] private float fadeDuration = 2.0f;
        [SerializeField] private bool startFadedOut = true;
        [SerializeField] private List<float> fadeInAtSeconds;
        [SerializeField] private List<float> fadeOutAtSeconds;
        
        private Image _backgroundImage;
        private float _elapsedTime;
        public void Start()
        {
            _backgroundImage = GetComponent<Image>();
            InitDefaults();
        }
        
        private void InitDefaults()
        {
            if (startFadedOut)
                _backgroundImage.canvasRenderer.SetAlpha(ShowValue);
            
            fadeInAtSeconds.Sort();
            fadeOutAtSeconds.Sort();

            _elapsedTime -= delay;
        }

        public void Update()
        {
            _elapsedTime += Time.deltaTime;
            CheckFadeIn();
            CheckFadeOut();
        }

        private void CheckFadeIn()
        {
            if (fadeInAtSeconds.Count == 0) return;
            if (_elapsedTime < fadeInAtSeconds[0]) return;
            
            fadeInAtSeconds.RemoveAt(0);
            FadeIn();
        }
        
        private void CheckFadeOut()
        {
            if (fadeOutAtSeconds.Count == 0) return;
            if (_elapsedTime < fadeOutAtSeconds[0]) return;
            
            fadeOutAtSeconds.RemoveAt(0);
            FadeOut();
        }

        private void FadeIn()
        {
            _backgroundImage.CrossFadeAlpha(HideValue, fadeDuration, false);
        }
        
        private void FadeOut()
        {
            _backgroundImage.CrossFadeAlpha(ShowValue, fadeDuration, false);
        }
    }
}