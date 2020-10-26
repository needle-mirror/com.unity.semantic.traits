namespace UnityEditor.Semantic.Traits.CodeGen
{
    /// <summary>
    /// Class that holds code-generation related data for a Trait property
    /// </summary>
    public class TraitPropertyDescriptorData
    {
        internal string AuthoringType;
        internal string RuntimeType;
        internal string DefaultValue;
        internal int MaxSize;

        internal TraitPropertyDescriptorData(string authoringType, string runtimeType, string defaultValue, int maxSize = 0)
        {
            AuthoringType = authoringType;
            RuntimeType = runtimeType;
            DefaultValue = defaultValue;
            MaxSize = maxSize;
        }
    }

    /// <summary>
    /// Base class used to provide custom descriptor for code-generation purpose
    /// </summary>
    /// <typeparam name="T">Trait Property Definition type</typeparam>
    public abstract class TraitPropertyDescriptor<T>
    {
        /// <summary>
        /// Get code-generation related data for a trait property
        /// </summary>
        /// <param name="property">Trait property</param>
        /// <returns>Trait property descriptor data</returns>
        public abstract TraitPropertyDescriptorData GetData(T property);
    }
}
