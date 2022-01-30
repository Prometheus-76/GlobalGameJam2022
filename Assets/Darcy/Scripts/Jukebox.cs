using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jukebox : MonoBehaviour
{
    public List<AudioClip> songs;
    public AudioSource musicSource;

    private List<int> nextUpQueue;
    private List<int> tempIndexPool;
    private int lastSongID;

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Jukebox");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        nextUpQueue = new List<int>();
        tempIndexPool = new List<int>();
        ShuffleSongQueue();
    }

    // Update is called once per frame
    void Update()
    {
        if (musicSource.isPlaying == false)
        {
            // Is there another song that can be played?
            if (nextUpQueue.Count > 0)
            {
                musicSource.clip = songs[nextUpQueue[0]];
                musicSource.Play();
                lastSongID = nextUpQueue[0];
                nextUpQueue.RemoveAt(0);
            }
            else
            {
                ShuffleSongQueue();
            }
        }
    }

    void ShuffleSongQueue()
    {
        nextUpQueue.Clear();
        tempIndexPool.Clear();

        // Fill temp pool
        for (int i = 0; i < songs.Count; i++)
        {
            tempIndexPool.Add(i);
        }

        // Generate shuffled indexes
        for (int i = 0; i < tempIndexPool.Count; i++)
        {
            int choice = Random.Range(0, tempIndexPool.Count);
            nextUpQueue.Add(tempIndexPool[choice]);
            tempIndexPool.RemoveAt(choice);
            i--;
        }

        // Prevent double ups between queues
        if (lastSongID == nextUpQueue[0])
        {
            nextUpQueue.Reverse();
        }
    }
}
