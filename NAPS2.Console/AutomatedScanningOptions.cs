/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace NAPS2
{
    public class AutomatedScanningOptions
    {
        [Option('o', "output", Required = true, HelpText = "The path to the file to save.")]
        public string OutputPath { get; set; }

        [Option('p', "profile", HelpText = "The name of the profile to use for scanning. If not specified, the most-recently-used profile is selected.")]
        public string ProfileName { get; set; }

        [Option('v', "verbose", HelpText = "Display progress information. If not specified, no output is displayed if the scan is successful.")]
        public bool Verbose { get; set; }

        [Option('n', "number", DefaultValue = 1, HelpText = "The number of pages to scan.")]
        public int Number { get; set; }

        [Option('d', "delay", DefaultValue = 0, HelpText = "The delay (in milliseconds) before each page is scanned.")]
        public int Delay { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        // TODO: PDF/Image/ImageFormat
    }
}
