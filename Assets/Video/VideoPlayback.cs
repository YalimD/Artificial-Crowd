using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/*
 * Inspired from http://wiki.unity3d.com/index.php?title=FramesPerSecond by Dave Hampson about FPS calculation
 */

public class VideoPlayback : MonoBehaviour {

    float deltaTime = 0.0f;
    AudioSource source;
    MovieTexture movie;
   RawImage r;
    float totalTime;
    List<Texture2D> frames;
    Texture2D texture;
   // Renderer r;
    /*
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
            //Original: 31.75
            source.pitch = (fps * 1000f) / 31.75f;
            source.Play();
            movie.Play();
            totalTime += fps * 1000f;
            
        }
       // if (movie.isPlaying)
      //     Debug.Log(totalTime / 30);
    }
    */
    
    void FixedUpdate()
    {
        if (RVO.PedestrianProjection.Instance.IsSimulationRunning && frameNum >= 0)
        {
           // byte[] bytes = System.IO.File.ReadAllBytes(Application.dataPath + "/Video/videoFrames/" +  (frameNum++) + ".img");
         //   byte[] bytes = frames[frameNum++].GetRawTextureData();
            
           // Debug.Log(texture.LoadImage(bytes));
           // texture.LoadImage(frames[frameNum++]);
         //   texture.Apply();
             r.texture = frames[frameNum++];
         //   r.material.mainTexture = texture;
          //  r.texture = (Texture)texture; 
            
        }
    }

    int frameNum;
    string videoName; //Get this from associated GUI code
    void Awake () {
            frames = new List<Texture2D>();
            videoName = "";

            frameNum = -1;

        
            r = GetComponent<RawImage>();
       //     r = GetComponent<Renderer>();
         /*
            movie = (MovieTexture)r.mainTexture;
            source = transform.GetComponent<AudioSource>();
            source.clip= movie.audioClip;

            //We adjust the fps of the videoplayback according to fps of given datafile. 
            // As unity always runs videos at 25 fps
            source.pitch = 30f / 25f;
            source.Play();
            movie.Play();
        */

            texture = new Texture2D(1280,720);
            r.texture = (Texture)texture;
          //  r.material.mainTexture = texture;
            LoadVideoImages();
            Debug.Log(frames.Count);
    }

    private void LoadVideoImages()
    {
        Texture2D tx;
       // byte[] b;
        frameNum = 0;
        do
        {
            //    try
            //   {
            tx = (Texture2D)Resources.Load("videoFrames/" + (frameNum++) + ".img", typeof(Texture2D));
            //  b = System.IO.File.ReadAllBytes(Application.dataPath + "/Video/videoFrames/" + (frameNum++) + ".img");
            //   tx.LoadImage(b);
            if (tx != null)
                frames.Add(tx);
            //  }
            //   catch (System.IO.FileNotFoundException exp) { break; }
        } while (tx != null);
        frameNum = 0;
    }

}
