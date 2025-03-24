using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lkhsoft.Dring.Client.Services.Multimedia
{
    /// <summary>
    /// Native libraries import definition
    /// </summary>
    public interface INativeLibrariesImports
    {
        /// <summary>
        /// Loads multimedia_stream native library
        /// </summary>
        void Load(string libraryName);

        /// <summary>
        /// Get a library pointer
        /// </summary>
        /// <param name="libraryName">Library name</param>
        /// <returns>Loaded library pointer</returns>
        IntPtr GetLibrary(string libraryName);
    }
}
