using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFocusable
{
	//event System.Action<IFocusable> GotFocus;
	bool HasFocus { get; }
	void GiveFocus();
	void RemoveFocus();
}
