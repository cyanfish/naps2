/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NTwain;

namespace NAPS2.WinForms
{
    internal partial class FTwainGui : FormBase
    {
        private bool activated;

        public FTwainGui()
        {
            InitializeComponent();
            RestoreFormState = false;

            Activated += OnActivated;
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            if (activated)
                return;
            activated = true;

            // TODO: Pass in show/noshow
            // TODO: (Somewhere else) configure ds
            DataSource.Enable(SourceEnableMode.ShowUI, true, Handle); // SourceEnableMode.ShowUIOnly could be useful
        }

        public DataSource DataSource { get; set; }
    }
}
