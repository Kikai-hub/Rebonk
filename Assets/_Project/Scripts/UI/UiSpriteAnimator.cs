using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>Loops sprite frames on a UI Image in real time (keeps running while scenes load or the game is paused).</summary>
    [RequireComponent(typeof(Image))]
    public class UiSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 5f;

        private Image _image;
        private float _time;

        private void Awake() => _image = GetComponent<Image>();

        private void Update()
        {
            if (frames == null || frames.Length == 0)
                return;

            _time += Time.unscaledDeltaTime;
            var index = Mathf.FloorToInt(_time * framesPerSecond) % frames.Length;
            _image.sprite = frames[index];
        }
    }
}
