using UnityEngine;

namespace BOKI.LowPolyNature.Scripts
{
    public class WaterfallAudio : MonoBehaviour
    {
        [SerializeField] private float delay;
        
        private AudioSource _audioSource;

        public void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            PlayAudio();
        }

        private void PlayAudio()
        {
            if (_audioSource != null)
                _audioSource.PlayDelayed(delay);
        }
    }
}
