/*
using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using TaleWorlds.Library;

namespace Bannerlord.LauncherEx.Helpers
{
    public enum CheckResult
    {
        Success,
        TypeLoadException,
        ReflectionTypeLoadException,
        GenericException
    }

    public class Proxy : MarshalByRefObject
    {
        private static void RecursiveLoad(Assembly asm, string assemblyPath)
        {
            foreach (var referencedAssembly in asm.GetReferencedAssemblies())
            {
                var name = referencedAssembly.Name;

                if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == name))
                    continue;

                var assemblies = Directory.GetFiles(Path.GetDirectoryName(assemblyPath)!, "*.dll");
                var assembly = assemblies.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Equals(name));
                if (assembly is null)
                    continue;

                var refAsm = Assembly.LoadFrom(assembly);
                RecursiveLoad(refAsm, assemblyPath);
            }
        }

        public CheckResult CheckAssembly(string assemblyPath)
        {
            Assembly? CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
            {
                var name = args.Name.Contains(',') ? $"{args.Name.Split(',')[0]}.dll" : args.Name;

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var asmName = $"{asm.GetName().Name}.dll";
                    if (asmName == name)
                    {
                        return asm;
                    }
                }

                var assemblies = Directory.GetFiles(Path.GetDirectoryName(assemblyPath)!, "*.dll");
                var assembly = assemblies.FirstOrDefault(x => x.Contains(name));

                return assembly is not null ? Assembly.LoadFrom(assembly) : null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            try
            {
                var asm = Assembly.LoadFrom(assemblyPath);
                RecursiveLoad(asm, assemblyPath);
                _ = asm.GetTypes();
                return CheckResult.Success;
            }
            catch (TypeLoadException)
            {
                return CheckResult.TypeLoadException;
            }
            catch (ReflectionTypeLoadException)
            {
                return CheckResult.ReflectionTypeLoadException;
            }
            catch (Exception)
            {
                return CheckResult.GenericException;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
            }
        }
    }

    internal sealed class AssemblyCompatibilityChecker : IDisposable
    {
        private AppDomain? _domain;
        private Proxy? _proxy;
        private bool _initialized;

        private readonly Dictionary<string, CheckResult> _checkResult = new();

        public CheckResult CheckAssembly(string assemblyPath)
        {
            if (LauncherSettings.DisableBinaryCheck)
                return CheckResult.Success;

            if (!_initialized)
            {
                _initialized = true;

                _domain = AppDomain.CreateDomain("Compatibility Checker", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation, AppDomain.CurrentDomain.PermissionSet);
                _proxy = (Proxy) _domain.CreateInstanceAndUnwrap(typeof(Proxy).Assembly.FullName, typeof(Proxy).FullName);

                // Load official modules before cheking the mods
                var baseOfficialPath = Path.Combine(Path.GetDirectoryName(typeof(Common).Assembly.Location)!, "../", "../");
                var officialModulesDirectories = Directory.GetDirectories(Path.Combine(baseOfficialPath, "Modules")).Select(x => new DirectoryInfo(x).Name);
                var officialModules = officialModulesDirectories.Select(ModuleInfoHelper.LoadFromId).OfType<ModuleInfoExtendedWithMetadata>().Where(x => x.IsOfficial).ToList();
                var sortedModules = ModuleSorter.Sort(officialModules).OfType<ModuleInfoExtendedWithMetadata>();
                foreach (var module in sortedModules)
                {
                    var path = Path.Combine(module.Path, "bin", Common.ConfigName);
                    if (!Directory.Exists(path)) continue;

                    var assemblies = Directory.GetFiles(path, "*.dll");
                    foreach (var assembly in assemblies)
                    {
                        _checkResult[assembly] = _proxy.CheckAssembly(assembly);
                    }
                }
            }

            if (!_checkResult.TryGetValue(assemblyPath, out var result))
            {
                result = _proxy?.CheckAssembly(assemblyPath) ?? CheckResult.GenericException;
                _checkResult[assemblyPath] = result;
            }

            return result;
        }

        public void Dispose()
        {
            if (_domain is not null)
                AppDomain.Unload(_domain);
        }
    }
}
*/