﻿namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    public class ExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly AppDomain appDomain;
        readonly string previousWorkingDirectory;

        public ExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);
            appDomain = CreateAppDomain(assemblyFullPath);

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);
        }

        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(Options options)
        {
            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.DiscoverTestMethodGroups(assemblyFullPath, options);
        }

        public AssemblyResult RunAssembly(Options options)
        {
            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.RunAssembly(assemblyFullPath, options);
        }

        public AssemblyResult RunAssembly<TListener>(Options options, TListener listener)
            where TListener : LongLivedMarshalByRefObject, Listener
        {
            using (var executionProxy = Create<ExecutionProxy>())
            using (var bus = new Bus(listener))
                return executionProxy.RunAssembly(assemblyFullPath, options, bus);
        }

        public AssemblyResult RunMethods(Options options, Listener listener, MethodGroup[] methodGroups)
        {
            using (var executionProxy = Create<ExecutionProxy>())
            using (var bus = new Bus(listener))
                return executionProxy.RunMethods(assemblyFullPath, options, bus, methodGroups);
        }

        T Create<T>() where T : LongLivedMarshalByRefObject, new()
        {
            return (T)appDomain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName, false, 0, null, null, null, null);
        }

        public void Dispose()
        {
            AppDomain.Unload(appDomain);
            Directory.SetCurrentDirectory(previousWorkingDirectory);
        }

        static AppDomain CreateAppDomain(string assemblyFullPath)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(assemblyFullPath),
                ApplicationName = Guid.NewGuid().ToString(),
                ConfigurationFile = GetOptionalConfigFullPath(assemblyFullPath)
            };

            return AppDomain.CreateDomain(setup.ApplicationName, null, setup, new PermissionSet(PermissionState.Unrestricted));
        }

        static string GetOptionalConfigFullPath(string assemblyFullPath)
        {
            var configFullPath = assemblyFullPath + ".config";

            return File.Exists(configFullPath) ? configFullPath : null;
        }
    }
}