using System;

namespace fi.Core;

[AttributeUsage(AttributeTargets.Class)]
public class InjectAppNameAttribute(params string[] names) : Attribute
{
    public string[] Tags { get; } = names;
}