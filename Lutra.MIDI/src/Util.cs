using Commons.Music.Midi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lutra.MIDI;

public static class Util
{
    public static List<IMidiPortDetails> GetMIDIInputDetails()
    {
        var accessManager = MidiAccessManager.Default;
        return accessManager.Inputs.ToList();
    }

    public static List<IMidiPortDetails> GetMIDIOutputDetails()
    {
        var accessManager = MidiAccessManager.Default;
        return accessManager.Outputs.ToList();
    }

    public static MIDICommandEvent BytesToCommandEvent(byte[] bytes, ref int previousChannel, ref MIDICommandEventType previousType)
    {
        MIDICommandEvent midiEvent = new MIDICommandEvent();

        midiEvent.RawBytes = bytes;

        // Determine event type.
        byte statusByte = bytes[0];
        int statusByteTop = (statusByte & 0b11110000) >> 4;
        midiEvent.Channel = statusByte & 0b00001111;
        if (bytes.Length < 3)
        {
            // No status byte sent - use previous type sent, and pick values from first two bytes.
            midiEvent.EventType = previousType;
            midiEvent.Value = bytes[0];
            midiEvent.Value2 = bytes[1];

            // Channel is unreliable here, so set it to the last known channel.
            midiEvent.Channel = previousChannel;

            if (midiEvent.EventType == MIDICommandEventType.ControllerValueChanged)
            {
                midiEvent.Controller = bytes[0];
                midiEvent.Value = bytes[1];
            }
        }
        else
        {
            previousChannel = midiEvent.Channel;

            if (statusByteTop == 0b1001)
            {
                midiEvent.EventType = MIDICommandEventType.NoteOn;
                midiEvent.Value = bytes[1];
                midiEvent.Value2 = bytes[2];

                // Note: Note-On events with a velocity of 0 are to be interpreted as Note-Off events.
                if (midiEvent.Value2 == 0)
                {
                    midiEvent.EventType = MIDICommandEventType.NoteOff;
                    previousType = midiEvent.EventType;
                }
                previousType = midiEvent.EventType;
            }
            if (statusByteTop == 0b1000)
            {
                midiEvent.EventType = MIDICommandEventType.NoteOff;
                midiEvent.Value = bytes[1];
                midiEvent.Value2 = 0;
                previousType = midiEvent.EventType;
            }
            if (statusByteTop == 0b1011)
            {
                midiEvent.EventType = MIDICommandEventType.ControllerValueChanged;
                midiEvent.Controller = bytes[1];
                midiEvent.Value = bytes[2];
                previousType = midiEvent.EventType;
            }
            if (statusByteTop == 0b1110)
            {
                midiEvent.EventType = MIDICommandEventType.PitchBend;
                midiEvent.Value = bytes[1];
                midiEvent.Value2 = bytes[2];
                previousType = midiEvent.EventType;
            }


        }

        return midiEvent;
    }

    public static byte[] CommandEventToBytes(MIDICommandEvent midiEvent)
    {
        byte[] bytes = new byte[3];
        byte statusByteBottom = (byte)midiEvent.Channel;
        byte statusByteTop = midiEvent.EventType switch
        {
            MIDICommandEventType.NoStatusSent => 0,
            MIDICommandEventType.NoteOn => 0b1001,
            MIDICommandEventType.NoteOff => 0b1000,
            MIDICommandEventType.ControllerValueChanged => 0b1011,
            MIDICommandEventType.PitchBend => 0b1110,
            _ => throw new NotImplementedException()
        };

        byte statusByte = (byte)(statusByteBottom | (statusByteTop << 4));

        bytes[0] = statusByte;
        if (midiEvent.EventType == MIDICommandEventType.NoStatusSent)
        {
            bytes[0] = (byte)midiEvent.Value; // Status byte is stomped when doing this. This is OK, and part of the MIDI spec.
            bytes[1] = (byte)midiEvent.Value2;
        }
        else if (midiEvent.EventType == MIDICommandEventType.ControllerValueChanged)
        {
            bytes[1] = (byte)midiEvent.Controller;
            bytes[2] = (byte)midiEvent.Value;
        }
        else
        {
            bytes[1] = (byte)midiEvent.Value;
            bytes[2] = (byte)midiEvent.Value2;
        }

        return bytes;
    }
}