﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotusLabsTimeTracker.model
{
    public class EntityCode : EntityHistory
    {
        public virtual String code { get; set; }
        public virtual String name { get; set; }
        public virtual String description { get; set; }

    }
}
