namespace Example
{
    using System;

    public class ClassWithFieldToRemovedInNextMajor
    {
        [Obsolete("FieldToBeRemoved. Will be removed in version 2.0.0.", true)] public string FieldToBeRemoved;
    }
}