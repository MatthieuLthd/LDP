using Oculus.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnToHubManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private SnapInteractable _snapInteractable;
    [SerializeField] private string _hubSceneName = "Hub";
    [SerializeField] private float _fadeDuration = 1.0f;

    private void Awake()
    {
        // Si non assigné, on le cherche sur l'objet
        if (_snapInteractable == null) 
            _snapInteractable = GetComponent<SnapInteractable>();
    }

    private void OnEnable()
    {
        if (_snapInteractable != null)
        {
            // On utilise l'événement de retrait de sélection
            _snapInteractable.WhenSelectingInteractorViewRemoved += ExitToHub;
        }
    }

    private void OnDisable()
    {
        if (_snapInteractable != null)
            _snapInteractable.WhenSelectingInteractorViewRemoved -= ExitToHub;
    }

    private void ExitToHub(IInteractorView interactor)
    {
        Debug.Log("OBJET RETIRÉ : Retour au Hub initié.");
        StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        // 1. Vibration haptique pour confirmer le retrait
        OVRInput.SetControllerVibration(0.4f, 0.4f, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0.4f, 0.4f, OVRInput.Controller.LTouch);

        // 2. Fondu au noir (OVRScreenFade est le plus fiable en VR)
        OVRScreenFade fader = FindObjectOfType<OVRScreenFade>();
        if (fader != null)
        {
            fader.fadeTime = _fadeDuration;
            fader.FadeOut();
            yield return new WaitForSeconds(_fadeDuration);
        }
        else
        {
            // Attente de sécurité si pas de fader
            yield return new WaitForSeconds(0.5f);
        }

        // 3. Arrêt vibration et chargement
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        
        SceneManager.LoadScene(_hubSceneName);
    }
}