using Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: base class 
public class CatcherMinigame : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject catcherPlatform;
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private BoxCollider2D spawnCollider;
    [SerializeField] private float objectDropInterval = 1f;
    [SerializeField] private int objectCount = 3;
    [SerializeField] private float catcherSpeed = 5f;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;

    private int objectsCaught;
    private int objectsMissed;
    private readonly List<GameObject> objects = new();

    private void Start()
    {
        GameManager.EventService.Add<ObjectCaughtEvent>(ObjectCaught);
        GameManager.EventService.Add<ObjectMissedEvent>(ObjectMissed);
        GameManager.EventService.Add<MinigameStartedEvent>(StartMinigame);
        container.SetActive(false);
    }

    public void StartMinigame()
    {
        container.SetActive(true);
        objectsCaught = 0;
        objectsMissed = 0;
        StartCoroutine(SpawnObjects());
    }

    private void EndMinigame()
    {
        GameManager.EventService.Dispatch<MinigameEndedEvent>();
        objects.ForEach(o => Destroy(o));
        objects.Clear();
        container.SetActive(false);
    }

    private void Update()
    {
        if (!GameManager.MinigameActive)
        {
            return;
        }

        var input = Input.GetAxis("Horizontal");
        catcherPlatform.transform.Translate(catcherSpeed * input * Time.deltaTime * Vector2.right);
    }

    private IEnumerator SpawnObjects()
    {
        for (int i = 0; i < objectCount; i++)
        {
            var bounds = spawnCollider.bounds;
            var size = objectPrefab.GetComponent<SpriteRenderer>().bounds.size;
            var minX = bounds.min.x + size.x / 2;
            var maxX = bounds.max.x - size.x / 2;

            var randomX = Random.Range(minX, maxX);
            var spawnPosition = new Vector3(randomX, bounds.max.y, 0f);

            var instance = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);
            objects.Add(instance);
            yield return new WaitForSeconds(objectDropInterval);
        }
    }

    public void ObjectCaught()
    {
        objectsCaught++;
        if (objectsCaught >= objectCount)
        {
            StartCoroutine(EndAfterDelay());
        }
    }

    public void ObjectMissed()
    {
        objectsMissed++;
        if (objectsMissed + objectsCaught >= objectCount)
        {
            StartCoroutine(EndAfterDelay());
        }
    }

    private IEnumerator EndAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        EndMinigame();
    }
}
