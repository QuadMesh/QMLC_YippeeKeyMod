using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YippeeKey.LocalScripts
{

    /// <summary>
    /// Locally manage playing the yippe, disconnected from the network since the FX are local, the rest is triggered on the server.
    /// </summary>
    internal class YippeeSoundManager : MonoBehaviour
    {

        public AudioSource YippeAudio { get; private set; }

        public ParticleSystem[] particleSystems {get; private set; } = new ParticleSystem[3];

        private void Awake()
        {
            YippeAudio = GetComponent<AudioSource>();
            int count = 0;
            foreach (Transform child in transform)
            {
                particleSystems[count] = child.GetComponent<ParticleSystem>();
                count++;
            }
        }

        public void Play()
        {
            YippeAudio.Stop();
            YippeAudio.Play();

            //If we don't want visuals, don't execute this part.
            if (!YippeeKeyPlugin.Instance.AllowVisuals.Value) return;

            foreach (ParticleSystem particles in particleSystems) {
                particles.Stop();
                particles.Play();
            }
        }
    }
}
