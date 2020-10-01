﻿using Microsoft.Extensions.Configuration;
using MP.Core.Context;
using MP.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandbrakeCliWrapper;

namespace MP.Core
{
    public class Converter
    {
        private MPContext context;
        private readonly IConfiguration config;
        private MP.Core.Analysis analyzer;
        public Converter(MPContext ctx, IConfiguration cfg)
        {
            context = ctx;
            config = cfg;
            analyzer = new MP.Core.Analysis(context, config);
        }
        public async Task ProcessDB()
        {
            string targetFormat = config["MP:Conversion:ProfileName"];
            while (true)
            {
                MediaFile currentFile = context.MediaFiles
                    .Where(m => m.ProcessedFormat == targetFormat)
                    .OrderByDescending(m => m.BytesPerSecondPerPixel)
                    .First();
                await ProcessFile(currentFile);
            }
        }
        public async Task ProcessFile(MediaFile file)
        {
            string fullpath = Path.Join(file.FilePath, file.FileName);
            await ProcessFile(fullpath, file.ContentType);
        }
        public async Task ProcessFile(string filename, string content_type)
        {
            string tempPath = (config["MP:Conversion:TempDir"] ?? Path.GetTempPath());
            string fileExtension = (config["MP:Conversion:FileExtension"] ?? "mp4");

            FileInfo existingFileInfo = new FileInfo(filename);

            string tmpSourceName = "src-" + existingFileInfo.Name;
            string tmpSourceFullname = Path.Join(tempPath, tmpSourceName);

            string tmpDestName = "dest-" + Path.GetFileNameWithoutExtension(existingFileInfo.Name) + "." + fileExtension;
            string tmpDestFullname = Path.Join(tempPath, tmpDestName);

            string finalName = Path.GetFileNameWithoutExtension(existingFileInfo.Name) + "." + fileExtension;
            string finalFullname = Path.Join(existingFileInfo.DirectoryName, finalName);

            cleanupFiles(tmpSourceFullname, tmpDestFullname);

            // Copy file to local temp dir
            File.Copy(filename, tmpSourceFullname);

            // Run local file analysis
            FileInfo tmpSourceFileInfo = new FileInfo(tmpSourceFullname);
            MediaFile tmpSourceAnalysis = await analyzer.AnalyzeFile(content_type, tmpSourceFileInfo);

            // Transcode file locally
            Handbrake.Configuration hbConfig = getHandbrakeConfig();
            await transcodeFile(tmpSourceFullname, tempPath, tmpDestName, hbConfig);

            // Run local file analysis against temp file
            FileInfo tmpDestFileInfo = new FileInfo(tmpDestFullname);
            MediaFile tmpDestAnalysis = await analyzer.AnalyzeFile(content_type, tmpDestFileInfo);

            // Verify
            if (!verifyTranscodedFile(tmpSourceAnalysis, tmpDestAnalysis))
            {
                cleanupFiles(tmpSourceFullname, tmpDestFullname);
                Console.Out.WriteLine($"[Error] Verification failed for {tmpDestName}");
                return;
            }

            // Copy file back and verify
            if (! await copyWithMediaVerification(tmpDestFullname, tmpDestAnalysis, finalFullname))
            {
                Console.Out.WriteLine($"[Error] Could not copy \"{tmpDestName}\" to \"{finalFullname}\"");
                cleanupFiles(tmpDestFullname);
                return;
            }

            // Delete extra files leftover
            cleanupFiles(tmpDestFullname, tmpSourceFullname);
            if (finalFullname != filename)
            {
                cleanupFiles(filename);
            }

            // Cleanup any existing database records
            await cleanupDB(filename, finalFullname);

            // Process file back into the database
            await analyzer.ProcessFile(finalFullname, content_type, hbConfig.Profile);
        }
        private Handbrake.Configuration getHandbrakeConfig()
        {
            Handbrake.Configuration hbConfig = new Handbrake.Configuration();
            hbConfig.Profile = config["MP:Conversion:ProfileName"];
            hbConfig.PresetImportFile = config["MP:Conversion:HandBrakeProfilePath"];

            return hbConfig;
        }

        private async Task transcodeFile(string source, string destDir, string filename, Handbrake.Configuration hbConfig)
        {
            HandbrakeCliWrapper.Handbrake conv = new HandbrakeCliWrapper.Handbrake(config["MP:Conversion:HandBrakeCLIPath"]);
            await conv.Transcode(hbConfig, source, destDir, filename, true);
        }

        private async Task<bool> copyWithMediaVerification(string source, MediaFile sourceMedia, string dest)
        {
            string destPath = Path.GetDirectoryName(dest);
            string destName = Path.GetFileName(dest);
            string tmpName = "mp-" + destName;
            string tmpFullname = Path.Join(destPath, tmpName);
            try
            {
                cleanupFiles(tmpFullname);
                File.Copy(source, tmpFullname);
                FileInfo tmpFileInfo = new FileInfo(tmpFullname);
                MediaFile tmpAnalysis = await analyzer.AnalyzeFile(sourceMedia.ContentType, tmpFileInfo);
                bool verification = verifyTranscodedFile(sourceMedia, tmpAnalysis);
                if (verification)
                {
                    cleanupFiles(dest);
                    File.Move(tmpFullname, dest);
                }
                cleanupFiles(tmpFullname);
                await cleanupDB(tmpFullname);
                return verification;
            } catch
            {
                cleanupFiles(tmpFullname);
                return false;
            }
        }
        private bool verifyTranscodedFile(MediaFile source, MediaFile toVerify)
        {
            double difference = Math.Abs(source.Analysis.Format.Duration.TotalSeconds - toVerify.Analysis.Format.Duration.TotalSeconds);
            return (difference < 3 * 60);
        }
        private async Task cleanupDB(params string[] filenames)
        {
            foreach (string file in filenames)
            {
                string dirPath = Path.GetDirectoryName(file);
                string filename = Path.GetFileName(file);
                var matches = context.MediaFiles
                    .Where(m => m.FilePath == dirPath)
                    .Where(m => m.FileName == filename)
                    .ToList();
                var errorMatches = context.FilesWithErrors
                    .Where(f => f.FilePath == file)
                    .ToList();
                context.RemoveRange(matches);
                context.RemoveRange(errorMatches);
            }
            await context.SaveChangesAsync();
        }

        private void cleanupFiles(params string[] filenames)
        {
            Console.Out.WriteLine("=== cleanupFiles ===");
            foreach (string file in filenames)
            {
                Console.Out.WriteLine(file);
                File.Delete(file);
            }
        }
    }
}
