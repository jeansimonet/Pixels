#pragma once

#include <fastmath.h>

namespace Core
{
	struct float3
	{
		float x;
		float y;
		float z;

		float3() {}
		float3(float ax, float ay, float az) : x(ax), y(ay), z(az) {}
		float3(const float3& model) : x(model.x), y(model.y), z(model.z) {}
		float3& operator=(const float3& model)
		{
			x = model.x;
			y = model.y;
			z = model.z;
			return *this;
		}
		float3& operator+=(const float3& right)
		{
			x += right.x;
			y += right.y;
			z += right.z;
			return *this;
		}
		float3& operator-=(const float3& right)
		{
			x -= right.x;
			y -= right.y;
			z -= right.z;
			return *this;
		}
		float3& operator*=(float right)
		{
			x *= right;
			y *= right;
			z *= right;
			return *this;
		}
		float3& operator/=(float right)
		{
			x /= right;
			y /= right;
			z /= right;
			return *this;
		}
		float sqrMagnitude() const
		{
			return x * x + y * y + z * z;
		}
		float magnitude() const
		{
			return sqrt(sqrMagnitude());
		}
		float3& normalize()
		{
			float mag = magnitude();
			x /= mag;
			y /= mag;
			z /= mag;
			return *this;
		}
		float3 normalized() const
		{
			float3 ret = *this;
			ret.normalize();
			return ret;
		}
		static float dot(const float3& left, const float3& right)
		{
			return left.x * right.x + left.y * right.y + left.z * right.z;
		}
		static float3 cross(const float3& left, const float3& right)
		{
			return float3(
				left.y * right.z - left.z * right.y,
				left.z * right.x - left.x * right.z,
				left.x * right.y - left.y * right.y
			);
		}

		static float3 zero() { return float3(0, 0, 0); }
	};

	inline float3 operator+(const float3& left, const float3& right)
	{
		return float3(left.x + right.x, left.y + right.y, left.z + right.z);
	}
	inline float3 operator-(const float3& left, const float3& right)
	{
		return float3(left.x - right.x, left.y - right.y, left.z - right.z);
	}
	inline float3 operator*(const float3& left, float right)
	{
		return float3(left.x * right, left.y * right, left.z * right);
	}
	inline float3 operator*(float left, const float3& right)
	{
		return float3(left * right.x, left * right.y, left * right.z);
	}
	inline float3 operator/(const float3& left, float right)
	{
		return float3(left.x / right, left.y / right, left.z / right);
	}
}