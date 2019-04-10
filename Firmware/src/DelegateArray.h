#pragma once

/// <summary>
/// Stores an array of delegates (i.e. object and function)
/// </summary>
template<typename DelegateType, int MaxSize>
struct DelegateArray
{
	struct HandlerAndToken
	{
		DelegateType handler;
		void* token;
	};

private:
	HandlerAndToken items[MaxSize];
	int itemCount;

public:
	/// <summary>
	/// Constructor
	/// </summary>
	DelegateArray()
		: itemCount(0)
	{
	}

	/// <summary>
	/// Add a delegate to the list
	/// </summary>
	bool Register(void* token, DelegateType handler)
	{
		bool ret = (itemCount < MaxSize);
		if (ret)
		{
			items[itemCount].handler = handler;
			items[itemCount].token = token;
			itemCount++;
		}
		return ret;
	}

	/// <summary>
	/// Removes a delegate by handler
	/// </summary>
	void UnregisterWithHandler(DelegateType handler)
	{
		int index = 0;
		for (; index < itemCount; ++index)
		{
			if (items[index].handler == handler)
				break;
		}

		if (index != itemCount)
		{
			// Shift entries down
			itemCount--;
			for (; index < itemCount; ++index)
				items[index] = items[index + 1];
		}
	}

	/// <summary>
	/// Removes a delegate by token
	/// </summary>
	void UnregisterWithToken(void* token)
	{
		int index = 0;
		for (; index < itemCount; ++index)
		{
			if (items[index].token == token)
				break;
		}

		if (index != itemCount)
		{
			// Shift entries down
			itemCount--;
			for (; index < itemCount; ++index)
				items[index] = items[index + 1];
		}
	}

	/// <summary>
	/// How many registered delegates?
	/// </summary>
	int Count() const
	{
		return itemCount;
	}

	/// <summary>
	/// Fetch a delegate and token by index
	/// </summary>
	const HandlerAndToken& operator[](int index) const
	{
		return items[index];
	}

	/// <summary>
	/// Fetch a delegate and token by index, non-const
	/// </summary>
	HandlerAndToken& operator[](int index)
	{
		return items[index];
	}
};
