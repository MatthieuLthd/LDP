using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElementManagerPro : MonoBehaviour
{
    [System.Serializable]
    public class ElementData
    {
        public string nomOutil = "Cyclone";
        public bool estActif = true; 
        
        [Header("Référence")]
        public GameObject objetDansLaScene; 
        
        [Header("Noms des Etats d'Animation")]
        public string nomAnimEntree = "Entree";
        public string nomAnimSortie = "Sortie";

        [Header("Timing (Secondes)")]
        public float delaiAvantApparition = 1.0f;
        public float dureeDePresence = 3.0f;
        public float dureeAnimSortie = 1.0f;
    }

    public List<ElementData> listeElements = new List<ElementData>();

    void Start()
    {
        foreach (ElementData data in listeElements)
        {
            if (data.objetDansLaScene != null)
            {
                // On s'assure qu'il est éteint au départ
                data.objetDansLaScene.SetActive(false); 
                
                if (data.estActif)
                {
                    Debug.Log($"[Manager] Lancement de la séquence pour : {data.nomOutil}");
                    StartCoroutine(GererCycleVieElement(data));
                }
            }
            else
            {
                Debug.LogError("Il manque un objet dans la liste d'ElementManager !");
            }
        }
    }

    IEnumerator GererCycleVieElement(ElementData data)
    {
        // 1. Attente initiale
        yield return new WaitForSeconds(data.delaiAvantApparition);

        // 2. Activation
        data.objetDansLaScene.SetActive(true);
        Debug.Log($"[Manager] {data.nomOutil} est maintenant ACTIF.");

        // 3. Petite pause d'une frame pour laisser l'Animator s'initialiser
        yield return new WaitForEndOfFrame();

        Animator anim = data.objetDansLaScene.GetComponent<Animator>();
        if (anim != null && !string.IsNullOrEmpty(data.nomAnimEntree))
        {
            anim.Play(data.nomAnimEntree);
            Debug.Log($"[Manager] Animation d'entrée lancée pour : {data.nomOutil}");
        }

        // 4. Temps de présence
        yield return new WaitForSeconds(data.dureeDePresence);

        // 5. Animation de sortie
        if (anim != null && !string.IsNullOrEmpty(data.nomAnimSortie))
        {
            anim.Play(data.nomAnimSortie);
            Debug.Log($"[Manager] Animation de sortie lancée pour : {data.nomOutil}");
        }

        // 6. Attente de la fin et disparition
        yield return new WaitForSeconds(data.dureeAnimSortie);
        data.objetDansLaScene.SetActive(false);
        Debug.Log($"[Manager] {data.nomOutil} est maintenant DÉSACTIVÉ.");
    }
}