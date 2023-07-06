﻿using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// This class represents the file system of the application and provides methods for accessing the file system on the drive.
    /// </summary>
    internal class Drive
    {
        public static readonly string Folder = Windows.ApplicationModel.Package.Current.InstalledPath;

        #region File Paths

        internal static readonly string s_regdir =   $"{Folder}\\FileSystem\\Registry\\";

        internal static readonly string s_system =      $"{Folder}\\FileSystem\\System.dntf";
        internal static readonly string s_orders =      $"{Folder}\\FileSystem\\Orders\\";
        internal static readonly string s_registry =    $"{Folder}\\FileSystem\\Registry\\Registry.dntf";
        internal static readonly string s_exports =     $"{Folder}\\FileSystem\\Registry\\Exports.dntf";
        
        #endregion

        /// <summary>
        /// This method validates the file system by checking for the existence
        /// of the system file and creates the file system if it does not exist.
        /// </summary>
        public static void Validate()
        {
            if (File.Exists(s_system))
            {
                return;
            }

            Setup.CreateFileSystem();
        }
        
        /// <summary>
        /// This method loads the registry from the file system.
        /// </summary>
        public static void LoadRegistry()
        {
            Manager.OrdersRegistry.Clear();
            Manager.Users.Clear();
            Manager.Machines.Clear();
            Manager.Exports.Clear();
            Manager.Contents.Clear();
            
            string source = Read(s_registry);

            foreach (string order in source.Split('\n'))
            {
                if (order == string.Empty) { continue; }

                string[] parameters = order.Split('\t');
                Manager.OrdersRegistry.Add(parameters[0]);
                Manager.Users.Add(Int32.Parse(parameters[1]));
                Manager.Machines.Add(Int32.Parse(parameters[2]));
                Manager.Exports.Add(Int32.Parse(parameters[3]));
                Manager.Contents.Add(Int32.Parse(parameters[4]));

                if (Int32.Parse(parameters[3]) >= Int32.Parse(parameters[4])) { continue; }

                Export.Unexported.Add(parameters[0]);
            }
        }

        public static void SaveRegistry()
        {
            string output = "";
            for (int i = 0; i < Manager.OrdersRegistry.Count; i++)
            {
                output += $"{Manager.OrdersRegistry[i]}\t{Manager.Users[i]}\t{Manager.Machines[i]}\t{Manager.Exports[i]}\t{Manager.Contents[i]}\n";
            }
            output += $"SP{Export.AuftragSplitter}\t{Export.NetstalSplitter}";
            Write(s_registry, output);
        }

        public static List<Fault>[] LoadFaults(string orderNumber)
        {
            List<Fault>[] output = new List<Fault>[3];
            
            List<Fault> faults = new();
            List<Fault> previews = new();
            List<Fault> pending = new();

            string source = Read(CreateFaultsPath(orderNumber));

            string header = "Normal";

            foreach (string line in source.Split('\n'))
            {
                if (line == "Preview:") { header = "Preview"; continue; }
                if (line == "Pending:") { header = "Pending"; continue; }
                if (line == "") { continue; }
                string[] parts = line.Split('\t');

                Fault fault = new(parts[3]);

                fault.Index = int.Parse(parts[0]);
                fault.Component = parts[1];
                fault.Placement = parts[2];
                fault.Cause = parts[4];
                fault.Classification = parts[5];
                fault.Type = parts[6];
                fault.ClassIndexes[0] = int.Parse(parts[7]);
                fault.ClassIndexes[1] = int.Parse(parts[8]);
                fault.ClassIndexes[2] = int.Parse(parts[9]);

                if (header == "Normal") { faults.Add(fault); continue; }
                if (header == "Preview") { previews.Add(fault); continue; }
                
                pending.Add(fault);
            }

            output[0] = faults;
            output[1] = previews;
            output[2] = pending;

            return output;
        }

        public static void SaveFaults(string orderNumber, List<Fault> faults, List<Fault> previews, List<Fault> pending)
        {
            string output = "";

            foreach (Fault fault in faults)
            {
                output += $"{fault.Index}\t{fault.ToString()}\n";
            }
            output += "Preview:\n";
            foreach (Fault fault in previews)
            {
                output += $"{fault.Index}\t{fault.ToString()}\n";
            }
            output += "Pending:\n";
            foreach (Fault fault in pending)
            {
                output += $"{fault.Index}\t{fault.ToString()}\n";
            }

            Write(CreateFaultsPath(orderNumber), output);
        }

        internal static string CreateFaultsPath(string orderNumber)
        {
            return $"{s_orders}{orderNumber.Replace(" ", string.Empty)}.dnff";
        }

        /// <summary>
        /// This method writes to the specific file in the file system.
        /// </summary>
        /// <param name="file">The path to the file to save.</param>
        /// <param name="value">The value to save to the file.</param>
        public static void Write(string file, string value)
        {
            File.WriteAllText(file, value);
        }

        /// <summary>
        /// This method reads the specific file from the file system using the
        /// safe method of reading the file (if the file does not exist, it will
        /// be created).
        /// </summary>
        /// <param name="file">The path to the file to read.</param>
        public static string Read(string file)
        {
            Check(file);
            return File.ReadAllText(file);
        }

        /// <summary>
        /// This method checks if the file exists in the file system and if not,
        /// it creates the file.
        /// </summary>
        /// <param name="file">The path to the file to check.</param>
        public static void Check(string file)
        {
            if (!File.Exists(file))
            {
                File.Create(file);
            }
        }

        public static void LoadExportHistory()
        {
            string[] source = Read(s_exports).Split('\n');

            int[] datea = new int[3];

            datea[0] = Int32.Parse(source[0].Substring(0,2));
            datea[1] = Int32.Parse(source[0].Substring(2,2));
            datea[2] = Int32.Parse(source[0].Substring(4));

            DateTime date = new DateTime(datea[2], datea[1], datea[0]);

            int offset = (int)(DateTime.UtcNow - date).TotalDays;

            int x = 1;
            for (int i = Export.GraphicalCount - offset - 1; i >= 0; i--)
            {
                if (x >= source.Length) { continue; }

                if (source[x] == "\n") { x++; continue; }

                string[] parts = source[x].Split("\t");

                Export.UserService[i] = (float)Int32.Parse(parts[0]) / 60f;
                Export.MachService[i] = (float)Int32.Parse(parts[1]) / 60f;

                Export.UserExports[i] = (float)Int32.Parse(parts[2]) / 60f;
                Export.MachExports[i] = (float)Int32.Parse(parts[3]) / 60f;

                x++;
            }
        }

        public static async void DialogSave(string content, bool netstal = false)
        {
            string filetype = ".dnha";
            string filetext = "Auftrag File";

            if (netstal)
            {
                filetype = ".dnhn";
                filetext = "Netstal File";
            }
            
            FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(Manager.CurrentWindow);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            // Set options for your file picker
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add(filetext, new List<string>() { filetype });

            // Open the picker for the user to pick a file
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    using (var tw = new StreamWriter(stream))
                    {
                        tw.Write(content);
                    }
                }
            }
        }
    }
}
