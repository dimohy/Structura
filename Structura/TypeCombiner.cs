using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Structura
{
    /// <summary>
    /// Main class for type combination and generation using fluent API
    /// </summary>
    public static class TypeCombiner
    {
        /// <summary>
        /// Combines anonymous types only.
        /// </summary>
        public static AnonymousTypeCombinerBuilder Combine()
            => new AnonymousTypeCombinerBuilder();
    }
}