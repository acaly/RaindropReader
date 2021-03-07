﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class PluginAttribute : Attribute
    {
        public string Name { get; }

        public PluginAttribute(string name)
        {
            Name = name;
        }
    }
}
