using System;

namespace fi.Core.AbstractUtility;

public abstract class LazySingleton<T> where T : LazySingleton<T>
{
    private static readonly Lazy<T> Lazy = new (()=> (Activator.CreateInstance(typeof(T), true) as T), true);
    public static T Instance => Lazy.Value;
}