using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiToggleWithDelay : MonoBehaviour
{
    // On définit ce qu'est un "élément" à gérer
    [System.Serializable]
    public class ObjectSettings
    {
        public string name; // Pour s'y retrouver dans l'inspector
        public GameObject targetObject;
        public KeyCode key;
        public float delay = 0.5f;
        public bool isToggle = true;
    }

    [Header("Liste des éléments")]
    public List<ObjectSettings> elements = new List<ObjectSettings>();

    void Update()
    {
        // On parcourt la liste à chaque frame pour vérifier les touches
        foreach (var item in elements)
        {
            if (Input.GetKeyDown(item.key))
            {
                // On lance une coroutine pour gérer le délai sans bloquer le jeu
                StartCoroutine(ProcessAction(item));
            }
        }
    }

    IEnumerator ProcessAction(ObjectSettings item)
    {
        if (item.targetObject == null) yield break;

        // Attente du délai spécifié
        yield return new WaitForSeconds(item.delay);

        if (item.isToggle)
        {
            item.targetObject.SetActive(!item.targetObject.activeSelf);
        }
        else
        {
            item.targetObject.SetActive(true);
        }
    }
}