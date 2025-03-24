using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lkhsoft.Dring.Client.Services.Multimedia
{
    internal class NativeLibrariesImports : INativeLibrariesImports
    {
        /// <summary>
        /// Loaded Libraries
        /// </summary>
        private static readonly IDictionary<string, IntPtr> LoadedLibraries = new Dictionary<string, IntPtr>();

        /// <inheritdoc/>
        public void Load(string libraryName)
        {
            try
            {
                if (LoadedLibraries.ContainsKey(libraryName)) return;

                var libraryPointer = NativeLibrary.Load(libraryName, Assembly.GetCallingAssembly(), DllImportSearchPath.AssemblyDirectory);
                LoadedLibraries.Add(libraryName, libraryPointer);
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                    throw;

                throw new DllNotFoundException($"Failed to load native library '{libraryName}'", ex);
            }
        }

        /// <inheritdoc/>
        public IntPtr GetLibrary(string libraryName)
        {
            if (!LoadedLibraries.TryGetValue(libraryName, out var result))
                throw new InvalidOperationException($"Library {libraryName} is not loaded");
            return result;
        }
    }
}
