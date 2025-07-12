using System;
using System.Collections.Generic;

namespace Structura
{
    /// <summary>
    /// 타입 생성 모드를 정의합니다.
    /// </summary>
    public enum TypeGenerationMode
    {
        /// <summary>
        /// 레코드 타입으로 생성
        /// </summary>
        Record,
        
        /// <summary>
        /// 클래스 타입으로 생성
        /// </summary>
        Class,
        
        /// <summary>
        /// 구조체 타입으로 생성
        /// </summary>
        Struct
    }
}