#pragma once

#include <fastmath.h>
#include "float3.h"

namespace Core
{
	struct matrix3x3
	{
		float m11; float m12; float m13;
		float m21; float m22; float m23;
		float m31; float m32; float m33;

		matrix3x3() {}
		matrix3x3(const float3& col1, const float3& col2, const float3& col3)
            : m11(col1.x), m12(col2.x), m13(col3.x)
            , m21(col1.y), m22(col2.y), m23(col3.y)
            , m31(col1.z), m32(col2.z), m33(col3.z)
        {}
        float3 col1() const { return float3(m11, m21, m31); }
        float3 col2() const { return float3(m12, m22, m32); }
        float3 col3() const { return float3(m13, m23, m33); }
        float3 row1() const { return float3(m11, m12, m13); }
        float3 row2() const { return float3(m21, m22, m23); }
        float3 row3() const { return float3(m31, m32, m33); }

        inline static matrix3x3 transpose(const matrix3x3& m) {
            matrix3x3 ret;
            ret.m11 = m.m11; ret.m12 = m.m21; ret.m13 = m.m31;
            ret.m21 = m.m12; ret.m22 = m.m22; ret.m23 = m.m32;
            ret.m31 = m.m13; ret.m32 = m.m32; ret.m33 = m.m33;
            return ret;
        }

        inline static float3 mul(const matrix3x3& left, const float3& right) {
            return float3(
                float3::dot(left.row1(), right),
                float3::dot(left.row2(), right),
                float3::dot(left.row3(), right));
        }

        inline static matrix3x3 mul(const matrix3x3& left, const matrix3x3& right) {
            matrix3x3 ret;
            ret.m11 = float3::dot(left.row1(), right.col1()); ret.m12 = float3::dot(left.row1(), right.col2()); ret.m13 = float3::dot(left.row1(), right.col3());
            ret.m21 = float3::dot(left.row2(), right.col1()); ret.m22 = float3::dot(left.row2(), right.col2()); ret.m23 = float3::dot(left.row2(), right.col3());
            ret.m31 = float3::dot(left.row3(), right.col1()); ret.m32 = float3::dot(left.row3(), right.col2()); ret.m33 = float3::dot(left.row3(), right.col3());
            return ret;
        }
	};
}