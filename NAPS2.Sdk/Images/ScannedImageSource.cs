using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NAPS2.Images
{
    public abstract class ScannedImageSource
    {
        public abstract Task<ScannedImage> Next();
        
        public async Task<List<ScannedImage>> ToList()
        {
            var list = new List<ScannedImage>();
            try
            {
                await ForEach(image => list.Add(image));
            }
            catch (Exception)
            {
                // TODO: If we ever allow multiple enumeration, this will need to be rethought
                foreach (var image in list)
                {
                    image.Dispose();
                }
                throw;
            }
            return list;
        }

        public async Task ForEach(Action<ScannedImage> action)
        {
            ScannedImage image;
            while ((image = await Next()) != null)
            {
                action(image);
            }
        }

        public async Task ForEach(Func<ScannedImage, Task> action)
        {
            ScannedImage image;
            while ((image = await Next()) != null)
            {
                await action(image);
            }
        }
    }
}
