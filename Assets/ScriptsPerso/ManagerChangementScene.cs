using Oculus.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ManagerChangementScene : MonoBehaviour
{
    [Header("Configuration Interaction")]
    [SerializeField] private SnapInteractable _snapInteractable;

    [Header("Configuration Fade")]
    [SerializeField] private CanvasGroup _fadeCanvasGroup; 
    [SerializeField] private float _delayBeforeFade = 0.5f; // Réduit pour plus de réactivité au retrait
    [SerializeField] private float _fadeDuration = 1.0f;

    private void OnEnable()
    {
        if (_snapInteractable != null)
        {
            // Événement 1 : On pose la boule (Aller vers la scène de la boule)
            _snapInteractable.WhenSelectingInteractorViewAdded += TriggerSceneChange;
            
            // Événement 2 : On retire la boule (Retour au Hub)
            _snapInteractable.WhenSelectingInteractorViewRemoved += TriggerReturnToHub;
        }
    }

    private void OnDisable()
    {
        if (_snapInteractable != null)
        {
            _snapInteractable.WhenSelectingInteractorViewAdded -= TriggerSceneChange;
            _snapInteractable.WhenSelectingInteractorViewRemoved -= TriggerReturnToHub;
        }
    }

    // --- LOGIQUE ALLER ---
    private void TriggerSceneChange(IInteractorView interactor)
    {
        // On ne déclenche l'aller que si on est dans le Hub
        if (SceneManager.GetActiveScene().name == "Hub")
        {
            MonoBehaviour interactorObj = interactor as MonoBehaviour;
            if (interactorObj == null) return;

            string sceneName = interactorObj.transform.parent.name;
            TriggerVibration(0.5f);
            StartCoroutine(TransitionRoutine(sceneName));
        }
    }

    // --- LOGIQUE RETOUR ---
    private void TriggerReturnToHub(IInteractorView interactor)
    {
        // On ne déclenche le retour que si on n'est PAS déjà dans le Hub
        if (SceneManager.GetActiveScene().name != "Hub")
        {
            Debug.Log("Objet retiré : Retour au Hub demandé.");
            TriggerVibration(0.3f); // Vibration plus légère pour le retrait
            StartCoroutine(TransitionRoutine("Hub"));
        }
    }

    private void TriggerVibration(float strength)
    {
        OVRInput.SetControllerVibration(strength, strength, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(strength, strength, OVRInput.Controller.LTouch);
        Invoke("StopVibration", 0.2f);
    }

    private void StopVibration()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        yield return new WaitForSeconds(_delayBeforeFade);

        // Utilisation du fondu (OVRScreenFade est recommandé pour le casque)
        OVRScreenFade fader = FindObjectOfType<OVRScreenFade>();
        if (fader != null)
        {
            fader.fadeTime = _fadeDuration;
            fader.FadeOut();
            yield return new WaitForSeconds(_fadeDuration);
        }
        else if (_fadeCanvasGroup != null)
        {
            float timer = 0;
            while (timer < _fadeDuration)
            {
                timer += Time.deltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / _fadeDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene(sceneName);
    }
}