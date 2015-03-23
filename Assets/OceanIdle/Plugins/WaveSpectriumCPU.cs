using UnityEngine;
using System;
using System.Collections;
using System.Threading;

namespace OceanSurfaceEffects
{
    /// <summary>
    /// 
    /// Generates a wave spectrum using the formula in the follow research paper.
    /// Can the found with a bit of googling.
    /// 
    /// WAVES SPECTRUM
    /// using "A unified directional spectrum for long and short wind-driven waves"
    /// T. Elfouhaily, B. Chapron, K. Katsaros, D. Vandemark
    /// Journal of Geophysical Research vol 102, p781-796, 1997
	/// 
    /// </summary>
    public class WaveSpectrumCPU
    {
        //CONST DONT CHANGE
        const float WAVE_CM = 0.23f;	// Eq 59
        const float WAVE_KM = 370.0f;	// Eq 59

		/// <summary>
		/// Has the thread generating the data finished.
		/// </summary>
		public volatile bool done = true;

        /// <summary>
        /// The current buffer index.
        /// Flips between 0 and 1 each frame. 
        /// </summary>
		volatile int m_bufferIdx = 0;
		
		/// <summary>
		/// This is the fourier transform size, must pow2 number. 
        /// </summary>
        int m_size = 64;
        float m_fsize;

        /// <summary>
        /// A higher wind speed gives greater swell to the waves.
        /// </summary>
        float m_windSpeed = 8.0f;

        /// <summary>
        /// Scales the height of the waves.
        /// </summary>
        float m_waveAmp = 1.0f;

        /// <summary>
        /// A lower number means the waves last longer and 
        /// will build up larger waves.
        /// </summary>
        float m_omega = 0.84f;

        /// <summary>
        /// The waves are made up of 4 layers of heights
        /// at different wave lengths. These grid sizes 
        /// are basically the wave length for each layer.
        /// </summary>
        public Vector4 gridSizes { get { return m_gridSizes; } }
        Vector4 m_gridSizes = new Vector4(5488, 392, 28, 2);
        Vector4 m_inverseGridSizes;

        /// <summary>
        /// Number of mipmap levels in the height maps.
        /// </summary>
        public float mipMapLevels { get { return m_mipMapLevels; } }
        float m_mipMapLevels;
        
        /// <summary>
        /// The fourier buffers and spectrum data.
        /// </summary>
        Vector2[,] m_fourierBuffer0;
        Vector4[,] m_fourierBuffer1, m_fourierBuffer2;
        Vector4[] m_spectrum01, m_spectrum23, m_WTable;

		/// <summary>
		/// The arrays to hold results that get written to textures.
		/// </summary>
		Color[] m_result0, m_result1, m_result2;

        /// <summary>
        /// The maps holding the height and slope data.
        /// </summary>
        public Texture2D map0 { get { return m_map0; } }
        public Texture2D map1 { get { return m_map1; } }
        public Texture2D map2 { get { return m_map2; } }
        Texture2D m_map0, m_map1, m_map2;

        /// <summary>
        /// Fourier object to do the fourier transform.
        /// </summary>
        FourierCPU m_fourier;

