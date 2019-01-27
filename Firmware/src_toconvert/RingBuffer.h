#pragma once
#ifndef __RINGBUFFER__
#define __RINGBUFFER__

namespace Core
{
	/// <summary>
	/// Simple Ring Buffer, automatically overwrites older items if necessary
	/// (i.e. it never fails to add items)
	/// </summary>
	template <typename T, int MaxCount>
	class RingBuffer
	{
	private:
		T data[MaxCount];
		int next;

	public:
		/// <summary>
		/// Constructor
		/// </summary>
		RingBuffer()
		{
			memset(&data[0], 0, sizeof(T) * MaxCount);
			next = 0;
		}

		/// <summary>
		/// Adds an item to the ring buffer, replacing an old one if necessary
		/// </summary>
		/// <param name="frame"></param>
		void push(const T& frame)
		{
			noInterrupts();
			data[next] = frame;
			next = (next + 1) % MaxCount;
			interrupts();
		}

		/// <summary>
		/// Returns the oldest item
		/// </summary>
		const T& first() const
		{
			return data[next];
		}

		/// <summary>
		/// Returns the newest item
		/// </summary>
		/// <returns></returns>
		const T& last() const
		{
			int dataIndex = next - 1;
			if (dataIndex == -1)
				dataIndex = MaxCount - 1;
			return data[dataIndex];
		}

		/// <summary>
		/// Returns the number of items in the buffer
		/// </summary>
		/// <returns></returns>
		constexpr int count() const
		{
			return MaxCount;
		}

		/// <summary>
		/// Allows you to iterate the items from oldest to newest
		/// </summary>
		const T& operator[](int index) const
		{
			int dataIndex = next + index;
			if (dataIndex >= MaxCount)
				dataIndex -= MaxCount;
			return data[dataIndex];
		}
	};
}

#endif
