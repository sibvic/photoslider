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
    class ImagesLayout : IDisposable
    {
        public ImagesLayout(int width, int height)
        {
            mMaxHeight = height;
            mMaxWidth = width;
        }

        int mMaxWidth;
        int mMaxHeight;
        ImagesRows mRows = new ImagesRows();

        public ImagesRows Rows
        {
            get { return mRows; }
        }

        public int GetRowsHeight()
        {
            int rowsHeight = 0;
            for (int row = 0; row < mRows.RowCount; ++row)
            {
                rowsHeight += mRows[row].Height;
            }
            return rowsHeight;
        }

        public bool Add(StackableImage image)
        {
            return Add(mRows, image, mMaxWidth, mMaxHeight, true, false);
        }

        bool addSubitem(ImagesRow r, StackableImage image, int maxHeight)
        {
            for (int i = 0; i < r.Count; ++i)
            {
                if (r[i].GetItemType() == RowType.Rows)
                {
                    if (Add(r[i] as ImagesRows, image, r[i].Width, r.Height, false, true))
                        return true;
                }
                else
                {
                    StackableImage desc = r[i] as StackableImage;
                    if (r.Height - desc.Height < image.Height || desc.Width < image.Width)
                        continue;
                    if (desc != null)
                    {
                        ImagesRows subrows = new ImagesRows();
                        Add(subrows, desc, desc.Width, maxHeight, false, true);
                        Add(subrows, image, desc.Width, maxHeight, false, true);
                        r.Insert(subrows, i);
                        r.Remove(desc);
                        return true;
                    }
                }
            }
            return false;
        }

        bool Add(ImagesRows target, StackableImage image, int maxWidth, int maxHeight, bool addSubRows, bool useFind)
        {
            if (image.Width > maxWidth)
                return false;
            int rowsHeight = 0;
            for (int row = 0; row < target.RowCount; ++row)
            {
                if (addSubRows && addSubitem(target[row] as ImagesRow, image, maxHeight))
                    return true;

                int rowHeight = target[row].Height;
                int rowWidth = target[row].Width;
                rowsHeight += rowHeight;
                int heightDiff = rowHeight - image.Height;
                bool widthFits = maxWidth - rowWidth >= image.Width;
                bool heightFits = heightDiff >= 0 || maxHeight - GetRowsHeight() >= Math.Abs(heightDiff);
                if (heightFits && widthFits)
                {
                    ImagesRow imagesRow = target[row] as ImagesRow;
                    imagesRow.Add(image);
                    return true;
                }
            }
            if (maxHeight - rowsHeight >= image.Height)
            {
                ImagesRow newRow = new ImagesRow();
                newRow.Add(image);
                target.Add(newRow);
                return true;
            }

            return false;
        }

        bool mDisposing = false;
        public void Dispose()
        {
            if (mDisposing)
                return;
            mDisposing = true;
            mRows.Dispose();
        }
    }
}
