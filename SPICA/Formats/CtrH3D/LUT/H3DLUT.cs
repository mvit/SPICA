﻿using SPICA.Formats.Common;

using System.Collections.Generic;

namespace SPICA.Formats.CtrH3D.LUT
{
    public class H3DLUT : INamed
    {
        public readonly List<H3DLUTSampler> Samplers;

        private string _Name;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value == null)
                {
                    throw Exceptions.GetNullException("Name");
                }

                _Name = value;
            }
        }

        public H3DLUT()
        {
            Samplers = new List<H3DLUTSampler>();
        }
    }
}
