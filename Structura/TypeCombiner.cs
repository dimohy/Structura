using System;
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
        /// Combines two types to create a new type.
        /// </summary>
        public static TypeCombinerBuilder<T1, T2> Combine<T1, T2>()
            => new TypeCombinerBuilder<T1, T2>();

        /// <summary>
        /// Combines a single type.
        /// </summary>
        public static TypeCombinerBuilder<T> Combine<T>()
            => new TypeCombinerBuilder<T>();

        /// <summary>
        /// Combines anonymous types only.
        /// </summary>
        public static AnonymousTypeCombinerBuilder Combine()
            => new AnonymousTypeCombinerBuilder();

        /// <summary>
        /// Creates a new type based on an existing type.
        /// </summary>
        public static SingleTypeCombinerBuilder<T> From<T>()
            => new SingleTypeCombinerBuilder<T>();
    }
}