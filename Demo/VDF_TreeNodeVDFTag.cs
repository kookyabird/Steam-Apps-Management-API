using System;
using Indieteur.VDFAPI;

namespace Demo
{
    class TreeNodeVdfTag
    {
        public enum Type
        {
            Node,
            Key
        }
        public Type TagType { get; private set; }
        public BaseToken Token { get; private set; }
        public TreeNodeVdfTag(Type tagType, BaseToken token)
        {
            TagType = tagType;
            Token = token ?? throw new NullReferenceException("Object property of TreeNodeVDFTag cannot be null!");
        }

        
    }
}
