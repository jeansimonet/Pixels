#pragma once

namespace Core
{
	/// <summary>
	/// Simple FIFO queue template, with a fixed max size so it doesn't allocate
	/// </summary>
	template <typename T, int MaxCount>
	class Queue
	{
		T items[MaxCount];
		int _count;
		int _reader;
		int _writer;

	public:
		/// <summary>
		/// Constructor
		/// </summary>
		Queue()
			: _count(0)
			, _reader(0)
			, _writer(0)
		{
		}

		/// <summary>
		/// Add an element to the queue
		/// Returns true if the element could be added
		/// </summary>
		bool enqueue(const T& clientIndex)
		{
			bool ret = false;
			CRITICAL_REGION_ENTER();
			ret = _count < MaxCount;
			if (ret)
			{
				items[_writer] = clientIndex;
				_writer = (_writer + 1) % MaxCount;
				_count++;
			}
			CRITICAL_REGION_EXIT();
			return ret;
		}

		/// <summary>
		/// Tries to pop the oldest element
		/// Returns true if the element could be popped
		/// </summary>
		bool tryDequeue(T& outItem)
		{
			bool ret = false;
			CRITICAL_REGION_ENTER();
			ret = _count > 0;
			if (ret)
			{
				outItem = items[_reader];
				_reader = (_reader + 1) % MaxCount;
				_count--;
			}
			CRITICAL_REGION_EXIT();
			return ret;
		}

		typedef bool(*TryDequeueFunctor)(T& item);

		/// <summary>
		/// Tries to pop the oldest element and call functor on it,
		/// Returns true if the element could be popped AND functor could process it
		/// if functor could not process element, then it isn't popped
		/// </summary>
		bool tryDequeue(TryDequeueFunctor functor)
		{
			bool ret = false;
			CRITICAL_REGION_ENTER();
			ret = _count > 0;
			if (ret)
			{
				auto& outItem = items[_reader];
				ret = functor(outItem);
				if (ret)
				{
					_reader = (_reader + 1) % MaxCount;
					_count--;
				}
			}
			CRITICAL_REGION_EXIT();
			return ret;
		}

		/// <summary>
		/// Clear the event queue
		/// </summary>
		void clear()
		{
			CRITICAL_REGION_ENTER();
			_count = 0;
			_writer = 0;
			_reader = 0;
			CRITICAL_REGION_EXIT();
		}

		int count() const
		{
			return _count;
		}
	};

}
