//
//  Photo Slider
//  Copyright (C) 2015 Victor Tereschenko (aka sibvic)
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

namespace PhotoSlider
{
    /// <summary>
    /// Contains photo slider options.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Parses options from command line arguments.
        /// May throw Exception
        /// </summary>
        /// <exception cref="Exception">Thrown in case of not enought arguments for a parameter.</exception>
        /// <exception cref="NotSupportedException">Thrown in case of not supported/unknown parameter</exception>
        /// <param name="args"></param>
        public void Load(string[] args)
        {
            RunOnAllScreens = false;
            MovePath = "_good";
            MoveDeletedPath = "";
            PrintHelp = false;
            Rows = 1;
            Columns = 1;

            int i = 0;
            while (i < args.Length)
            {
                switch (args[i])
                {
                    case "as":
                        RunOnAllScreens = true;
                        break;
                    case "--movepath":
                        ++i;
                        if (args.Length <= i)
                            throw new Exception("Not enought arguments for --movepath");
                        MovePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), args[i]);
                        break;
                    case "--movedeleted":
                        ++i;
                        if (args.Length <= i)
                            throw new Exception("Not enought arguments for --movedeleted");
                        MoveDeletedPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), args[i]);
                        break;
                    case "--rows":
                        ++i;
                        if (args.Length <= i)
                            throw new Exception("Not enought arguments for --rows");
                        uint rows;
                        if (!uint.TryParse(args[i], out rows))
                            throw new Exception("Incorrect number of rows");
                        if (rows > 50 || rows == 0)
                            throw new Exception("Incorrect number of rows");

                        Rows = rows;
                        break;
                    case "--cols":
                        ++i;
                        if (args.Length <= i)
                            throw new Exception("Not enought arguments for --cols");
                        uint cols;
                        if (!uint.TryParse(args[i], out cols))
                            throw new Exception("Incorrect number of columns");
                        if (cols > 50 || cols == 0)
                            throw new Exception("Incorrect number of columns");

                        Columns = cols;
                        break;
                    case "--help":
                        PrintHelp = true;
                        break;
                    default:
                        throw new NotSupportedException("Unknown argument:" + args[i]);
                }
                ++i;
            }
        }

        public bool RunOnAllScreens { get; private set; }
        public string MovePath { get; private set; }
        public string MoveDeletedPath { get; private set; }
        public uint Rows { get; private set; }
        public uint Columns { get; private set; }
        public bool PrintHelp { get; private set; }
    }
}
