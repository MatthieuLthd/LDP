using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteTerrainManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableElement
    {
        public string label = "Objet";
        public GameObject prefab;
        public bool isEnabled = true;
        public int count = 10;
        
        [Header("Animations")]
        public string entryAnimName = "SpawnIn";
        public string exitAnimName = "SpawnOut";
        [Tooltip("Temps d'attente avant de jouer l'animation de sortie")]
        public float delayBeforeExit = 5f;
        [Tooltip("Durée de l'animation de sortie avant destruction de l'objet")]
        public float exitAnimDuration = 1.5f;

        [Header("Style & Physique")]
        public Vector3 scale = Vector3.one;
        public float overlapRadius = 2f;
    }

    [Header("Délais Globaux")]
    public float initialGlobalDelay = 2f;
    public float delayBetweenSpawns = 0.05f;

    [Header("Configurations")]
    public List<SpawnableElement> elementsToSpawn = new List<SpawnableElement>();
    public Vector2 areaSize = new Vector2(50, 50);
    public LayerMask terrainLayer;
    public LayerMask obstacleLayer;
    public bool alignToSurfaceNormal = true;

    void Start()
    {
        StartCoroutine(MasterRoutine());
    }

    IEnumerator MasterRoutine()
    {
        // 1. Délai global avant le début de toute l'opération
        yield return new WaitForSeconds(initialGlobalDelay);

        GameObject mainContainer = new GameObject("--- DYNAMIC_ENVIRONMENT ---");

        foreach (var element in elementsToSpawn)
        {
            if (!element.isEnabled || element.prefab == null) continue;

            int spawnedCount = 0;
            int attempts = 0;
            while (spawnedCount < element.count && attempts < element.count * 10)
            {
                if (TrySpawn(element, mainContainer.transform))
                {
                    spawnedCount++;
                    yield return new WaitForSeconds(delayBetweenSpawns);
                }
                attempts++;
            }
        }
    }

    bool TrySpawn(SpawnableElement element, Transform parentFolder)
    {
        float randomX = Random.Range(transform.position.x - areaSize.x / 2, transform.position.x + areaSize.x / 2);
        float randomZ = Random.Range(transform.position.z - areaSize.y / 2, transform.position.z + areaSize.y / 2);
        Vector3 rayOrigin = new Vector3(randomX, 500f, randomZ);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1000f, terrainLayer))
        {
            if (Physics.CheckSphere(hit.point, element.overlapRadius, obstacleLayer))
                return false;

            // 2. Création du Pivot (Garantit la rotation vers 0,0,0)
            GameObject pivot = new GameObject("[Pivot] " + element.label);
            pivot.transform.position = hit.point;
            pivot.transform.SetParent(parentFolder);

            Vector3 dirToCenter = (Vector3.zero - hit.point);
            dirToCenter.y = 0;
            if (dirToCenter != Vector3.zero) 
                pivot.transform.rotation = Quaternion.LookRotation(dirToCenter);

            if (alignToSurfaceNormal)
                pivot.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * pivot.transform.rotation;

            // 3. Création de l'élément visuel
            GameObject instance = Instantiate(element.prefab, pivot.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            pivot.transform.localScale = element.scale;

            // 4. Lancement du cycle de vie (Entrée -> Attente -> Sortie -> Destruction)
            StartCoroutine(LifeCycleRoutine(instance, pivot, element));

            return true;
        }
        return false;
    }

    IEnumerator LifeCycleRoutine(GameObject instance, GameObject pivot, SpawnableElement settings)
    {
        Animator anim = instance.GetComponent<Animator>();

        // Animation d'entrée
        if (anim != null) anim.Play(settings.entryAnimName);

        // Attente avant la sortie
        yield return new WaitForSeconds(settings.delayBeforeExit);

        // Animation de sortie
        if (anim != null) anim.Play(settings.exitAnimName);

        // Attente de la fin de l'anim de sortie avant de supprimer
        yield return new WaitForSeconds(settings.exitAnimDuration);

        Destroy(pivot);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 2, areaSize.y));
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Vector3.zero, 1f);
    }
}