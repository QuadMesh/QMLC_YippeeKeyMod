using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YippeeKey.ConfigSync;

namespace YippeeKey.LocalScripts
{

    /// <summary>
    /// Locally manage playing the yippe, disconnected from the network since the FX are local, the rest is triggered on the server.
    /// </summary>
    internal class YippeeSoundManager : MonoBehaviour
    {

        public AudioSource YippeAudio { get; private set; }

        public ParticleSystem[] particleSystems {get; private set; } = new ParticleSystem[3];

        public void Play()
        {
            // ---- AUDIO ----
            //If the yippeeAudio does not exit, get them.
            if (YippeAudio == null) { YippeAudio = GetComponent<AudioSource>(); }
            //Set the volume of the AudioSource to that of which is saved in the config file.
            YippeAudio.volume = YippeeSyncedConfig.Default.YippeeVolume.Value;
            //Stop the audio
            YippeAudio.Stop();
            //Play the Audio
            YippeAudio.Play();
            //Send the audio over the walkie-talkie
            WalkieTalkie.TransmitOneShotAudio(YippeAudio, YippeAudio.clip);
            
            //If we don't want visuals, don't execute this part.
            if (!YippeeSyncedConfig.Default.AllowVisuals.Value) return;
            
            // ---- VISUALS ----
            // Get particlesystems if the particles have not been retrieved yet.
            if (particleSystems[0] == null) getParticleSystems();

            //For each particlesystem in the array of particlesystems, Stop then play.
            foreach (ParticleSystem particles in particleSystems) {
                if (YippeeSyncedConfig.Default.AllowParticeSpam.Value) particles.Stop(false);
                else particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.Play();
            }
        }

        /// <summary>
        /// Gets the particle systems for the particles of 'Yippee!'
        /// </summary>
        private void getParticleSystems()
        {
            int count = 0;
            foreach (Transform child in transform)
            {
                particleSystems[count] = child.GetComponent<ParticleSystem>();
                count++;
            }
        }

    }
}
