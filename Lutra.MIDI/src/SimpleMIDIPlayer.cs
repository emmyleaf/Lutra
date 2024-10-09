using System;
using System.IO;
using System.Linq;
using Commons.Music.Midi;

namespace Lutra.MIDI;

/// <summary>
/// This is a super-simple MIDI player that can read MIDI files, play them back, and expose callbacks for the MIDI events that appear as it plays.
/// </summary>
public class SimpleMIDIPlayer
{
    // MIDI Backend
    private readonly MIDIOutput midiOutput;
    private SmfReader smfReader;
    private MidiPlayer midiPlayer;


    // API
    public void LoadFile(string path)
    {
        smfReader ??= new SmfReader();

        using (FileStream fs = File.Open(path, FileMode.Open))
        {
            smfReader.Read(fs);
        }

        midiPlayer = new MidiPlayer(smfReader.Music, midiOutput.RawDevice);
        midiPlayer.Finished += OnFinishedPlayingInternal;
        midiPlayer.EventReceived += OnMidiEventInternal;
    }


    public void Play()
    {
        if (midiPlayer == null)
        {
            Utility.Util.LogError("Error: Load a MIDI file before trying to Play, Pause, Stop, Seek, or Set/Get BPM.");
            return;
        }

        midiPlayer.Play();
    }

    public void Pause()
    {
        if (midiPlayer == null)
        {
            Utility.Util.LogError("Error: Load a MIDI file before trying to Play, Pause, Stop, Seek, or Set/Get BPM.");
            return;
        }

        midiPlayer.Pause();
    }
    public void Stop()
    {
        if (midiPlayer == null)
        {
            Utility.Util.LogError("Error: Load a MIDI file before trying to Play, Pause, Stop, Seek, or Set/Get BPM.");
            return;
        }

        midiPlayer.Stop();
    }

    public void Seek(int position)
    {
        if (midiPlayer == null)
        {
            Utility.Util.LogError("Error: Load a MIDI file before trying to Play, Pause, Stop, Seek, or Set/Get BPM.");
            return;
        }

        midiPlayer.Seek(position);
    }


    public void SetBPM(int bpm)
    {
        if (midiPlayer == null)
        {
            Utility.Util.LogError("Error: Load a MIDI file before trying to Play, Pause, Stop, Seek, or Set/Get BPM.");
            return;
        }

        midiPlayer.TempoChangeRatio = bpm / midiPlayer.Bpm;
    }

    public int GetBPM()
    {
        if (midiPlayer == null)
        {
            Utility.Util.LogError("Error: Load a MIDI file before trying to Play, Pause, Stop, Seek, or Set/Get BPM.");
            return -1;
        }

        return (int)(midiPlayer.TempoChangeRatio * midiPlayer.Bpm);
    }

    // Callbacks
    public Action<MIDICommandEvent> OnMidiEvent;
    public Action OnFinishedPlaying;

    private MIDICommandEventType previousEventType = MIDICommandEventType.NoteOn;
    private int previousChannel;

    public static SimpleMIDIPlayer SimpleMIDIPlayerOnDefaultMIDIDevice()
    {
        SimpleMIDIPlayer player = new(0);

        return player;
    }

    public SimpleMIDIPlayer(int outputDeviceIndex)
    {
        var accessManager = MidiAccessManager.Default;
        var availableOutputs = accessManager.Outputs.ToArray();
        var chosenOutput = availableOutputs[outputDeviceIndex];

        midiOutput = new MIDIOutput(chosenOutput.Id);
    }

    public SimpleMIDIPlayer(MIDIOutput outputDevice)
    {
        midiOutput = outputDevice;
    }

    private void OnFinishedPlayingInternal()
    {
        OnFinishedPlaying?.Invoke();
    }

    private void OnMidiEventInternal(MidiEvent midiEvent)
    {
        byte[] bytes = [midiEvent.StatusByte, midiEvent.Lsb, midiEvent.Msb];
        OnMidiEvent?.Invoke(Util.BytesToCommandEvent(bytes, ref previousChannel, ref previousEventType));
    }

}
