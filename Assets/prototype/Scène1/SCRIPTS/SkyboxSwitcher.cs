using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SimpleSkyboxFader : MonoBehaviour
{
    [Header("Skyboxes")]
    public Material skyboxA;
    public Material skyboxB;

    [Header("Réglages")]
    public float delayBeforeStart = 2.0f;
    public float fadeDuration = 1.0f; // Vitesse du fondu au noir

    private Image fadeImage;
    private Canvas fadeCanvas;

    void Start()
    {
        // On initialise la scène avec la première skybox
        if (skyboxA != null) RenderSettings.skybox = skyboxA;
        
        // On crée l'effet de fondu automatiquement au lancement
        CreateFadeOverlay();
        StartCoroutine(ExecuteTransition());
    }

    IEnumerator ExecuteTransition()
    {
        // 1. Attente initiale
        yield return new WaitForSeconds(delayBeforeStart);

        // 2. Fondu au NOIR (Apparition du voile)
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            SetAlpha(timer / fadeDuration);
            yield return null;
        }

        // 3. Changement de Skybox (pendant que l'écran est noir)
        if (skyboxB != null)
        {
            RenderSettings.skybox = skyboxB;
            DynamicGI.UpdateEnvironment(); // Met à jour la lumière de la scène
        }

        yield return new WaitForSeconds(0.2f); // Petite pause pour la stabilité

        // 4. Fondu vers la SCÈNE (Disparition du voile)
        timer = fadeDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            SetAlpha(timer / fadeDuration);
            yield return null;
        }
    }

    // Crée un Canvas et une Image noire dynamiquement pour ne pas avoir à le faire à la main
    void CreateFadeOverlay()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; // Toujours au dessus du reste
        canvasObj.AddComponent<CanvasScaler>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Commence transparent
        
        // Étirer l'image sur tout l'écran
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.one;
    }

    void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = Mathf.Clamp01(alpha);
            fadeImage.color = c;
        }
    }
}