using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;
using Lutra;

namespace Lutra.MIDI;

/// <summary>
/// This class represents a physical MIDI Controller device. 
/// </summary>
public class MIDIInput : IDisposable
{
    // Private Fields
    private IMidiAccess accessManager;
    private IMidiInput midiInput;
    private Queue<Byte[]> midiEventQueue = new Queue<Byte[]>();
    private Queue<string> decodingMessageQueue = new Queue<string>();
    private MIDICommandEventType previousType;
    private int previousChannel;


    public MidiPortConnectionState ConnectionState => midiInput != null ? midiInput.Connection : MidiPortConnectionState.Closed;
    public String PortID => midiInput.Details.Id;
    public String Name => midiInput.Details.Name;
    public String Manufacturer => midiInput.Details.Manufacturer;
    public Action<MIDICommandEvent> OnInputEventReceivedCallback;


    public MIDIInput(string portID = null)
    {
        accessManager = MidiAccessManager.Default;
        var availableInputs = accessManager.Inputs.ToArray();
        bool connectSuccess = false;
        if (availableInputs == null || availableInputs.Length < 1)
        {
            Utility.Util.LogError("Cannot open MIDI Input. No devices available.");
        }

        Utility.Util.Log($"Attempting to connect to MIDI Input {(portID == null ? availableInputs.Last().Id : portID)}");
        midiInput = accessManager.OpenInputAsync(portID == null ? availableInputs.Last().Id : portID).Result;

        if (midiInput != null)
        {
            Utility.Util.LogInfo($"Connected to MIDI Input {midiInput.Details.Id}. {midiInput.Details.Name}, {midiInput.Details.Manufacturer}.");
            connectSuccess = true;
        }

        if (!connectSuccess)
        {
            Utility.Util.LogError($"Failed to connect to MIDI Input {(portID == null ? availableInputs.Last().Id : portID)}.");
        }
        else
        {
            midiInput.MessageReceived += OnInputReceivedInternal;
        }
    }

    public virtual void OnInputEventReceived(MIDICommandEvent midiEvent)
    {
        OnInputEventReceivedCallback?.Invoke(midiEvent);
    }

    private void OnInputReceivedInternal(object sender, MidiReceivedEventArgs args)
    {
        try
        {

            Byte[] midiEvent = new byte[args.Length];
            midiEvent[0] = args.Data[args.Start];
            midiEvent[1] = args.Data[args.Start + 1];
            if (args.Length > 2)
            {
                midiEvent[2] = args.Data[args.Start + 2];
            }
            midiEventQueue.Enqueue(midiEvent);
        }
        catch (Exception e)
        {
            decodingMessageQueue.Enqueue($"Error while decoding MIDI data: {e.Message}");
        }
    }

    public void Update()
    {
        while (midiEventQueue != null && midiEventQueue.TryDequeue(out var midiEvent))
        {
            var inputEvent = Util.BytesToCommandEvent(midiEvent, ref previousChannel, ref previousType);

            OnInputEventReceived(inputEvent);
        }

        while (decodingMessageQueue != null && decodingMessageQueue.TryDequeue(out var errorMsg))
        {
            Utility.Util.LogError(errorMsg);
        }
    }

    public void Dispose()
    {
        if (ConnectionState != MidiPortConnectionState.Closed)
        {
            midiInput.Dispose();
        }
    }
}
