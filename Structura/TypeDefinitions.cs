using System;
using System.Collections.Generic;

namespace Structura
{
    /// <summary>
    /// Defines the type generation mode.
    /// </summary>
    public enum TypeGenerationMode
    {
        /// <summary>
        /// Generate as record type
        /// </summary>
        Record,
        
        /// <summary>
        /// Generate as class type
        /// </summary>
        Class,
        
        /// <summary>
        /// Generate as struct type
        /// </summary>
        Struct
    }
}