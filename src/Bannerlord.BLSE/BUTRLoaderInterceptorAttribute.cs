using System;

namespace Bannerlord.BLSE
{
    [Obsolete("Use BLSEInterceptorAttribute")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BUTRLoaderInterceptorAttribute : Attribute { }
}