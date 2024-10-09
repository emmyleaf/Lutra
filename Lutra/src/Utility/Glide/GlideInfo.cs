using System.Reflection;

namespace Lutra.Utility.Glide
{
    internal class GlideInfo
    {
        private static readonly BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public string PropertyName { get; private set; }
        public Type PropertyType { get; private set; }

        private readonly FieldInfo fieldInfo;
        private readonly PropertyInfo prop;
        private readonly object Target;

        public object Value
        {
            get => fieldInfo != null ? fieldInfo.GetValue(Target) : prop.GetValue(Target, null);

            set
            {
                if (fieldInfo != null) fieldInfo.SetValue(Target, value);
                else prop.SetValue(Target, value, null);
            }
        }

        public GlideInfo(object target, PropertyInfo info)
        {
            Target = target;
            prop = info;
            PropertyName = info.Name;
            PropertyType = prop.PropertyType;
        }

        public GlideInfo(object target, FieldInfo info)
        {
            Target = target;
            fieldInfo = info;
            PropertyName = info.Name;
            PropertyType = info.FieldType;
        }

        public GlideInfo(object target, string property, bool writeRequired = true)
        {
            Target = target;
            PropertyName = property;

            var targetType = target as Type ?? target.GetType();

            if ((fieldInfo = targetType.GetField(property, flags)) != null)
            {
                PropertyType = fieldInfo.FieldType;
            }
            else if ((prop = targetType.GetProperty(property, flags)) != null)
            {
                PropertyType = prop.PropertyType;
            }
            else
            {
                //	Couldn't find either
                throw new Exception(string.Format("Field or {0} property '{1}' not found on object of type {2}.",
                    writeRequired ? "read/write" : "readable",
                    property, targetType.FullName));
            }
        }
    }
}
