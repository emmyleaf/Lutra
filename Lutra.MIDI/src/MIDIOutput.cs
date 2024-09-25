using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;
using Lutra;
using Lutra.Utility.Collections;

namespace Lutra.MIDI;

/// <summary>
/// This class represents a MIDI Output device, physical or virutal, that can receive events.
/// </summary>
public class MIDIOutput : IDisposable
{
    // Private Fields
    private IMidiAccess accessManager;
    private IMidiOutput midiOutput;
    private Queue<MIDICommandEvent> outgoingCommandQueue = new Queue<MIDICommandEvent>();



    public MidiPortConnectionState ConnectionState => midiOutput != null ? midiOutput.Connection : MidiPortConnectionState.Closed;
    public String PortID => midiOutput.Details.Id;
    public String Name => midiOutput.Details.Name;
    public String Manufacturer => midiOutput.Details.Manufacturer;
    public int Timestamp { get; private set; }
    public IMidiOutput RawDevice => midiOutput;


    public MIDIOutput(string portID = null)
    {
        accessManager = MidiAccessManager.Default;
        var availableOutputs = accessManager.Outputs.ToArray();
        bool connectSuccess = false;
        if (availableOutputs == null || availableOutputs.Length < 1)
        {
            Utility.Util.LogError("Cannot open MIDI Output. No devices available.");
        }

        Utility.Util.Log($"Attempting to connect to MIDI Output {(portID == null ? availableOutputs.Last().Id : portID)}");
        midiOutput = accessManager.OpenOutputAsync(portID == null ? availableOutputs.Last().Id : portID).Result;

        if (midiOutput != null)
        {
            Utility.Util.LogInfo($"Connected to MIDI Output {midiOutput.Details.Id}. {midiOutput.Details.Name}, {midiOutput.Details.Manufacturer}.");
            connectSuccess = true;
        }

        if (!connectSuccess)
        {
            Utility.Util.LogError($"Failed to connect to MIDI Output {(portID == null ? availableOutputs.Last().Id : portID)}.");
        }

    }

    private void EncodeEventBytes(ref MIDICommandEvent outputEvent)
    {
        outputEvent.RawBytes = Util.CommandEventToBytes(outputEvent);
    }

    public void SendNoteOn(int channel, int note, int velocity)
    {
        MIDICommandEvent outputEvent = new MIDICommandEvent
        {
            EventType = MIDICommandEventType.NoteOn,
            Value = note,
            Value2 = velocity,
            Channel = channel
        };

        if (velocity == 0)
        {
            Utility.Util.Log("Warning: Sending a Note-On MIDI event with Velocity set to 0 will register as a Note-Off event in the receiving device.");
        }

        EncodeEventBytes(ref outputEvent);
        outgoingCommandQueue.Enqueue(outputEvent);
    }

    public void SendNoteOff(int channel, int note)
    {
        MIDICommandEvent outputEvent = new MIDICommandEvent
        {
            EventType = MIDICommandEventType.NoteOff,
            Value = note,
            Channel = channel
        };

        EncodeEventBytes(ref outputEvent);
        outgoingCommandQueue.Enqueue(outputEvent);
    }

    public void SendValueUpdateOnly(int valueA, int valueB)
    {
        MIDICommandEvent outputEvent = new MIDICommandEvent
        {
            EventType = MIDICommandEventType.NoStatusSent,
            Value = valueA,
            Value2 = valueB
        };

        EncodeEventBytes(ref outputEvent);
        outgoingCommandQueue.Enqueue(outputEvent);
    }

    public void SendValueUpdateOnlyWithController(int valueA, int controller)
    {
        MIDICommandEvent outputEvent = new MIDICommandEvent
        {
            EventType = MIDICommandEventType.NoStatusSent,
            Value = controller,
            Value2 = valueA
        };

        EncodeEventBytes(ref outputEvent);
        outgoingCommandQueue.Enqueue(outputEvent);
    }

    public void SendControllerValueChanged(int channel, int controller, int value)
    {
        MIDICommandEvent outputEvent = new MIDICommandEvent
        {
            EventType = MIDICommandEventType.ControllerValueChanged,
            Value = controller,
            Value2 = value,
            Channel = channel
        };

        EncodeEventBytes(ref outputEvent);
        outgoingCommandQueue.Enqueue(outputEvent);
    }

    public void SendPitchbend(int channel, int lsb, int msb)
    {
        MIDICommandEvent outputEvent = new MIDICommandEvent
        {
            EventType = MIDICommandEventType.PitchBend,
            Value = lsb,
            Value2 = msb,
            Channel = channel
        };

        EncodeEventBytes(ref outputEvent);
        outgoingCommandQueue.Enqueue(outputEvent);
    }

    public void ResetTimestamp()
    {
        Timestamp = 0;
    }

    public void ForceIncrementTimestamp()
    {
        Timestamp++;
    }

    public void Update()
    {
        // While output queue not empty, send events.
        while (outgoingCommandQueue != null && outgoingCommandQueue.TryDequeue(out var midiEvent))
        {
            midiOutput.Send(midiEvent.RawBytes, 0, midiEvent.RawBytes.Length, Timestamp++);
        }
    }

    public void Dispose()
    {
        if (ConnectionState != MidiPortConnectionState.Closed)
        {
            midiOutput.Dispose();
        }
    }
}
