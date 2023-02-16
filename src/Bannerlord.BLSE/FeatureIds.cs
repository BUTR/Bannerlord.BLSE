using System.Collections.Generic;

namespace Bannerlord.BLSE
{
    public static class FeatureIds
    {
        public static string InterceptorId => "BLSE.LoadingInterceptor";
        public static string AssemblyResolverId => "BLSE.AssemblyResolver";
        private static string InterceptorId2 => "BUTRLoader.BUTRLoadingInterceptor";
        private static string AssemblyResolverId2 => "BUTRLoader.BUTRAssemblyResolver";
        public static string ContinueSaveFileId => "BLSE.ContinueSaveFile";
        public static string CommandsId => "BLSE.Commands";

        public static readonly HashSet<string> Features = new()
        {
            InterceptorId,
            InterceptorId2,
            AssemblyResolverId,
            AssemblyResolverId2,
            ContinueSaveFileId,
            CommandsId,
        };
        public static readonly HashSet<string> LauncherFeatures = new()
        {
            InterceptorId,
            InterceptorId2,
            AssemblyResolverId,
            AssemblyResolverId2,
        };
    }
}