        public WaveSpectrumCPU(int size, float windSpeed, float waveAmp, float omega, int ansio, Vector4 gridSizes)
        {
            if (!Mathf.IsPowerOfTwo(size))
            {
                Debug.Log("Fourier grid size must be pow2 number, changing to nearest pow2 number");
                size = Mathf.NextPowerOfTwo(size);
            }

            //These are the recommend limits of wind speed and wave age.
			//If you exceed them you may see artifacts or objects not floating at wave height
            if (windSpeed > 8.0f)
            {
                Debug.Log("Wind speed must be <= 8.0, changing to 8.0");
                windSpeed = 8.0f;
            }

            if (omega < 0.8f)
            {
                Debug.Log("Inverse wave age must be >= 0.8, changing to 0.8");
                m_omega = 0.8f;
            }

            if (waveAmp < 0.0 || waveAmp > 1.0)
            {
                Debug.Log("Wave amp must be between 0 and 1, clamping");
                Mathf.Clamp01(waveAmp);
            }

            m_size = size;
            m_waveAmp = waveAmp;
            m_windSpeed = windSpeed;
            m_omega = omega;
            m_gridSizes = gridSizes;

            m_fsize = (float)m_size;
            m_mipMapLevels = Mathf.Log(m_fsize) / Mathf.Log(2.0f);

            float factor = 2.0f * Mathf.PI * m_fsize;
            m_inverseGridSizes = new Vector4(factor / m_gridSizes.x, factor / m_gridSizes.y, factor / m_gridSizes.z, factor / m_gridSizes.w);

            m_fourierBuffer0 = new Vector2[2, m_size * m_size];
            m_fourierBuffer1 = new Vector4[2, m_size * m_size];
            m_fourierBuffer2 = new Vector4[2, m_size * m_size];

			m_result0 = new Color[m_size * m_size];
			m_result1 = new Color[m_size * m_size];
			m_result2 = new Color[m_size * m_size];

            m_fourier = new FourierCPU(m_size);

            m_map0 = new Texture2D(m_size, m_size, TextureFormat.RGB24, true, true);
            m_map0.wrapMode = TextureWrapMode.Repeat;
            m_map0.filterMode = FilterMode.Trilinear;
            m_map0.anisoLevel = ansio;

            m_map1 = new Texture2D(m_size, m_size, TextureFormat.ARGB32, true, true);
            m_map1.wrapMode = TextureWrapMode.Repeat;
            m_map1.filterMode = FilterMode.Trilinear;
            m_map1.anisoLevel = ansio;

            m_map2 = new Texture2D(m_size, m_size, TextureFormat.ARGB32, true, true);
            m_map2.wrapMode = TextureWrapMode.Repeat;
            m_map2.filterMode = FilterMode.Trilinear;
            m_map2.anisoLevel = ansio;

            GenerateWavesSpectrum();
            CreateWTabele();
        }

        float sqr(float x) { return x * x; }

        float omega(float k) { return Mathf.Sqrt(9.81f * k * (1.0f + sqr(k / WAVE_KM))); } // Eq 24

        /// <summary>
        /// I know this is a big chunk of ugly math but dont worry to much about what it all means
        /// It recreates a statistcally representative model of a wave spectrum in the frequency domain.
        /// </summary>
        float Spectrum(float kx, float ky)
        {

            float U10 = m_windSpeed;

            // phase speed
            float k = Mathf.Sqrt(kx * kx + ky * ky);
            float c = omega(k) / k;

            // spectral peak
            float kp = 9.81f * sqr(m_omega / U10); // after Eq 3
            float cp = omega(kp) / kp;

            // friction velocity
            float z0 = 3.7e-5f * sqr(U10) / 9.81f * Mathf.Pow(U10 / cp, 0.9f); // Eq 66
            float u_star = 0.41f * U10 / Mathf.Log(10.0f / z0); // Eq 60

            float Lpm = Mathf.Exp(-5.0f / 4.0f * sqr(kp / k)); // after Eq 3
            float gamma = (m_omega < 1.0f) ? 1.7f : 1.7f + 6.0f * Mathf.Log(m_omega); // after Eq 3 // log10 or log?
            float sigma = 0.08f * (1.0f + 4.0f / Mathf.Pow(m_omega, 3.0f)); // after Eq 3
            float Gamma = Mathf.Exp(-1.0f / (2.0f * sqr(sigma)) * sqr(Mathf.Sqrt(k / kp) - 1.0f));
            float Jp = Mathf.Pow(gamma, Gamma); // Eq 3
            float Fp = Lpm * Jp * Mathf.Exp(-m_omega / Mathf.Sqrt(10.0f) * (Mathf.Sqrt(k / kp) - 1.0f)); // Eq 32
            float alphap = 0.006f * Mathf.Sqrt(m_omega); // Eq 34
            float Bl = 0.5f * alphap * cp / c * Fp; // Eq 31

            float alpham = 0.01f * (u_star < WAVE_CM ? 1.0f + Mathf.Log(u_star / WAVE_CM) : 1.0f + 3.0f * Mathf.Log(u_star / WAVE_CM)); // Eq 44
            float Fm = Mathf.Exp(-0.25f * sqr(k / WAVE_KM - 1.0f)); // Eq 41
            float Bh = 0.5f * alpham * WAVE_CM / c * Fm * Lpm; // Eq 40 (fixed)

            float a0 = Mathf.Log(2.0f) / 4.0f;
            float ap = 4.0f;
            float am = 0.13f * u_star / WAVE_CM; // Eq 59
            float Delta = (float)System.Math.Tanh(a0 + ap * Mathf.Pow(c / cp, 2.5f) + am * Mathf.Pow(WAVE_CM / c, 2.5f)); // Eq 57

            float phi = Mathf.Atan2(ky, kx);

            if (kx < 0.0f) return 0.0f;

            Bl *= 2.0f;
            Bh *= 2.0f;

            return m_waveAmp * (Bl + Bh) * (1.0f + Delta * Mathf.Cos(2.0f * phi)) / (2.0f * Mathf.PI * sqr(sqr(k))); // Eq 67
        }

