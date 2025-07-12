using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Structura
{
    /// <summary>
    /// 플루언트 API를 통해 타입 조합 및 조작을 위한 메인 클래스
    /// </summary>
    public static class TypeCombiner
    {
        /// <summary>
        /// 여러 타입을 결합하여 새로운 타입을 생성합니다.
        /// </summary>
        public static TypeCombinerBuilder<T1, T2> Combine<T1, T2>()
            => new TypeCombinerBuilder<T1, T2>();

        /// <summary>
        /// 단일 타입을 결합합니다.
        /// </summary>
        public static TypeCombinerBuilder<T> Combine<T>()
            => new TypeCombinerBuilder<T>();

        /// <summary>
        /// 무명 타입들만을 결합합니다.
        /// </summary>
        public static AnonymousTypeCombinerBuilder Combine()
            => new AnonymousTypeCombinerBuilder();

        /// <summary>
        /// 기존 타입을 기반으로 새로운 타입을 생성합니다.
        /// </summary>
        public static SingleTypeCombinerBuilder<T> From<T>()
            => new SingleTypeCombinerBuilder<T>();
    }
}