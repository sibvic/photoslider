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

namespace PhotoSlider
{
    class ImagesRow : IDisposable, IRowItem
    {
        public ImagesRow()
        {
        }

        List<IRowItem> mImages = new List<IRowItem>();
        public int Width
        {
            get
            {
                int width = 0;
                foreach (IRowItem image in mImages)
                {
                    width += image.Width;
                }

                return width;
            }
        }

        public int Height
        {
            get
            {
                int height = 0;
                foreach (IRowItem image in mImages)
                {
                    int imageHeigh = image.Height;
                    if (height < imageHeigh)
                        height = imageHeigh;
                }

                return height;
            }
        }

        public void Add(IRowItem image)
        {
            mImages.Add(image);
        }

        public void Remove(IRowItem item)
        {
            mImages.Remove(item);
        }

        public void Insert(IRowItem item, int position)
        {
            mImages.Insert(position, item);
        }

        public int Count
        {
            get { return mImages.Count; }
        }

        public IRowItem this[int index]
        {
            get { return mImages[index]; }
        }

        bool mDisposing = false;
        public void Dispose()
        {
            if (mDisposing)
                return;
            mDisposing = true;
            foreach (IRowItem image in mImages)
                image.Dispose();
        }

        public RowType GetItemType()
        {
            return RowType.Row;
        }
    }
}
