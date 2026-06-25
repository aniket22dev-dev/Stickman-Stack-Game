using System.Collections;
using UnityEngine;

public class Stack : MonoBehaviour
{
    public GameObject[] Stacker;
    public bool canStack = true;
    private GameObject Player;
    public GameObject canvas;

    private float stacker0OriginalX;
    private float stacker1OriginalX;

    [HideInInspector] public bool lastSpawnFromRight = true;

    [HideInInspector] public float currentStopperX0 = 0f;
    [HideInInspector] public float currentStopperX1 = 0f;

    private void Start()
    {
        Player = GameObject.FindWithTag("Player");

        if (Player != null && Player.GetComponent<Stickman>() == null)
            Player = Player.GetComponentInChildren<Stickman>().gameObject;

        stacker0OriginalX = Stacker[0].transform.position.x;
        stacker1OriginalX = Stacker[1].transform.position.x;

        currentStopperX0 = stacker0OriginalX;
        currentStopperX1 = stacker1OriginalX;
    }

    void UpdateMoveSpeed(int score)
    {
        float newSpeed = 0.5f;

        if (score >= 50) newSpeed = 0.70f;
        else if (score >= 40) newSpeed = 0.66f;
        else if (score >= 30) newSpeed = 0.62f;
        else if (score >= 20) newSpeed = 0.58f;
        else if (score >= 10) newSpeed = 0.54f;

        // Apply to all currently spawned objects
        Spawn spawnComp = Stacker[0].GetComponent<Spawn>();
        if (spawnComp != null)
        {
            foreach (GameObject obj in spawnComp.spawnedObjects)
            {
                if (obj != null)
                {
                    Move m = obj.GetComponent<Move>();
                    if (m != null) m.speed = Mathf.Min(newSpeed, 0.7f);
                }
            }
            // Store it so future spawns also use the new speed
            spawnComp.prefab.GetComponent<Move>().speed = Mathf.Min(newSpeed, 0.7f);
        }
    }

    public void Stacked()
    {
        if (!canStack) return;

        // ── FIX: lock immediately — before ANY other logic runs.
        // Previously canStack was set to false inside the coroutine (after a yield),
        // meaning a second OnTriggerEnter could slip through in the same frame and
        // call Stacked() again, incrementing Y twice or corrupting the stopper state.
        canStack = false;

        GameObject Detector_ = GameObject.FindWithTag("Onit");
        if (Detector_ != null)
            Detector_.SetActive(false);

        foreach (GameObject t in Player.GetComponent<Stickman>().cubes)
        {
            t.gameObject.tag = "Ground";
            t.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }

        // ── FIX: snapshot Y before incrementing so the coroutine resets to the
        // correct height even if currentStopperX changes before Rebot_ runs.
        float newY0 = Stacker[0].transform.position.y + 0.19f;
        float newY1 = Stacker[1].transform.position.y + 0.19f;
        Stacker[0].transform.position = new Vector3(Stacker[0].transform.position.x, newY0, Stacker[0].transform.position.z);
        Stacker[1].transform.position = new Vector3(Stacker[1].transform.position.x, newY1, Stacker[1].transform.position.z);

        canvas.GetComponent<Points>().score++;
        Stacker[0].GetComponent<Spawn>().Check_interval = true;
        canvas.GetComponent<Points>().Pluspoint();
        UpdateMoveSpeed(canvas.GetComponent<Points>().score);
        StartCoroutine(Rebot_());
    }

    IEnumerator Rebot_()
    {
        yield return new WaitForSeconds(0.5f);

        // Reset X to wherever Spawn.cs last moved the stopper (left or right)
        // Y is already committed above — we only touch X here
        Vector3 pos0 = Stacker[0].transform.position;
        Vector3 pos1 = Stacker[1].transform.position;
        Stacker[0].transform.position = new Vector3(currentStopperX0, pos0.y, pos0.z);
        Stacker[1].transform.position = new Vector3(currentStopperX1, pos1.y, pos1.z);

        canStack = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Stack")
        {
            other.gameObject.GetComponent<Move>().IsMove = false;
            other.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Player.GetComponent<Stickman>().cubes.Add(other.gameObject);
            Stacked();
        }
    }
}