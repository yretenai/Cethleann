using System;
using Cethleann.Structure;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Generic FS Interface
    /// </summary>
    public interface IManagedFS : IDisposable
    {
        /// <summary>
        ///     Maximum number of entries found in both containers and patches
        /// </summary>
        int EntryCount { get; }

        /// <summary>
        ///     Game ID of the game.
        /// </summary>
        DataGame GameId { get; }

        /// <summary>
        ///     Reads an entry from the first valid (non-zero) storage.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Memory<byte> ReadEntry(int index);

        /// <summary>
        ///     Loads a filelist from a csv file
        /// </summary>
        void LoadFileList(string filename = null);

        /// <summary>
        ///     Attempts to get a valid filename/filepath.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ext"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None);

        /// <summary>
        ///     Adds a container
        /// </summary>
        /// <param name="path"></param>
        void AddDataFS(string path);
    }
}
