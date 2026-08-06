using System.Collections;
using UnityEngine;
using TMPro;

public class CreditsScroller : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float startDelay = 1f;

    private RectTransform textRect;
    private float textHeight;
    private float startY;

    private void Start()
    {
        textRect = creditsText.GetComponent<RectTransform>();
        
        creditsText.text = 
            "<size=60><b>Mindcell</b></size>\n\n" +
            "<size=36>Candy Candle Games</size>\n\n\n" +
            "<size=32>Вова (Kannon)</size>\n" +
            "UI/UX Artist, Frontend Programmer, Level Designer, Sound Designer\n\n\n" +
            "<size=32>Ис (Kofeyek)</size>\n" +
            "Lead, Programmer, Architecture Author\n\n\n" +
            "<size=32>Женя (Eugene_Volsky)</size>\n" +
            "3D Artist, Game Designer, Idea Author\n\n\n" +
            "<size=32>CCGAI</size>\n" +
            "Consultant";

        creditsText.alignment = TextAlignmentOptions.Center;
        
        // Принудительно обновляем layout чтобы получить правильную высоту
        Canvas.ForceUpdateCanvases();
        textHeight = textRect.rect.height;
        
        // Стартовая позиция - текст внизу за экраном
        startY = -Screen.height * 0.5f - textHeight * 0.5f;
        textRect.anchoredPosition = new Vector2(0, startY);
        
        StartCoroutine(ScrollCredits());
    }

    private IEnumerator ScrollCredits()
    {
        yield return new WaitForSeconds(startDelay);
        
        // Конечная позиция - текст уходит вверх за экран
        float endY = Screen.height * 0.5f + textHeight * 0.5f;
        
        while (textRect.anchoredPosition.y < endY)
        {
            textRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }
        
        gameObject.SetActive(false);
    }
}