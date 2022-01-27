namespace Lutra.Utility.Debugging
{
    public class DebugCommand : Attribute
    {
        public string Alias;
        public string Usage;
        public string Help;
        public string Group;
        public bool IsBuffered;

        public DebugCommand(string alias = "", string usage = "", string help = "", string group = "", bool buffered = false)
        {
            Alias = alias;
            Usage = usage;
            Help = help;
            Group = group;
            IsBuffered = buffered;
        }
    }
}
