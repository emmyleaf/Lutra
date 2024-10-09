namespace Lutra.Utility.Debugging
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DebugCommand(string alias = "", string usage = "", string help = "", string group = "", bool buffered = false) : Attribute
    {
        public string Alias = alias;
        public string Usage = usage;
        public string Help = help;
        public string Group = group;
        public bool IsBuffered = buffered;
    }
}
