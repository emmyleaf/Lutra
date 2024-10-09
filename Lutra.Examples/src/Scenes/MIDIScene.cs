using ImGuiNET;
using Lutra.MIDI;
using Lutra.Utility;
using System.Collections.Generic;
using System.Linq;
using Commons.Music.Midi;
using Lutra.Graphics;

namespace Lutra.Examples
{
    public class MIDIScene : Scene
    {
        private MIDIInput MIDIInput;
        private MIDIOutput MIDIOutput;
        private SimpleMIDIPlayer SimpleMIDIPlayer;

        private List<string> MidiInputDetails;
        private List<string> MidiOutputDetails;
        private readonly Dictionary<int, int> DetectedValues = [];
        private Shape FunShape;
        private string LastEventPlayed = "";

        public override void Begin()
        {
            FunShape = Shape.CreateRectangle(128, 128, Color.Black);
            FunShape.X = 200;
            FunShape.Y = 200;
            FunShape.CenterOrigin();
            AddGraphic(FunShape);


            IMGUIDraw += DrawMyUI;

            OnEnd += () =>
            {
                MIDIInput?.Dispose();
            };
        }

        private void DrawMyUI()
        {
            ImGui.Begin("MIDI Input Controller");

            if (ImGui.Button("Get Available MIDI Inputs"))
            {
                MidiInputDetails = [];
                foreach (var midiInput in MIDI.Util.GetMIDIInputDetails())
                {
                    MidiInputDetails.Add(midiInput.Id + ":" + midiInput.Name);
                }
            }

            if (MidiInputDetails != null && MidiInputDetails.Count > 0)
            {
                ImGui.Text("MIDI Inputs: ");
                foreach (var midiInput in MidiInputDetails)
                {
                    string inputID = midiInput;
                    if (MIDIInput != null && MIDIInput.PortID == inputID.Split(":")[0])
                    {
                        ImGui.TextColored(Color.Green.ToVector4(), inputID + " - Connected");
                    }
                    else
                    {
                        ImGui.Text(inputID);
                        ImGui.SameLine();
                        if (ImGui.Button($"Connect to {inputID.Split(":")[0]}"))
                        {
                            MIDIInput = new MIDIInput(inputID.Split(":")[0]);
                            MIDIInput.OnInputEventReceivedCallback += OnInputReceived;
                        }
                    }

                }
            }

            if (DetectedValues.Count > 0)
            {
                ImGui.Text("Detected Input Values:");
                var keys = DetectedValues.Keys.ToList();
                keys.Sort();
                foreach (var key in keys)
                {
                    int val = DetectedValues[key];
                    ImGui.SliderInt($"Controller {key}", ref val, 0, 127);
                }
            }

            ImGui.End();

            ImGui.Begin("MIDI Output Controller");

            if (ImGui.Button("Get Available MIDI Outputs"))
            {
                MidiOutputDetails = [];
                foreach (var midiOutput in MIDI.Util.GetMIDIOutputDetails())
                {
                    MidiOutputDetails.Add(midiOutput.Id + ":" + midiOutput.Name);
                }
            }

            if (MidiOutputDetails != null && MidiOutputDetails.Count > 0)
            {
                ImGui.Text("MIDI Outputs: ");
                foreach (var midiOutput in MidiOutputDetails)
                {
                    string outputID = midiOutput;
                    if (MIDIOutput != null && MIDIOutput.PortID == outputID.Split(":")[0])
                    {
                        ImGui.TextColored(Color.Green.ToVector4(), outputID + " - Connected");
                    }
                    else
                    {
                        ImGui.Text(outputID);
                        ImGui.SameLine();
                        if (ImGui.Button($"Connect to {outputID.Split(":")[0]}"))
                        {
                            MIDIOutput = new MIDIOutput(outputID.Split(":")[0]);
                        }
                    }

                }
            }

            if (MIDIOutput != null && MIDIOutput.ConnectionState != MidiPortConnectionState.Closed)
            {
                ImGui.Text("Test Output Events: ");
                if (ImGui.Button("Note ON C-4"))
                {
                    MIDIOutput.SendNoteOn(0, 60, 127);
                }
                if (ImGui.Button("Note OFF C-4"))
                {
                    MIDIOutput.SendNoteOff(0, 60);
                }
            }


            ImGui.End();

            ImGui.Begin("Simple MIDI Player");
            if (SimpleMIDIPlayer == null)
            {
                if (ImGui.Button("Load Player"))
                {
                    SimpleMIDIPlayer = SimpleMIDIPlayer.SimpleMIDIPlayerOnDefaultMIDIDevice();
                    SimpleMIDIPlayer.OnMidiEvent += OnMidiPlayerEvent;
                    SimpleMIDIPlayer.LoadFile(AssetManager.AssetPath + "example.mid");
                }
            }
            else
            {
                if (ImGui.Button(">"))
                {
                    SimpleMIDIPlayer.Play();
                }
                ImGui.SameLine();
                if (ImGui.Button("||"))
                {
                    SimpleMIDIPlayer.Pause();
                }
                ImGui.SameLine();
                if (ImGui.Button("[]"))
                {
                    SimpleMIDIPlayer.Stop();
                }
                ImGui.Text($"BPM: {SimpleMIDIPlayer.GetBPM()}");
                ImGui.Text($"Last Event: {LastEventPlayed}");
            }

            ImGui.End();
        }

