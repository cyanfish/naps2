﻿using NAPS2.Config;
using NAPS2.Lang.Resources;
using System.IO;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class DialogHelper
    {
        private readonly IUserConfigManager userConfigManager;
        private readonly IFormFactory formFactory;

        public DialogHelper(IUserConfigManager userConfigManager, IFormFactory formFactory)
        {
            this.userConfigManager = userConfigManager;
            this.formFactory = formFactory;
        }

        public bool PromptToSavePdfOrImage(string defaultPath, out string savePath)
        {
            var sd = new SaveFileDialog
            {
                OverwritePrompt = false,
                AddExtension = true,
                Filter = MiscResources.FileTypePdf + "|*.pdf|" +
                         MiscResources.FileTypeBmp + "|*.bmp|" +
                         MiscResources.FileTypeEmf + "|*.emf|" +
                         MiscResources.FileTypeExif + "|*.exif|" +
                         MiscResources.FileTypeGif + "|*.gif|" +
                         MiscResources.FileTypeJpeg + "|*.jpg;*.jpeg|" +
                         MiscResources.FileTypePng + "|*.png|" +
                         MiscResources.FileTypeTiff + "|*.tiff;*.tif",
                FileName = Path.GetFileName(defaultPath),
                InitialDirectory = GetDir(defaultPath)
            };
            if (sd.ShowDialog() == DialogResult.OK)
            {
                savePath = sd.FileName;
                return true;
            }
            savePath = null;
            return false;
        }

        public bool PromptToSavePdf(string defaultPath, out string savePath)
        {
            var sd = new SaveFileDialog
            {
                OverwritePrompt = false,
                AddExtension = true,
                Filter = MiscResources.FileTypePdf + "|*.pdf",
                FileName = Path.GetFileName(defaultPath),
                InitialDirectory = GetDir(defaultPath)
            };
            if (sd.ShowDialog() == DialogResult.OK)
            {
                savePath = sd.FileName;
                return true;
            }
            savePath = null;
            return false;
        }

        public bool PromptToSaveImage(string defaultPath, out string savePath)
        {
            var sd = new SaveFileDialog
            {
                OverwritePrompt = false,
                AddExtension = true,
                Filter = MiscResources.FileTypeBmp + "|*.bmp|" +
                            MiscResources.FileTypeEmf + "|*.emf|" +
                            MiscResources.FileTypeExif + "|*.exif|" +
                            MiscResources.FileTypeGif + "|*.gif|" +
                            MiscResources.FileTypeJpeg + "|*.jpg;*.jpeg|" +
                            MiscResources.FileTypePng + "|*.png|" +
                            MiscResources.FileTypeTiff + "|*.tiff;*.tif",
                FileName = Path.GetFileName(defaultPath),
                InitialDirectory = GetDir(defaultPath)
            };
            switch ((userConfigManager.Config.LastImageExt ?? "").ToLowerInvariant())
            {
                case "bmp":
                    sd.FilterIndex = 1;
                    break;

                case "emf":
                    sd.FilterIndex = 2;
                    break;

                case "exif":
                    sd.FilterIndex = 3;
                    break;

                case "gif":
                    sd.FilterIndex = 4;
                    break;

                case "png":
                    sd.FilterIndex = 6;
                    break;

                case "tif":
                case "tiff":
                    sd.FilterIndex = 7;
                    break;

                default: // Jpeg
                    sd.FilterIndex = 5;
                    break;
            }
            if (sd.ShowDialog() == DialogResult.OK)
            {
                savePath = sd.FileName;
                userConfigManager.Config.LastImageExt = (Path.GetExtension(savePath) ?? "").Replace(".", "");
                userConfigManager.Save();
                return true;
            }
            savePath = null;
            return false;
        }

        private string GetDir(string defaultPath)
        {
            return Path.IsPathRooted(defaultPath)
                ? Path.GetDirectoryName(defaultPath)
                : "";
        }

        public void ShowErrorWithDetails(string errorMesage, string details)
        {
            var form = formFactory.Create<FError>();
            form.ErrorMessage = errorMesage;
            form.Details = details;
            form.ShowDialog();
        }
    }
}