using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction;
using System.Collections;
using TMPro;

public class ReturnToHubSimple : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject _objetAObserver;
    [SerializeField] private TextMeshPro _texteCompteur;
    [SerializeField] private float _tempsDeMaintienRequis = 3.0f;
    [SerializeField] private string _nomDuHub = "Hub";

    private IInteractableView _interactableView;
    private Rigidbody _rb;
    private float _timer = 0f;
    private bool _isChangingScene = false;

    // Variables pour mémoriser la position de départ
    private Vector3 _positionInitiale;
    private Quaternion _rotationInitiale;

    void Start()
    {
        if (_objetAObserver == null)
            _objetAObserver = GameObject.Find(SceneManager.GetActiveScene().name);

        if (_objetAObserver != null)
        {
            _interactableView = _objetAObserver.GetComponentInChildren<IInteractableView>();
            _rb = _objetAObserver.GetComponent<Rigidbody>();

            // 1. On mémorise la position et la rotation exactes au démarrage
            _positionInitiale = _objetAObserver.transform.position;
            _rotationInitiale = _objetAObserver.transform.rotation;
            
            Debug.Log($"<color=green>[HUB] Position initiale enregistrée pour : {_objetAObserver.name}</color>");
        }

        if (_texteCompteur != null) _texteCompteur.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_interactableView == null || _isChangingScene) return;

        if (_interactableView.State == InteractableState.Select)
        {
            _timer += Time.deltaTime;
            AfficherCompteur();

            if (_timer >= _tempsDeMaintienRequis)
                StartCoroutine(RoutineChangementScene());
        }
        else
        {
            // 2. Si le timer avait commencé mais qu'on a lâché l'objet
            if (_timer > 0)
            {
                ReplacerObjet();
            }
            ResetCompteur();
        }
    }

    private void ReplacerObjet()
    {
        Debug.Log("[HUB] Objet lâché trop tôt : Retour à la position initiale.");
        
        // On remet l'objet à sa place exacte
        _objetAObserver.transform.position = _positionInitiale;
        _objetAObserver.transform.rotation = _rotationInitiale;

        // On stoppe tout mouvement physique (vitesse) pour éviter qu'il ne s'envole
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    private void AfficherCompteur()
    {
        if (_texteCompteur == null) return;
        _texteCompteur.gameObject.SetActive(true);
        _texteCompteur.text = Mathf.Ceil(_tempsDeMaintienRequis - _timer).ToString();
    }

    private void ResetCompteur()
    {
        _timer = 0f;
        if (_texteCompteur != null) _texteCompteur.gameObject.SetActive(false);
    }

    private IEnumerator RoutineChangementScene()
    {
        _isChangingScene = true;
        if (_texteCompteur != null) _texteCompteur.text = "GO !";
        
        OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.RTouch);

        OVRScreenFade fader = Object.FindFirstObjectByType<OVRScreenFade>();
        if (fader != null)
        {
            fader.FadeOut();
            yield return new WaitForSeconds(fader.fadeTime);
        }
        else yield return new WaitForSeconds(0.5f);

        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        SceneManager.LoadScene(_nomDuHub);
    }
}