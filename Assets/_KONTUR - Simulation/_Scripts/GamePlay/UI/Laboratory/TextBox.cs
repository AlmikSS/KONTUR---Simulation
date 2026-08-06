using System.Collections;
using _KONTUR___Simulation._Scripts.SceneManagement;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _KONTUR___Simulation._Scripts.GamePlay.UI.Laboratory
{
    public sealed class TextBox : MonoBehaviour
    {
        [Title("UI")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _background;
        
        [Title("Settings")]
        [SerializeField, Slider(0f, 0.1f)] private float _letterWaitTime;
        
        private DayConfig _currentDayConfig;
        private Coroutine _textingRoutine;

        private void Start()
        {
            Initialize(SceneService.Instance.State.DayConfig);
        }
        
        private void Initialize(DayConfig config)
        {
            _currentDayConfig = config;
            
            if (_textingRoutine != null)
                StopCoroutine(_textingRoutine);
            
            _textingRoutine = StartCoroutine(TextingRoutine());
        }

        private IEnumerator TextingRoutine()
        {
            foreach (var replic in _currentDayConfig.Replics)
            {
                _nameText.text = replic.Name;
                _text.text = "";
                
                if (replic.Image != null)
                    _background.sprite = replic.Image;

                foreach (var letter in replic.Text)
                {
                    _text.text += letter;

                    if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                    {
                        _text.SetText(replic.Text);
                        yield return null;
                        break;
                    }
                    
                    yield return new WaitForSeconds(_letterWaitTime);
                }

                yield return new WaitUntil(() => UnityEngine.Input.GetKeyDown(KeyCode.Space));
                yield return null;
            }
            
            yield return new WaitUntil(() => UnityEngine.Input.GetKeyDown(KeyCode.Space));
            SceneService.Instance.LoadScene("Level" + SceneService.Instance.State.CurrentLevel);
        }
    }
}