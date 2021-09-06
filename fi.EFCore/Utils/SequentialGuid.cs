using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;

namespace fi.EFCore
{
    public class SequentialGuid
    {
        private static readonly Lazy<SequentialGuid> lazy = new(() => new SequentialGuid());
        public static SequentialGuid Current => lazy.Value;
        private readonly SequentialGuidValueGenerator _sequentialGuidValueGenerator;
        private SequentialGuid() => _sequentialGuidValueGenerator = new();
        public Guid Next(EntityEntry entry) => _sequentialGuidValueGenerator.Next(entry);
    }
}
