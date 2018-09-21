using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace AuraEditor.Common
{
    static class StorageHelper
    {
        static public async Task<string> LoadFile(string filePath)
        {
            StorageFile sf = await StorageFile.GetFileFromPathAsync(filePath);
            return await LoadFile(sf);
        }
        static public async Task<string> LoadFile(StorageFile sf)
        {
            return await Windows.Storage.FileIO.ReadTextAsync(sf);
        }
        static public async Task SaveFile(string filePath, string content)
        {
            StorageFile sf = await StorageFile.GetFileFromPathAsync(filePath);
            await SaveFile(sf, content);
        }
        static public async Task SaveFile(StorageFile sf, string content)
        {
            // Prevent updates to the remote version of the file until
            // we finish making changes and call CompleteUpdatesAsync.
            Windows.Storage.CachedFileManager.DeferUpdates(sf);
            // write to file
            await Windows.Storage.FileIO.WriteTextAsync(sf, content);
            // Let Windows know that we're finished changing the file so
            // the other app can update the remote version of the file.
            // Completing updates may require Windows to ask for user input.
            Windows.Storage.Provider.FileUpdateStatus status =
                await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(sf);
        }

        static public async Task<StorageFolder> EnterOrCreateFolder(StorageFolder sf, string folderName)
        {
            IReadOnlyList<StorageFolder> folderList = await sf.GetFoldersAsync();

            foreach (StorageFolder folder in folderList)
            {
                if (folder.Name == folderName)
                    return folder;
            }

            return await sf.CreateFolderAsync(folderName);
        }
    }
}
