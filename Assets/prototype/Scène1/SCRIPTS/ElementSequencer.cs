using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElementSequencer : MonoBehaviour
{
    [System.Serializable]
    public class SequenceElement
    {
        public string name; // Pour s'y retrouver dans l'inspecteur
        public GameObject prefab;
        public float delayBeforeStart;
        public bool targetState = true; // ON ou OFF
        public float duration;
    }

    public List<SequenceElement> elements;

    void Start()
    {
        foreach (var element in elements)
        {
            StartCoroutine(ExecuteElement(element));
        }
    }

    IEnumerator ExecuteElement(SequenceElement element)
    {
        if (element.prefab == null) yield break;

        // 1. Attente avant le début
        yield return new WaitForSeconds(element.delayBeforeStart);

        // 2. Application de l'état (SetActive)
        element.prefab.SetActive(element.targetState);

        // 3. Si l'état était ON, on attend la durée pour le repasser en OFF
        if (element.targetState == true && element.duration > 0)
        {
            yield return new WaitForSeconds(element.duration);
            element.prefab.SetActive(false);
        }
        // Si l'état était OFF, on peut imaginer le rallumer après la durée
        else if (element.targetState == false && element.duration > 0)
        {
            yield return new WaitForSeconds(element.duration);
            element.prefab.SetActive(true);
        }
    }
}