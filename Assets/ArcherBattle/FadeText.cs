using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;


namespace Assets.ArcherBattle
{
    public class FadeText : MonoBehaviour
    {

        void OnEnable()
        {
            TextMeshPro tmp = GetComponent<TextMeshPro>();
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1);
            StartCoroutine(HideTurnText(tmp));
        }

        IEnumerator HideTurnText(TextMeshPro text)
        {
            float duration = 1f;
            float elapsed = 0f;

            Color originalColor = text.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            text.gameObject.SetActive(false);
        }
    }
}

