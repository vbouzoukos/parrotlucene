﻿using System;
using System.Runtime.CompilerServices;

namespace ParrotLucene.Indexing.Metadata
{
    /// <summary>
    /// We use this to store field data only. Used for query by id only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class LuceneField:Attribute
    {
        public string Name { get; set; }

        public LuceneField([CallerMemberName] string name="")
        {
            Name = name;
        }
    }
}
