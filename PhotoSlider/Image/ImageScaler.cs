//
//  Photo Slider
//  Copyright (C) 2011 Victor Tereschenko (aka sibvic)
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
// ========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PhotoSlider
{
    class ImageScaler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bmp"></param>
        /// <exception cref="OutOfMemoryException">TODO</exception>
        /// <returns></returns>
        public static Image ScaleImage(int width, int height, Image bmp)
        {
            int targetWidth;
            int targetHeight;
            if (bmp.Width > width || bmp.Height > height)
            {
                double wrate = (double)bmp.Width / (double)width;
                double hrate = (double)bmp.Height / (double)height;
                if (wrate > hrate)
                {
                    targetWidth = width;
                    targetHeight = (int)Math.Round(((double)width * (double)bmp.Height / (double)bmp.Width));
                }
                else
                {
                    targetHeight = height;
                    targetWidth = (int)Math.Round(((double)height * (double)bmp.Width / (double)bmp.Height));
                }
            }
            else
                return (Image)bmp.Clone();

            Bitmap image = new Bitmap(targetWidth, targetHeight);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.DrawImage(bmp, 0, 0, targetWidth, targetHeight);
            }
            return image;
        }
    }
}