        private void OnMidiPlayerEvent(MIDICommandEvent midiEvent)
        {
            if (midiEvent.EventType == MIDICommandEventType.NoteOn || midiEvent.EventType == MIDICommandEventType.PitchBend)
            {
                if (midiEvent.Channel == 0)
                {
                    FunShape.ScaleY += 1;
                }
                if (midiEvent.Channel == 9)
                {
                    FunShape.ScaleX += 0.5f;
                }

                LastEventPlayed = $"{midiEvent.EventType}: {midiEvent.Channel}|{midiEvent.Value}, {midiEvent.Value2}";
            }
        }


        public override void Update()
        {
            base.Update();

            MIDIInput?.Update();
            MIDIOutput?.Update();

            FunShape.ScaleY = MathHelper.Lerp(FunShape.ScaleY, 1.0f, 0.25f);
            FunShape.ScaleX = MathHelper.Lerp(FunShape.ScaleX, 1.0f, 0.25f);
        }

        public void OnInputReceived(MIDICommandEvent midiEvent)
        {
            Utility.Util.Log($"MIDI Event: Type: {midiEvent.EventType}, Channel: {midiEvent.Channel}, Controller: {midiEvent.Controller}, Value: {midiEvent.Value}");
            if (midiEvent.EventType == MIDICommandEventType.ControllerValueChanged)
            {
                DetectedValues[midiEvent.Controller] = midiEvent.Value;

                if (midiEvent.Controller == 3)
                {
                    FunShape.X = 200 + (midiEvent.Value - 64);
                }
                if (midiEvent.Controller == 4)
                {
                    FunShape.Y = 200 + (midiEvent.Value - 64);
                }
                if (midiEvent.Controller == 5)
                {
                    FunShape.ScaleX = 0.01f + ((midiEvent.Value / 127.0f) * 2.0f);
                }
                if (midiEvent.Controller == 6)
                {
                    FunShape.ScaleY = 0.01f + ((midiEvent.Value / 127.0f) * 2.0f);
                }
                if (midiEvent.Controller == 7)
                {
                    FunShape.Color = FunShape.Color.WithRed(midiEvent.Value / 127.0f);
                }
                if (midiEvent.Controller == 8)
                {
                    FunShape.Color = FunShape.Color.WithGreen(midiEvent.Value / 127.0f);
                }
                if (midiEvent.Controller == 9)
                {
                    FunShape.Color = FunShape.Color.WithBlue(midiEvent.Value / 127.0f);
                }
                if (midiEvent.Controller == 10)
                {
                    FunShape.Color = FunShape.Color.WithAlpha(midiEvent.Value / 127.0f);
                }
            }
        }
    }
}
