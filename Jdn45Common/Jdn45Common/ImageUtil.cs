using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Jdn45Common
{
    /// <summary>
    /// Image manipulation functions.
    /// </summary>
    public static class ImageUtil
    {
        /// <summary>
        /// Crop an image in the given rectangle.
        /// </summary>
        /// <param name="img"></param>
        /// <param name="cropArea"></param>
        /// <returns></returns>
        public static Image Crop(Image image, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(image);
            Bitmap bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);

            return bmpCrop;
        }

        /// <summary>
        /// Resize an image to the given size.
        /// If keeping the ratio, there may be rounding errors.
        /// It's recommended that you crop to the desired ration and then call this method with keepRatio=false.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="size"></param>
        /// <param name="keepRatio">Whether or not to keep the aspect ratio of the image. If true, final size may be different than specified.</param>
        /// <returns></returns>
        public static Image Resize(Image image, Size size, bool keepRatio)
        {
            int destWidth, destHeight;

            if (keepRatio)
            {
                int sourceWidth = image.Width;
                int sourceHeight = image.Height;

                float nPercentW = ((float)size.Width / (float)sourceWidth);
                float nPercentH = ((float)size.Height / (float)sourceHeight);

                float nPercent = 0;
                if (nPercentH < nPercentW)
                    nPercent = nPercentH;
                else
                    nPercent = nPercentW;

                destWidth = (int)(sourceWidth * nPercent);
                destHeight = (int)(sourceHeight * nPercent);
            }
            else
            {
                destWidth = size.Width;
                destHeight = size.Height;
            }

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            g.DrawImage(image, 0, 0, destWidth, destHeight);
            g.Dispose();

            return b;
        }

        /// <summary>
        /// Gets the square in the center of the image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Rectangle GetCenterSquare(Image image)
        {
            // If it's already a square, return its dimensions
            if (image.Width == image.Height)
            {
                return new Rectangle(0, 0, image.Width, image.Height);
            }

            float curRatio = (float)image.Width / (float)image.Height;

            int top, bottom, left, right;
            if (curRatio > 1)
            {
                // Rectangle in the middle of left and right
                top = 0;
                bottom = image.Height;
                left = (int)((image.Width - image.Height) / 2);
                right = (int)((image.Width + image.Height) / 2);
            }
            else
            {
                // Rectangle in the middle of top and bottom
                top = (int)((image.Height - image.Width) / 2);
                bottom = (int)((image.Height + image.Width) / 2);
                left = 0;
                right = image.Width;
            }

            return new Rectangle(left, top, right - left, bottom - top);
        }
    }
}
