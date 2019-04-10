#pragma once
#ifndef __QUEUE__
#define __QUEUE__

namespace Core
{
	/// <summary>
	/// Simple FIFO queue template, with a fixed max size so it doesn't allocate
	/// </summary>
	template <typename T, int MaxCount>
	class Queue
	{
		T items[MaxCount];
		int count;
		int reader;
		int writer;

	public:
		/// <summary>
		/// Constructor
		/// </summary>
		Queue()
			: count(0)
			, reader(0)
			, writer(0)
		{
		}

		/// <summary>
		/// Add an element to the queue
		/// Returns true if the element could be added
		/// </summary>
		bool enqueue(T clientIndex)
		{
			noInterrupts();
			bool ret = count < MaxCount;
			if (ret)
			{
				items[writer] = clientIndex;
				writer = (writer + 1) % MaxCount;
				count++;
			}
			interrupts();
			return ret;
		}

		/// <summary>
		/// Tries to pop the oldest element
		/// Returns true if the element could be popped
		/// </summary>
		bool tryDequeue(T& outItem)
		{
			noInterrupts();
			bool ret = count > 0;
			if (ret)
			{
				outItem = items[reader];
				reader = (reader + 1) % MaxCount;
				count--;
			}
			interrupts();
			return ret;
		}

		/// <summary>
		/// Clear the event queue
		/// </summary>
		void clear()
		{
			noInterrupts();
			count = 0;
			writer = 0;
			reader = 0;
			interrupts();
		}
	};

}

#endif
