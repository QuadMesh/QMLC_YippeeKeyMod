using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YippeeKey.Patches;

//pulled a sneaky on ya :]
using Config = YippeeKey.ConfigSync.YippeeSyncedConfig;

namespace YippeeKey.LocalScripts
{

    /// <summary>
    /// Locally manage playing the yippee, disconnected from the network since the FX are local, the rest is triggered on the server.
    /// </summary>
    public sealed class YippeeSoundManager : MonoBehaviour
    {
        public AudioSource YippeeAudio { get; private set; } = null;

        public ParticleSystem[] ParticleSystems {get; private set; } = new ParticleSystem[3];

        //playercontroller property, used for the public field below.
        private PlayerControllerB player;

        //playercontroller field, used for the private property above.
        public PlayerControllerB Player { set
            {
                player = value;
                DeadBody = player.deadBody;
            }
            get { return player; }
        }

        //Deadbody reference for effects.
        public DeadBodyInfo DeadBody;

        //Reference to the GameNetworkManager's local playercontroller
        private bool IsLocalPlayerDead => GameNetworkManager.Instance.localPlayerController.isPlayerDead;

        public void Play(bool isCallerDead)
        {
            // ---- AUDIO ----
            //If the yippeeAudio does not exist, get them.
            if (YippeeAudio == null) { YippeeAudio = GetComponent<AudioSource>(); }
            //Reset previous effects, clean slate.
            ResetEffects();
            //Set the pitch
            RandomPitch();
            //Set up the post mortem effects if dead.
            if (isCallerDead) PostMortem();
            //Play
            PlayYippee();
            //Send the audio over the walkie-talkie
            WalkieTalkie.TransmitOneShotAudio(YippeeAudio, YippeeAudio.clip);

            // ---- VISUALS ----
            //If we don't want visuals or the caller is dead and we are alive, don't execute this part.
            if (!Config.Default.AllowVisuals.Value || (isCallerDead && !IsLocalPlayerDead)) return;
            // Get particlesystems if the particles have not been retrieved yet.
            if (ParticleSystems[0] == null) GetParticleSystems();

            //For each particlesystem in the array of particlesystems, Stop then play.
            foreach (ParticleSystem particles in ParticleSystems) {
                if (Config.Default.AllowParticeSpam.Value) particles.Stop(false);
                else particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.Play();
            }
        }

        //Destroy entry of the dictionairy, then add a brand new soundManager to the playerController.
        public void OnDestroy()
        {
            NetworkObjectManagerYK.soundManagers.Remove(Player.gameObject.name);
            PlayerControllerBPatch.AddSoundManagerToPlayer(Player);
        }
        
        //Play yippee!!
        private void PlayYippee()
        {
            if (Config.Default.AllowYippeeOverlap.Value)
            {
                //If allowing overlap, just play oneshot.
                YippeeAudio.PlayOneShot(YippeeAudio.clip);
            }
            else
            {
                //Else, do the pre-1.3.0 routine.
                //Stop the audio
                YippeeAudio.Stop();
                //Play the Audio
                YippeeAudio.Play();
            }
            YippeeKeyPlugin.Instance.Log($"Playing Yippee {YippeeKeyPlugin.BeautifyBool(YippeeAudio.enabled)}");
        }

        /// <summary>
        /// Gets the particle systems for the particles of 'Yippee!'
        /// </summary>
        private void GetParticleSystems()
        {
            //Possibly change into a forI loop?
            int count = 0;
            foreach (Transform child in transform)
            {
                ParticleSystems[count] = child.GetComponent<ParticleSystem>();
                count++;
            }
        }

        /// <summary>
        /// Sets the random pitch when the config entry is set to true.
        /// </summary>
        private void RandomPitch()
        {
            //We'll set the pitch randomly if the config is set
            if (Config.Default.RandomPitchedYippee.Value)
                YippeeAudio.pitch = UnityEngine.Random.Range(.95f, 1.05f);
        }

        /// <summary>
        /// Sets the sound up to be played for dead players.
        /// </summary>
        private void PostMortem()
        {
            if (IsLocalPlayerDead)
            {
                //Disable spatial blend
                YippeeAudio.spatialBlend = 0;
                //Bypass reverb
                YippeeAudio.reverbZoneMix = 0;
            }
            else
            {
                //We want the audio to be at least 25% that of the original
                YippeeAudio.volume = YippeeAudio.volume * .25f;
                //We want the pitch to be low, so that it feels dead.
                YippeeAudio.pitch = YippeeAudio.pitch * .75f;
                
            }
        }

        /// <summary>
        /// Reset the effects, a clean slate to add new effects depending on the situation.
        /// </summary>
        private void ResetEffects()
        {
            //Reset pitch
            YippeeAudio.pitch = 1;
            //Reset volume
            YippeeAudio.volume = Config.Default.YippeeVolume.Value;
            //reset spatial blend
            YippeeAudio.spatialBlend = 1;
            //Reset reverb bypass
            YippeeAudio.reverbZoneMix = 1;
        }
    }
}
