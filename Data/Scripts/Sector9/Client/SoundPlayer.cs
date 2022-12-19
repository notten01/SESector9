using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sector9.Core;
using Sector9.Core.Logging;
using System.Collections;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace Sector9.Client
{
    /// <summary>
    /// Class that can play sounds to the current sessions player
    /// </summary>
    public class SoundPlayer : ITickable
    {
        private readonly Queue SoundQueue;
        private int tickIndex = 2;
        private readonly MyEntity3DSoundEmitter QueuedSoundemitter;
        private readonly List<MyEntity3DSoundEmitter> LiveSoundEmitters;

        public SoundPlayer()
        {
            SoundQueue = new Queue();
            LiveSoundEmitters = new List<MyEntity3DSoundEmitter>();
            QueuedSoundemitter = new MyEntity3DSoundEmitter((MyEntity)MyAPIGateway.Session.LocalHumanPlayer.Controller.ControlledEntity);
            Logger.Log("Sound player initialized", Logger.Severity.Info, Logger.LogType.Player);
        }

        /// <summary>
        /// Plays the sound directly
        /// </summary>
        /// <param name="entity">Entity playing the sound</param>
        /// <param name="position">Location of the sound</param>
        /// <param name="soundName">Name of the sound (based on sbc id) to be played</param>
        /// <param name="volume">Volume of the sound</param>
        /// <param name="maxDistance">Maximum distance the sound can be heard from</param>
        public void PlaySound(IMyEntity entity, Vector3D position, string soundName, float volume = 1f, float maxDistance = 100f)
        {
            MyEntity3DSoundEmitter emitter = new MyEntity3DSoundEmitter((MyEntity)entity);
            LiveSoundEmitters.Add(emitter);
            Play(new SoundWrapper(soundName, position, volume, maxDistance), emitter);
        }

        /// <summary>
        /// Queue a sound to be played to the player, prevents overlapping sounds
        /// </summary>
        /// <param name="entity">Entity playing the sound</param>
        /// <param name="position">Location of the sound</param>
        /// <param name="soundName">Name of the sound (based on sbc id) to be played</param>
        /// <param name="volume">Volume of the sound</param>
        /// <param name="maxDistance">Maximum distance the sound can be heard from</param>
        public void PlaySoundInQueue(Vector3D position, string soundName, float volume = 1f, float maxDistance = 100f)
        {
            SoundQueue.Enqueue(new SoundWrapper(soundName, position, volume, maxDistance));
        }

        public void Tick()
        {
            if (tickIndex < 60)
            {
                tickIndex++;
                return;
            }
            tickIndex = 0;

            if (SoundQueue.Count > 0 && !QueuedSoundemitter.IsPlaying)
            {
                QueuedSoundemitter.Cleanup();
                Play((SoundWrapper)SoundQueue.Dequeue(), QueuedSoundemitter);
            }

            foreach (MyEntity3DSoundEmitter emitter in LiveSoundEmitters.ToArray())
            {
                if (!emitter.IsPlaying)
                {
                    LiveSoundEmitters.Remove(emitter);
                }
            }
        }

        private static void Play(SoundWrapper sound, MyEntity3DSoundEmitter emmiter)
        {
            MySoundPair soundPair = new MySoundPair(sound.SoundId);
            emmiter.CustomVolume = sound.Volume;
            emmiter.CustomMaxDistance = sound.MaxDistance;
            emmiter.PlaySound(soundPair, false, false, false, true);
        }

        private sealed class SoundWrapper
        {
            public SoundWrapper(string soundId, Vector3D location, float volume, float maxDistance)
            {
                SoundId = soundId;
                Location = location;
                Volume = volume;
                MaxDistance = maxDistance;
            }

            public Vector3D Location { get; }
            public float MaxDistance { get; }
            public string SoundId { get; }
            public float Volume { get; }
        }
    }
}