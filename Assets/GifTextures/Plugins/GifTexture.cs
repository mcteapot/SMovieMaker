using UnityEngine;
using System.IO;
using Gif2Textures;

public class GifTexture : MonoBehaviour
{
    public string m_GifFileName;
    public bool m_FasterButMoreMemoryUsage = true;

    float m_Timer = 0.0f;
    float m_CurrentDelay = 0;
    GifFrames m_GifFrames = null;

    void Start()
    {
        TextAsset ta = Resources.Load(m_GifFileName) as TextAsset;
        if (ta == null)
        {
            Debug.LogWarning("Can't open Gif file \"" + m_GifFileName + "\" for object: " + gameObject.name);
            return;
        }

        MemoryStream ms = new MemoryStream(ta.bytes);

        m_GifFrames = new GifFrames();
        if (m_GifFrames.Load(ms, m_FasterButMoreMemoryUsage))
            SetNextFrame();
        else
            m_GifFrames = null;
    }

    void Update()
    {
        if (m_GifFrames != null)
        {
            m_Timer += Time.deltaTime;

            if (m_Timer >= m_CurrentDelay)
            {
                m_Timer -= m_CurrentDelay;
                SetNextFrame();
            }
        }
    }

    void SetNextFrame()
    {
        Texture2D tex;
        m_GifFrames.GetNextFrame(out tex, out m_CurrentDelay);
        if (renderer != null && renderer.material != null)
            renderer.material.mainTexture = tex;
        if (guiTexture != null)
            guiTexture.texture = tex;
    }
}
