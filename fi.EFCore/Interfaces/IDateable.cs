﻿using System;

namespace fi.EFCore
{
    public interface IDateable : IInterceptor
    {
        DateTime AuditCreateDate { get; set; }
        DateTime AuditModifiedDate { get; set; }
    }

}
