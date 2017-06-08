using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class testVideo : MonoBehaviour {


    float deltaTime = 0.0f;
    AudioSource source;
    MovieTexture movie;
    
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
            source.pitch = (fps * 1000f) / 31.75f;
            source.Play();
            movie.Play();
        }
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
