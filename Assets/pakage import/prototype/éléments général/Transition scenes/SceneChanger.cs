using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class VRSceneTransitioner : MonoBehaviour
{
    [Header("Configuration")]
    public string sceneToLoad;
    public float delayBeforeStart = 2f;
    public float totalFadeDuration = 5f;

    private RawImage fadeImage;
    private Canvas fadeCanvas;
    private GameObject canvasObj;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(TransitionSequence());
    }

    private void CreateVRFadeOverlay()
    {
        // On cherche la caméra principale (le casque VR)
        Camera vrCamera = Camera.main;

        canvasObj = new GameObject("VRFadeCanvas");
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        
        // IMPORTANT POUR LA VR : On utilise la caméra pour le rendu
        fadeCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        fadeCanvas.worldCamera = vrCamera;
        fadeCanvas.planeDistance = 0.11f; // Placé juste devant les lentilles
        fadeCanvas.sortingOrder = 999;

        canvasObj.AddComponent<CanvasScaler>();
        DontDestroyOnLoad(canvasObj);

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        fadeImage = imageObj.AddComponent<RawImage>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        // On s'assure que l'image couvre tout le champ de vision
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = new Vector2(100, 100); // Un peu de marge pour la VR
    }

    private IEnumerator TransitionSequence()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        CreateVRFadeOverlay();

        float halfDuration = totalFadeDuration / 2f;
        float timer = 0f;

        // 1. Fondu vers le NOIR
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(timer / halfDuration));
            yield return null;
        }
        fadeImage.color = Color.black;

        // 2. Chargement de la scène
        // Note: En VR, le chargement peut faire "freezer" l'image. 
        // C'est normal si tu n'utilises pas de "Loading Scene" dédiée.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. RE-CONNEXION À LA NOUVELLE CAMÉRA
        // Après le chargement, l'ancienne caméra est détruite. Il faut donner la nouvelle au Canvas.
        fadeCanvas.worldCamera = Camera.main;

        // Mise à jour de l'éclairage (GI) comme demandé
        DynamicGI.UpdateEnvironment();
        
        yield return new WaitForSeconds(0.5f); // Petit temps mort pour stabiliser les FPS en VR

        // 4. Fondu vers le CLAIR
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(1f - (timer / halfDuration)));
            yield return null;
        }

        Destroy(canvasObj);
        Destroy(this.gameObject);
    }
}