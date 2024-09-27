using Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] private int winScore = 1000;

    private int objectsCaught;
    private int objectsMissed;
    private readonly List<GameObject> objects = new();
    private bool won;
    private UnityAction onEnd;

    private void Start()
    {
        GameManager.EventService.Add<ObjectCaughtEvent>(ObjectCaught);
        GameManager.EventService.Add<ObjectMissedEvent>(ObjectMissed);
        GameManager.EventService.Add<MinigameStartedEvent>(StartMinigame);  
        container.SetActive(false);
    }

    public void StartMinigame(MinigameStartedEvent evt)
    {
        container.SetActive(true);
        objectsCaught = 0;
        objectsMissed = 0;
        won = false;
        onEnd = evt.OnEnd;
        StartCoroutine(SpawnObjects());
    }

    private void EndMinigame()
    {
        onEnd?.Invoke();
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

    public void ObjectLanded(bool caught)
    {
        if (caught)
        {
            objectsCaught++;
        }
        else
        {
            objectsMissed++;
        }

        if (objectsCaught >= objectCount)
        {
            won = true;
            GameManager.AddScore(winScore);
            StartCoroutine(EndAfterDelay());
        }
        else if (objectsMissed + objectsCaught >= objectCount)
        {
            won = false;
            StartCoroutine(EndAfterDelay());
        }
    }

    public void ObjectCaught()
    {
        ObjectLanded(true);
    }

    public void ObjectMissed()
    {
        ObjectLanded(false);
    }

    private IEnumerator EndAfterDelay()
    {
        NotificationManager.Notify(won ? "Minigame won!" : "Minigame lost!");
        yield return new WaitForSeconds(2f);
        EndMinigame();
    }
}
