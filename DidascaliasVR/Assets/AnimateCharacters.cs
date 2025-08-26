using System.Collections;
using TMPro;
using UnityEngine;

public class AnimateCharacters : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _text;

    [SerializeField]
    private bool _additive = false;

    [SerializeField]
    private string _baseText = "Cargando sesión";

    [SerializeField]
    private string _addedText = "...";

    [SerializeField]
    private int _characterEachTick = 1;

    [SerializeField]
    private float _tickTime = 0.5f;

    [SerializeField]
    private bool _animateOnEnable = true;

    private bool _isPlaying = true;

    private const string HTML_ALPHA = "<color=#00000000>";

    public void StartAnimation()
    {
        _isPlaying = true;
        StartCoroutine(AnimateCharactersCoroutine());
    }

    public void StopAnimation()
    {
        _isPlaying = false;
    }

    IEnumerator AnimateCharactersCoroutine()
    {
        int len = _addedText.Length;
        int i = 0;
            
        while(_isPlaying)
        {
            if (!_additive)
            {
                _text.text = _baseText + _addedText;
                _text.text = _text.text.Insert(_baseText.Length + i, HTML_ALPHA);
            }
            else
            {
                _text.text = _baseText + _addedText.Substring(0, i);
            }

            if (i + _characterEachTick > len && i < len)
            {
                i = len;
            }
            else
            {
                i = (i + _characterEachTick) % (len+1);
            }
            yield return new WaitForSeconds(_tickTime);
        }
    }

    private void OnEnable()
    {
        if (_animateOnEnable) StartAnimation();
    }

}
