using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace StellarisModInstaller
{
    internal class Program
    {
        private static readonly string ModPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive", "Stellaris", "mod");

        private static readonly string WorkingFolder = Directory.GetCurrentDirectory();

        private static readonly Regex Parser = new Regex(@"(archive="")[^""]*("")");

        private static void Main()
        {
            ClearModsFolder();
            CreateModDescriptors();
        }

        private static void CreateModDescriptors()
        {
            var modDirectories = Directory.GetDirectories(WorkingFolder);
            foreach (var directory in modDirectories.AsParallel())
            {
                var modId = Path.GetFileName(directory);
                var mod = Directory.GetFiles(directory, "*.zip").FirstOrDefault();
                if (mod == null)
                    continue;
                using (var archive = ZipFile.Read(mod))
                {
                    var descriptor = archive["descriptor.mod"];
                    descriptor.Extract(directory, ExtractExistingFileAction.OverwriteSilently);
                }

                var descriptorPath = Path.Combine(directory, "descriptor.mod");
                var savePath = Path.Combine(ModPath, $"ugc_{modId}.mod");

                UpdateArchivePath(descriptorPath, mod, savePath);
                AddAttribute(savePath, FileAttributes.ReadOnly);
            }
        }

        private static void ClearModsFolder()
        {
            foreach (var file in Directory.GetFiles(ModPath, "*.mod"))
            {
                RemoveAttribute(file, FileAttributes.ReadOnly);
                File.Delete(file);
            }
        }

        private static void UpdateArchivePath(string configPath, string modPath, string savePath)
        {
            var content = File.ReadAllText(configPath);
            var updated = Parser.Replace(content, $"$1{modPath}$2");
            File.WriteAllText(savePath, updated);
        }

        private static void RemoveAttribute(string file, FileAttributes attribute)
        {
            var attributes = File.GetAttributes(file);
            attributes &= ~attribute;
            File.SetAttributes(file, attributes);
        }

        private static void AddAttribute(string file, FileAttributes attribute)
        {
            var attributes = File.GetAttributes(file);
            attributes |= attribute;
            File.SetAttributes(file, attributes);
        }
    }
}
