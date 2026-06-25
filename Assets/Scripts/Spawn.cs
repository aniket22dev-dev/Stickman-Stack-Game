using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject prefab;
    public float spawnInterval = 10f;
    private GameObject Stopper;
    private Stack stackComp;
    public List<GameObject> spawnedObjects = new List<GameObject>();
    public GameObject canvas;
    public bool Check_interval = false;

    private const float MIN_INTERVAL = 2.5f;

    // Set these in Inspector to exact world X where prefab appears
    public float spawnRightX = 5f;
    public float spawnLeftX = -5f;

    // Set these in Inspector to exact world X where stopper should sit for each side
    public float stopperXFromRight = 2f;
    public float stopperXFromLeft = -2f;

    // ── FIX: track whether a prefab is currently in-flight (spawned but not yet stacked/destroyed)
    // We block a new spawn until the active one is resolved, preventing stopper X from being
    // overwritten mid-flight which caused cubes to stack at the same Y position.
    private bool prefabInFlight = false;

    void Start()
    {
        Stopper = GameObject.FindWithTag("Use");
        stackComp = Stopper.GetComponent<Stack>();

        // Subscribe so we know when a stack event completes
        // Stack.cs calls Stacked() → we listen via the existing canStack flag (polled below)
    }

    private void Update()
    {
        if (canvas.GetComponent<Points>().score % 2 == 0 && Check_interval == true && spawnInterval > MIN_INTERVAL)
        {
            spawnInterval = Mathf.Max(spawnInterval - 0.5f, MIN_INTERVAL);
            Check_interval = false;
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Wait for the interval first
            yield return new WaitForSeconds(Mathf.Max(spawnInterval, MIN_INTERVAL));

            // ── FIX: also wait until any in-flight prefab is fully resolved
            // "Resolved" means either: stacked (Stack.canStack went false then back to true)
            // OR the prefab flew off-screen and was cleaned up.
            // We poll canStack — once it returns to true the Rebot_ coroutine has finished
            // and the stopper Y has been committed, so it is safe to move the stopper to the
            // next side without corrupting the previous stack.
            if (prefabInFlight)
            {
                // Give it up to 2 seconds; in practice Rebot_ takes 0.5 s
                float waited = 0f;
                while (prefabInFlight && waited < 2f)
                {
                    // Consider in-flight resolved when Stack is ready again OR prefab is gone
                    bool allGone = true;
                    foreach (GameObject obj in spawnedObjects)
                    {
                        if (obj != null) { allGone = false; break; }
                    }

                    if (allGone || (stackComp != null && stackComp.canStack))
                        prefabInFlight = false;

                    if (prefabInFlight)
                    {
                        waited += Time.deltaTime;
                        yield return null;
                    }
                }
                prefabInFlight = false; // safety fallthrough
            }

            Spawner();
        }
    }

    void Start_Spawn()
    {
        StartCoroutine(SpawnRoutine());
    }

    void Awake()
    {
        Invoke("Start_Spawn", 0.1f); // slight delay so Start() finishes first
    }

    void Spawner()
    {
        // ── FIX: collect objects to destroy in a separate pass, never modify a list mid-foreach
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null &&
                (obj.transform.position.x < spawnLeftX - 5f ||
                 obj.transform.position.x > spawnRightX + 5f))
            {
                toDestroy.Add(obj);
            }
        }
        foreach (GameObject obj in toDestroy)
            Destroy(obj);

        spawnedObjects.RemoveAll(obj => obj == null);

        // Randomly pick a side
        bool spawnFromRight = Random.value > 0.5f;

        float spawnX = spawnFromRight ? spawnRightX : spawnLeftX;
        float direction = spawnFromRight ? 1f : -1f;

        if (spawnFromRight)
        {
            Debug.Log("Coming from Right");

            if (Stopper != null)
            {
                Vector3 pos = Stopper.transform.position;
                Stopper.transform.position = new Vector3(stopperXFromRight, pos.y, pos.z);
            }

            if (stackComp != null)
            {
                stackComp.currentStopperX0 = stopperXFromRight;
                stackComp.currentStopperX1 = stopperXFromRight;
            }
        }
        else
        {
            Debug.Log("Coming from Left");

            if (Stopper != null)
            {
                Vector3 pos = Stopper.transform.position;
                Stopper.transform.position = new Vector3(stopperXFromLeft, pos.y, pos.z);
            }

            if (stackComp != null)
            {
                stackComp.currentStopperX0 = stopperXFromLeft;
                stackComp.currentStopperX1 = stopperXFromLeft;
            }
        }

        Vector3 spawnPos = new Vector3(spawnX, transform.position.y, transform.position.z);
        GameObject newObj = Instantiate(prefab, spawnPos, Quaternion.identity);

        Move moveComp = newObj.GetComponent<Move>();
        if (moveComp != null)
            moveComp.direction = direction;

        PrefabDirection dirComp = newObj.GetComponent<PrefabDirection>();
        if (dirComp == null)
            dirComp = newObj.AddComponent<PrefabDirection>();
        dirComp.comingFromRight = spawnFromRight;

        spawnedObjects.Add(newObj);

        // ── FIX: mark a prefab as in-flight so next spawn waits for this one to resolve
        prefabInFlight = true;
    }
}