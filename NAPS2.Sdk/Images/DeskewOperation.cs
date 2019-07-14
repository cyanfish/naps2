using System;
using System.Collections.Generic;
using System.Threading;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Util;

namespace NAPS2.Images
{
    public class DeskewOperation : OperationBase
    {
        private readonly ImageContext imageContext;
        private readonly ImageRenderer imageRenderer;
        private readonly Deskewer deskewer;

        public DeskewOperation() : this(ImageContext.Default, new ImageRenderer(ImageContext.Default), new HoughLineDeskewer())
        {
        }

        public DeskewOperation(ImageContext imageContext, ImageRenderer imageRenderer, Deskewer deskewer)
        {
            this.imageContext = imageContext;
            this.imageRenderer = imageRenderer;
            this.deskewer = deskewer;

            AllowCancel = true;
            AllowBackground = true;
        }

        public bool Start(ICollection<ScannedImage> images, DeskewParams deskewParams)
        {
            ProgressTitle = MiscResources.AutoDeskewProgress;
            Status = new OperationStatus
            {
                StatusText = MiscResources.AutoDeskewing,
                MaxProgress = images.Count
            };

            RunAsync(() =>
            {
                var memoryLimitingSem = new Semaphore(4, 4);
                Pipeline.For(images).StepParallel(img =>
                {
                    if (CancelToken.IsCancellationRequested)
                    {
                        return null;
                    }
                    memoryLimitingSem.WaitOne();
                    var bitmap = imageRenderer.Render(img).Result;
                    try
                    {
                        if (CancelToken.IsCancellationRequested)
                        {
                            return null;
                        }
                        var transform = deskewer.GetDeskewTransform(bitmap);
                        if (CancelToken.IsCancellationRequested)
                        {
                            return null;
                        }
                        bitmap = imageContext.PerformTransform(bitmap, transform);
                        var thumbnail = deskewParams.ThumbnailSize.HasValue
                            ? imageContext.PerformTransform(bitmap, new ThumbnailTransform(deskewParams.ThumbnailSize.Value))
                            : null;
                        lock (img)
                        {
                            img.AddTransform(transform);
                            if (thumbnail != null)
                            {
                                img.SetThumbnail(thumbnail);
                            }
                        }

                        // The final pipeline step is pretty fast, so updating progress here is more accurate
                        lock (this)
                        {
                            Status.CurrentProgress += 1;
                        }
                        InvokeStatusChanged();

                        return Tuple.Create(img, transform);
                    }
                    finally
                    {
                        bitmap.Dispose();
                        memoryLimitingSem.Release();
                    }
                }).Run();
                return !CancelToken.IsCancellationRequested;
            });

            return true;
        }
    }
}
