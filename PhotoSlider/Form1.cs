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
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace PhotoSlider
{
    public partial class Form1 : Form
    {
        public Form1(IApplicationController controller, ImagesScaner imagesLoader, Screen screen, Options options)
        {
            InitializeComponent();
            mOptions = options;
            mScreen = screen;
            mController = controller;
            Size = new Size(mScreen.Bounds.Width, mScreen.Bounds.Height);

            mImagesLoader = imagesLoader;
        }

        ScreenComposer mScreenComposer;
        ImagesScaner mImagesLoader;
        IApplicationController mController;
        Screen mScreen;

        PictureControlDesc findDescription(PictureBox pictureBox)
        {
            for (int i = 0; i < mPictures.Count; ++i)
                if (mPictures[i].mPictureBox == pictureBox)
                    return mPictures[i];
            return null;
        }

        PictureBox mLastMenuPictureBox = null;
        void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                PictureControlDesc desc = findDescription(sender as PictureBox);
                if (desc != null)
                    showNextImage(desc, ImageAction.Skip, null);
                return;
            }
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                PictureControlDesc desc = findDescription(sender as PictureBox);
                if (desc != null)
                    showNextImage(desc, ImageAction.Move, null);
                return;
            }
            PictureBox pictureBox = (sender as PictureBox);
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
            {
                mLastMenuPictureBox = pictureBox;
                return;
            }
        }

        public void showPicturesBoxes(int columns, int rows)
        {
            if (mLayout != null)
            {
                reuseLayout(mLayout);
                mLayout.Dispose();
                mLayout = null;
            }
            mWidth = Size.Width / columns;
            mHeight = Size.Height / rows;
            if (mScreenComposer != null)
            {
                mScreenComposer.Stop();
                var layout = mScreenComposer.GetNextLayout();
                while (layout != null)
                {
                    reuseLayout(layout);
                    layout.Dispose();
                    layout = mScreenComposer.GetNextLayout();
                }
                mScreenComposer.Dispose();
            }
            mScreenComposer = new ScreenComposer(mImagesLoader, Size, new Size(Size.Width / columns, Size.Height / rows));
           
            for (int picture = 0; picture < mPictures.Count; ++picture)
            {
                mPictures[picture].mPictureBox.Hide();
                mPictures[picture].mPictureBox.Image = null;
                mPictures[picture].mImageDescriptor = null;
            }
            updateImages(ImageAction.Reuse);
        }

        private void AddPictureBox()
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox.BackColor = Color.Black;
            pictureBox.Visible = false;
            pictureBox.MouseDown += new MouseEventHandler(pictureBox_MouseDown);
            pictureBox.ContextMenuStrip = contextMenuStrip1;
            this.Controls.Add(pictureBox);
            PictureControlDesc desc = new PictureControlDesc();
            desc.mPictureBox = pictureBox;
            mPictures.Add(desc);
        }

        class PictureControlDesc
        {
            public PictureBox mPictureBox;
            public StackableImage mImageDescriptor;
        }

        List<PictureControlDesc> mPictures = new List<PictureControlDesc>();
        int mWidth;
        int mHeight;
        Options mOptions;

        public enum ImageAction
        {
            Delete,
            Skip,
            Move,
            Reuse,
        }

        ImagesLayout mLayout = null;
        public void updateImages(ImageAction action)
        {
            ImagesLayout oldLayout = mLayout;
            mLayout = mScreenComposer.GetNextLayout();
            if (mLayout != null)
            {
                ImagesRows rows = mLayout.Rows;
                int i = 0;
                showRows(rows, 0, 0, Size.Height, Size.Width, action, ref i);
                for (int picture = i; picture < mPictures.Count; ++picture)
                {
                    mPictures[picture].mPictureBox.Hide();
                    showNextImage(mPictures[picture], action, null);
                }
            }
            else
            {
                for (int picture = 0; picture < mPictures.Count; ++picture)
                {
                    mPictures[picture].mPictureBox.Hide();
                    showNextImage(mPictures[picture], action, null);
                }
            }
            if (oldLayout != null)
                oldLayout.Dispose();
        }

        void showRow(ImagesRow row, int top, int left, int width, double vgap, ImageAction action, ref int i)
        {
            int rowWidth = row.Width;
            int rowHeight = row.Height;
            double gap = ((double)(width - rowWidth) / (double)row.Count);
            for (int cell = 0; cell < row.Count; ++cell)
            {
                if (mPictures.Count <= i)
                    AddPictureBox();

                IRowItem item = row[cell];
                if (item.GetItemType() == RowType.Image)
                {
                    StackableImage image = item as StackableImage;
                    mPictures[i].mPictureBox.Left = left;
                    mPictures[i].mPictureBox.Top = top;
                    mPictures[i].mPictureBox.Height = (int)Math.Round(rowHeight + vgap);
                    mPictures[i].mPictureBox.Width = (int)Math.Round(image.Width + gap);
                    left = mPictures[i].mPictureBox.Left + mPictures[i].mPictureBox.Width;

                    showNextImage(mPictures[i], action, image);
                    mPictures[i].mPictureBox.Show();
                    ++i;
                }
                else if (item.GetItemType() == RowType.Rows)
                {
                    showRows(item as ImagesRows, top, left, row.Height, (int)(item.Width + gap), action, ref i);
                    left += (int)(item.Width + gap);
                }
            }
        }

        void showRows(ImagesRows rows, int top, int left, int height, int width, ImageAction action, ref int i)
        {
            if (rows.RowCount == 0)
                return;
            double vgap = ((double)(height - rows.Height) / (double)rows.RowCount);
            for (int row = 0; row < rows.RowCount; ++row)
            {
                IRowItem item = rows[row];
                showRow(item as ImagesRow, top, left, width, vgap, action, ref i);
                top += (int)Math.Round(item.Height + vgap);
            }
        }

        void showNextImage(PictureControlDesc picture, ImageAction action, StackableImage next)
        {
            System.Drawing.Image img = next == null ? null : next.Image;
            if (picture.mPictureBox.Image != null)
                picture.mPictureBox.Image.Dispose();
            if (img == null)
                picture.mPictureBox.Image = null;
            else
            {
            	try
            	{
                    //TODO: optimize
                    picture.mPictureBox.Image = (System.Drawing.Image)img.Clone();
            	}
            	catch (Exception ex)
            	{
            		MessageBox.Show("This is the exception I'm searched for! " + ex.ToString(),
            		                "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            	}
            }
            if (picture.mImageDescriptor != null)
            {
                switch (action)
                {
                    case ImageAction.Reuse:
                        mImagesLoader.Return(picture.mImageDescriptor.Path);
                        break;
                    case ImageAction.Delete:
                        picture.mImageDescriptor.Dispose();
                        try
                        {
                            if (string.IsNullOrEmpty(mOptions.MoveDeletedPath))
                                System.IO.File.Delete(picture.mImageDescriptor.Path);
                            else
                                moveFile(picture.mImageDescriptor.Path, mOptions.MoveDeletedPath);

                        }
                        catch (IOException ex)
                        {
                            System.IO.File.SetAttributes(picture.mImageDescriptor.Path, FileAttributes.Normal);
                            // do nothing
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            if (System.IO.File.GetAttributes(picture.mImageDescriptor.Path).HasFlag(System.IO.FileAttributes.ReadOnly))
                            {
                                System.IO.File.SetAttributes(picture.mImageDescriptor.Path, FileAttributes.Normal);
                                System.IO.File.Delete(picture.mImageDescriptor.Path);
                            }
                        }
                        break;
                    case ImageAction.Move:
                        {
                            picture.mImageDescriptor.Dispose();
                            string movePath = mOptions.MovePath;
                            moveFile(picture.mImageDescriptor.Path, movePath);
                        }
                        break;
                    case ImageAction.Skip:
                        picture.mImageDescriptor.Dispose();
                        break;
                }
            }
            picture.mImageDescriptor = next;
        }

        private static void moveFile(string filePath, string movePath)
        {
            if (!Directory.Exists(movePath))
                Directory.CreateDirectory(movePath);
            string originalFileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string ext = System.IO.Path.GetExtension(filePath);
            string targetFileName = Path.Combine(movePath, originalFileName + ext);
            int i = 0;
            while (File.Exists(targetFileName))
            {
                ++i;
                targetFileName = Path.Combine(movePath, originalFileName + "_" + i.ToString() + ext);
            }
            try
            {
                File.Move(filePath, targetFileName);
            }
            catch (UnauthorizedAccessException) { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            showPicturesBoxes((int)mOptions.Columns, (int)mOptions.Rows);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    mController.Exit();
                    break;
                case Keys.Delete:
                    mController.Delete();
                    break;
                case Keys.Space:
                    mController.Skip();
                    break;
                case Keys.M:
                    mController.Move();
                    break;
                case Keys.Left:
                    mController.AddHorizontal();
                    break;
                case Keys.Right:
                    mController.RemoveHorizontal();
                    break;
                case Keys.Up:
                    mController.AddVertical();
                    break;
                case Keys.Down:
                    mController.RemoveVertical();
                    break;
            }
        }
        
        #region reuse of current images
        void reuseRow(ImagesRow row)
        {
            for (int i = 0; i < row.Count; ++i)
            {
                switch (row[i].GetItemType())
                {
                    case RowType.Rows:
                        reuseRows(row[i] as ImagesRows);
                        break;
                    case RowType.Row:
                        reuseRow(row[i] as ImagesRow);
                        break;
                    case RowType.Image:
                        mImagesLoader.Return((row[i] as StackableImage).Path);
                        break;
                }
            }
        }

        void reuseRows(ImagesRows rows)
        {
            for (int i = 0; i < rows.RowCount; ++i)
                reuseRow(rows[i] as ImagesRow);
        }

        void reuseLayout(ImagesLayout layout)
        {
            reuseRows(layout.Rows);
        }
        #endregion

        /// <summary>
        /// Stops all work
        /// </summary>
        public void Stop()
        {
            if (mScreenComposer != null)
                mScreenComposer.Dispose();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mScreenComposer != null)
                mScreenComposer.Dispose();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureControlDesc desc = findDescription(mLastMenuPictureBox);
            if (desc != null)
                showNextImage(desc, ImageAction.Delete, null);
        }

        private void moveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureControlDesc desc = findDescription(mLastMenuPictureBox);
            if (desc != null)
                showNextImage(desc, ImageAction.Move, null);
        }

        private void skipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureControlDesc desc = findDescription(mLastMenuPictureBox);
            if (desc != null)
                showNextImage(desc, ImageAction.Skip, null);
        }
    }
}
