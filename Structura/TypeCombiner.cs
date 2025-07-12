using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Structura
{
    /// <summary>
    /// �÷��Ʈ API�� ���� Ÿ�� ���� �� ������ ���� ���� Ŭ����
    /// </summary>
    public static class TypeCombiner
    {
        /// <summary>
        /// ���� Ÿ���� �����Ͽ� ���ο� Ÿ���� �����մϴ�.
        /// </summary>
        public static TypeCombinerBuilder<T1, T2> Combine<T1, T2>()
            => new TypeCombinerBuilder<T1, T2>();

        /// <summary>
        /// ���� Ÿ���� �����մϴ�.
        /// </summary>
        public static TypeCombinerBuilder<T> Combine<T>()
            => new TypeCombinerBuilder<T>();

        /// <summary>
        /// ���� Ÿ�Ե鸸�� �����մϴ�.
        /// </summary>
        public static AnonymousTypeCombinerBuilder Combine()
            => new AnonymousTypeCombinerBuilder();

        /// <summary>
        /// ���� Ÿ���� ������� ���ο� Ÿ���� �����մϴ�.
        /// </summary>
        public static SingleTypeCombinerBuilder<T> From<T>()
            => new SingleTypeCombinerBuilder<T>();
    }
}