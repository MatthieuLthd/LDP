using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VRElementSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ElementData
    {
        public string name;
        public GameObject prefab;
        public string introAnimationTrigger = "Start";
        public string outroAnimationTrigger = "End";
        public int quantity = 5;
        public float elementScale = 1f;
    }

    [Header("Paramètres Globaux")]
    public float globalActivationDelay = 2f;
    public Vector2 zoneSize = new Vector2(10f, 10f);
    public LayerMask groundLayer;
    public Transform playerTransform;

    [Header("Liste des Éléments")]
    public List<ElementData> elementsToSpawn;

    private void Start()
    {
        if (playerTransform == null)
            playerTransform = Camera.main.transform;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(globalActivationDelay);

        foreach (var element in elementsToSpawn)
        {
            for (int i = 0; i < element.quantity; i++)
            {
                SpawnElement(element);
            }
        }
    }

    void SpawnElement(ElementData data)
    {
        float randomX = Random.Range(-zoneSize.x / 2, zoneSize.x / 2);
        float randomZ = Random.Range(-zoneSize.y / 2, zoneSize.y / 2);
        Vector3 spawnPos = transform.position + new Vector3(randomX, 10f, randomZ);

        if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            GameObject instance = Instantiate(data.prefab, hit.point, Quaternion.identity);
            instance.name = data.name;
            instance.transform.localScale = Vector3.one * data.elementScale;

            // --- CALCUL DE LA ROTATION ---
            Vector3 directionToPlayer = playerTransform.position - hit.point;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, hit.normal);
            Quaternion localCorrection = Quaternion.Euler(0f, 0f, 90f);
            
            Quaternion finalRotation = targetRotation * localCorrection;
            instance.transform.rotation = finalRotation;

            // --- CORRECTION POUR L'ANIMATION ---
            // On ajoute un composant qui va forcer cette rotation précise à chaque frame
            // après que l'Animator ait fini son travail.
            RotationFixer fixer = instance.AddComponent<RotationFixer>();
            fixer.lockedRotation = finalRotation;

            // 4. Lecture de l'animation d'intro
            Animator anim = instance.GetComponent<Animator>();
            if (anim != null && !string.IsNullOrEmpty(data.introAnimationTrigger))
            {
                anim.SetTrigger(data.introAnimationTrigger);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(zoneSize.x, 0.1f, zoneSize.y));
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(zoneSize.x, 0.1f, zoneSize.y));
    }
}

// --- PETIT SCRIPT UTILITAIRE ---
// Ce script s'assure que la rotation ne bouge pas malgré l'Animator
public class RotationFixer : MonoBehaviour
{
    public Quaternion lockedRotation;

    void LateUpdate()
    {
        // LateUpdate s'exécute APRÈS l'Animator. 
        // On ré-applique la rotation souhaitée ici.
        transform.rotation = lockedRotation;
    }
}