        Vector2 GetSpectrumSample(float i, float j, float lengthScale, float kMin)
        {
            float dk = 2.0f * Mathf.PI / lengthScale;
            float kx = i * dk;
            float ky = j * dk;
            Vector2 result = new Vector2(0.0f, 0.0f);

            float rnd = UnityEngine.Random.value;

            if (Mathf.Abs(kx) >= kMin || Mathf.Abs(ky) >= kMin)
            {
                float S = Spectrum(kx, ky);
                float h = Mathf.Sqrt(S / 2.0f) * dk;

                float phi = rnd * 2.0f * Mathf.PI;
                result.x = h * Mathf.Cos(phi);
                result.y = h * Mathf.Sin(phi);
            }

            return result;
        }

        /// <summary>
        /// Generates the wave spectrum based on the 
        /// settings wind speed, wave amp and wave age.
        /// If these values change this function must be called again.
        /// </summary>
        void GenerateWavesSpectrum()
        {

            m_spectrum01 = new Vector4[m_size * m_size];
            m_spectrum23 = new Vector4[m_size * m_size];

            int idx;
            float i;
            float j;
            Vector2 sample12XY;
            Vector2 sample12ZW;
            Vector2 sample34XY;
            Vector2 sample34ZW;

            UnityEngine.Random.seed = 0;

            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    idx = x + y * m_size;
                    i = (x >= m_size / 2) ? (float)(x - m_size) : (float)x;
                    j = (y >= m_size / 2) ? (float)(y - m_size) : (float)y;

                    sample12XY = GetSpectrumSample(i, j, m_gridSizes.x, Mathf.PI / m_gridSizes.x);
                    sample12ZW = GetSpectrumSample(i, j, m_gridSizes.y, Mathf.PI * m_fsize / m_gridSizes.x);
                    sample34XY = GetSpectrumSample(i, j, m_gridSizes.z, Mathf.PI * m_fsize / m_gridSizes.y);
                    sample34ZW = GetSpectrumSample(i, j, m_gridSizes.w, Mathf.PI * m_fsize / m_gridSizes.z);

                    m_spectrum01[idx].x = sample12XY.x;
                    m_spectrum01[idx].y = sample12XY.y;
                    m_spectrum01[idx].z = sample12ZW.x;
                    m_spectrum01[idx].w = sample12ZW.y;

                    m_spectrum23[idx].x = sample34XY.x;
                    m_spectrum23[idx].y = sample34XY.y;
                    m_spectrum23[idx].z = sample34ZW.x;
                    m_spectrum23[idx].w = sample34ZW.y;

                }
            }

        }

        /// <summary>
        /// Some of the values needed in the InitWaveSpectrum function can be precomputed.
        /// If the grid sizes change this function must called again.
        /// </summary>
        void CreateWTabele()
        {
        
            Vector2 uv, st;
            float k1, k2, k3, k4, w1, w2, w3, w4;

            m_WTable = new Vector4[m_size * m_size];

            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    uv = new Vector2(x, y) / m_fsize;

                    st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
                    st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;

                    k1 = (st * m_inverseGridSizes.x).magnitude;
                    k2 = (st * m_inverseGridSizes.y).magnitude;
                    k3 = (st * m_inverseGridSizes.z).magnitude;
                    k4 = (st * m_inverseGridSizes.w).magnitude;

                    w1 = Mathf.Sqrt(9.81f * k1 * (1.0f + k1 * k1 / (WAVE_KM * WAVE_KM)));
                    w2 = Mathf.Sqrt(9.81f * k2 * (1.0f + k2 * k2 / (WAVE_KM * WAVE_KM)));
                    w3 = Mathf.Sqrt(9.81f * k3 * (1.0f + k3 * k3 / (WAVE_KM * WAVE_KM)));
                    w4 = Mathf.Sqrt(9.81f * k4 * (1.0f + k4 * k4 / (WAVE_KM * WAVE_KM)));

                    m_WTable[x + y * m_size] = new Vector4(w1, w2, w3, w4);
                }
            }

        }

        Vector2 GetSpectrum(float t, float w, float s0x, float s0y, float s0cx, float s0cy)
        {
            float c = Mathf.Cos(w * t);
            float s = Mathf.Sin(w * t);
            return new Vector2((s0x + s0cx) * c - (s0y + s0cy) * s, (s0x - s0cx) * s + (s0y - s0cy) * c);
        }

        Vector2 COMPLEX(Vector2 z)
        {
            return new Vector2(-z.y, z.x); // returns i times z (complex number)
        }

        /// <summary>
        /// Initilize the spectrum for a time period.
        /// </summary>
        void InitWaveSpectrum(float time)
        {
            Vector2 uv, st, k1, k2, k3, k4, h1, h2, h3, h4, h12, n1, n2, n3, n4;
            Vector4 s12, s34, s12c, s34c;
            int rx, ry;

            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    uv.x = x / m_fsize;
					uv.y = y / m_fsize;

                    st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
                    st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;

                    rx = x;
                    ry = y;

                    s12 = m_spectrum01[rx + ry * m_size];
                    s34 = m_spectrum23[rx + ry * m_size];

                    rx = (m_size - x) % m_size;
                    ry = (m_size - y) % m_size;

                    s12c = m_spectrum01[rx + ry * m_size];
                    s34c = m_spectrum23[rx + ry * m_size];

                    k1 = st * m_inverseGridSizes.x;
                    k2 = st * m_inverseGridSizes.y;
                    k3 = st * m_inverseGridSizes.z;
                    k4 = st * m_inverseGridSizes.w;

                    h1 = GetSpectrum(time, m_WTable[x + y * m_size].x, s12.x, s12.y, s12c.x, s12c.y);
                    h2 = GetSpectrum(time, m_WTable[x + y * m_size].y, s12.z, s12.w, s12c.z, s12c.w);
                    h3 = GetSpectrum(time, m_WTable[x + y * m_size].z, s34.x, s34.y, s34c.x, s34c.y);
                    h4 = GetSpectrum(time, m_WTable[x + y * m_size].w, s34.z, s34.w, s34c.z, s34c.w);

                    //heights
                    h12 = h1 + COMPLEX(h2);
                    //h34 = h3 + COMPLEX(h4);

                    //slopes (normals)
                    n1 = COMPLEX(k1.x * h1) - k1.y * h1;
                    n2 = COMPLEX(k2.x * h2) - k2.y * h2;
                    n3 = COMPLEX(k3.x * h3) - k3.y * h3;
                    n4 = COMPLEX(k4.x * h4) - k4.y * h4;

                    //Heights in last two channels (h34) have been removed as I found they arent really need for the shader
                    //h3 and h4 still needs to be calculated for the slope but they are no longer save and transformed by the fourier step
                    //m_fourierBuffer0[1, x+y*m_size] = new Vector4(h12.x, h12.y, h34.x, h34.y);

					int i = x + y * m_size;

                    m_fourierBuffer0[1, i] = h12;
                    m_fourierBuffer1[1, i] = new Vector4(n1.x, n1.y, n2.x, n2.y);
                    m_fourierBuffer2[1, i] = new Vector4(n3.x, n3.y, n4.x, n4.y);
                }
            }

        }

		/// <summary>
		/// Pack the data into result color
		/// arrays to then pass into map textures.
		/// Data has to be normalized to between
		/// 0 and 1 to go into a texture.
		/// </summary>
		void PackResults()
		{
			Vector4 map0, map1, map2;
			for (int x = 0; x < m_size; x++)
			{
				for (int y = 0; y < m_size; y++)
				{
					int i = x + y * m_size;
					
					map0 = m_fourierBuffer0[m_bufferIdx, i];
					map1 = m_fourierBuffer1[m_bufferIdx, i];
					map2 = m_fourierBuffer2[m_bufferIdx, i];
					
					m_result0[i].r = (map0.x + 1.0f) * 0.5f;
					m_result0[i].g = (map0.y + 1.0f) * 0.5f;
					m_result0[i].b = 0.0f;
					m_result0[i].a = 0.0f;
					
					m_result1[i].r = (map1.x + 1.0f) * 0.5f;
					m_result1[i].g = (map1.y + 1.0f) * 0.5f;
					m_result1[i].b = (map1.z + 1.0f) * 0.5f;
					m_result1[i].a = (map1.w + 1.0f) * 0.5f;
					
					m_result2[i].r = (map2.x + 1.0f) * 0.5f;
					m_result2[i].g = (map2.y + 1.0f) * 0.5f;
					m_result2[i].b = (map2.z + 1.0f) * 0.5f;
					m_result2[i].a = (map2.w + 1.0f) * 0.5f;
				}
			}
		}

		/// <summary>
		/// Runs a threaded task
		/// </summary>
		void RunThreaded(object o)
		{

			Nullable<float> time  = o as Nullable<float>;
			
			InitWaveSpectrum(time.Value);
			
			m_bufferIdx = m_fourier.PeformFFT(0, m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2);

			PackResults();

			done = true;
			
		}
		
		/// <summary>
		/// Calculate the wave heights at time period t.
		/// </summary>
		public void SimulateWaves(float t)
		{

			if(!done) return;

			done = false;

			Nullable<float> time = t;

			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded), time);

        }
		
		/// <summary>
		/// Writes the results into the textures.
		/// Will return if not done as data is still being generated.
		/// As Unity does not support floating point textures 
		/// (not counting render textures) the data has to be 
		/// packed into 8 bit channels
		/// This results in a lot of the data rounding down to 
		/// zero but its the only option at the moment. 
		/// </summary>
		public void WriteResults()
		{

			if(!done) return;

			m_map0.SetPixels(m_result0);
			m_map1.SetPixels(m_result1);
			m_map2.SetPixels(m_result2);
			
			m_map0.Apply();
			m_map1.Apply();
			m_map2.Apply();

		}
			
        /// <summary>
        /// This will return the ocean height at any world pos.
		/// Since the heights have been stored in a texture be
		/// aware that Unity will clamp the data to between 0 and 1.
		/// As long as you keep the wave settings to calm waves the
		/// height wont go above 1 often.
        /// </summary>
        public float SampleHeight(Vector3 worldPos)
        {

			float ht = 0.0f;

			Vector2 pos = new Vector2(worldPos.x / m_gridSizes.x, worldPos.z / m_gridSizes.x);

			ht += m_map0.GetPixelBilinear(pos.x, pos.y).r * 2.0f - 1.0f;

			pos = new Vector2(worldPos.x / m_gridSizes.y, worldPos.z / m_gridSizes.y);
			
			ht += m_map0.GetPixelBilinear( pos.x, pos.y).g * 2.0f - 1.0f;

			return ht;

        }

    }
}











