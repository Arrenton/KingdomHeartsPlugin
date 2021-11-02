using System;
using System.IO;
using System.Threading;
using Dalamud.Utility;
using NAudio.Wave;

namespace KingdomHeartsPlugin.Utilities
{
    public static class SoundEngine
    {
        // Copied from PeepingTom plugin
        public static void PlaySound(string path, float volume = 1.0f)
        {
            if (path.IsNullOrEmpty() || !File.Exists(path)) return;

            var soundDevice = KingdomHeartsPlugin.Ui.Configuration.SoundDeviceId;
            if (soundDevice < -1 || soundDevice > WaveOut.DeviceCount)
            {
                soundDevice = -1;
            }

            new Thread(() => {
                WaveStream reader;
                try
                {
                    reader = new AudioFileReader(path);
                }
                catch (Exception e)
                {
                    Dalamud.Logging.PluginLog.Error($"Could not play sound file: {e.Message}");
                    return;
                }
                
                using WaveChannel32 channel = new(reader)
                {
                    Volume = volume,
                    PadWithZeroes = false,
                };

                using (reader)
                {
                    using var output = new WaveOutEvent
                    {
                        DeviceNumber = soundDevice,
                    };
                    output.Init(channel);
                    output.Play();

                    while (output.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(500);
                    }
                }
            }).Start();
        }
    }
}
