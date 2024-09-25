using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;
using Lutra;

namespace Lutra.MIDI;

public enum MIDICommandEventType
{
    NoStatusSent,
    NoteOn,
    NoteOff,
    ControllerValueChanged,
    PitchBend
}

/// <summary>
/// This represents an input or output event from or to a MIDI Controller.
/// </summary>
public struct MIDICommandEvent
{
    public int Channel;
    public int Controller;

    /// <summary> For a controller, this is the value of the control. For a note, this is the pitch. For a pitch-bend, this is the LSB of the bend value.</summary>
    public int Value;

    /// <summary> For a note, this is the velocity. For a pitch bend, this is the MSB of the bend value.</summary>
    public int Value2;
    public MIDICommandEventType EventType;
    public long Timestamp;
    public byte[] RawBytes;

}
