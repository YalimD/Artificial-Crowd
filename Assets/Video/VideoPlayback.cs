using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Inspired from http://wiki.unity3d.com/index.php?title=FramesPerSecond by Dave Hampson about FPS calculation
 */

public class VideoPlayback : MonoBehaviour {

    float deltaTime = 0.0f;
    AudioSource source;
    MovieTexture movie;
    float totalTime;
    void Update()
    {
        
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = (1.0f / (deltaTime * 1000.0f));
        if (source != null)
        {
      //      Debug.Log("Changed");
            source.Pause();
            movie.Pause();

            //Actually it should be 30fps, but because of shadows we cannot
            //get acceptable accuracy

            //As the shadows are still considered in the given output file, there can be
            //inaccuracies betweeen the agents in the video and their projection
            source.pitch = (fps * 1000f) / 31.75f;
            source.Play();
            movie.Play();
            totalTime += fps * 1000f;
            
        }
        if (movie.isPlaying)
            Debug.Log(totalTime / 30);
    }

    void Start () {
        
            RawImage r = GetComponent<RawImage>();
         
            movie = (MovieTexture)r.mainTexture;
            source = transform.GetComponent<AudioSource>();
            source.clip= movie.audioClip;

            //We adjust the fps of the videoplayback according to fps of given datafile. 
            // As unity always runs videos at 25 fps
            source.pitch = 30f / 25f;
            source.Play();
            movie.Play();
            
    }

}
