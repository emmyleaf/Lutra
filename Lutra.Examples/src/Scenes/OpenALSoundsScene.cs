using System.Diagnostics;
using ImGuiNET;
using Lutra.Audio.OpenAL;
using Lutra.Utility;

namespace Lutra.Examples
{
    public class OpenALSoundsScene : Scene
    {
        private Sound ChordStereo16;
        private Sound ChordStereo8;
        private Sound ChordMono16;
        private Sound ChordMono8;
        private Music Music;
        private Stopwatch MusicStopwatch;
        private float musicPitch = 1f;

        public override void Begin()
        {
            ChordStereo16 = new Sound(WavFile.FromStream(AssetManager.LoadStream("chord_stereo16.wav")));
            ChordStereo8 = new Sound(WavFile.FromStream(AssetManager.LoadStream("chord_stereo8.wav")));
            ChordMono16 = new Sound(WavFile.FromStream(AssetManager.LoadStream("chord_mono16.wav")));
            ChordMono8 = new Sound(WavFile.FromStream(AssetManager.LoadStream("chord_mono8.wav")));
            Music = new Music(AssetManager.LoadStream("LType/engramloopA.ogg"));
            MusicStopwatch = new Stopwatch();

            IMGUIDraw += DrawMyUI;

            OnEnd += () =>
            {
                ChordStereo16.Dispose();
                ChordStereo8.Dispose();
                ChordMono16.Dispose();
                ChordMono8.Dispose();
                Music.Dispose();
            };
        }

        private void DrawMyUI()
        {
            ImGui.Begin("OpenAL Sounds!");

            if (ImGui.Button("Play Stereo16 Sound"))
            {
                ChordStereo16.Play();
            }

            if (ImGui.Button("Play Stereo8 Sound"))
            {
                ChordStereo8.Play();
            }

            if (ImGui.Button("Play Mono16 Sound"))
            {
                ChordMono16.Play();
            }

            if (ImGui.Button("Play Mono8 Sound"))
            {
                ChordMono8.Play();
            }

            if (ImGui.Button("Play Music"))
            {
                MusicStopwatch.Start();
                Music.Play();
            }

            if (ImGui.Button("Pause Music"))
            {
                MusicStopwatch.Stop();
                Music.Pause();
            }

            if (ImGui.Button("Stop Music"))
            {
                MusicStopwatch.Reset();
                Music.Stop();
            }

            ImGui.Text(MusicStopwatch.Elapsed.ToString());

            if (ImGui.SliderFloat("Pitch", ref musicPitch, 0.1f, 5f))
            {
                Music.Pitch = musicPitch;
            }

            ImGui.End();
        }
    }
}
