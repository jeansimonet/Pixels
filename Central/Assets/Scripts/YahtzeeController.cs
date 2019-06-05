using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


public class YahtzeeController : MonoBehaviour
{
	//enum YahtzeeValue
	//{
	//	ThreeOfAKind,
	//	FourOfAKind,
	//	FullHouse,
	//	SmallStraight,
	//	LargeStraight,
	//	Yahtzee
	//}

	//public void OnFaceDataReceived()
	//{
	//	// List all die faces
	//	List<int> faces = new List<int>(dice.Select(d => d.Face));
	//	faces.Sort();
	//	faces.Reverse();

	//	// Count how many of each
	//	int[] counts = new int[6];
	//	foreach (var die in dice)
	//	{
	//		counts[die.Face]++;
	//	}

	//	switch (counts.Max())
	//	{
	//		case 5:
	//			CurrentYahtzeeUI.text = "YAHTZEE";
	//			break;
	//		case 4:
	//			CurrentYahtzeeUI.text = "Four of a kind";
	//			break;
	//		case 3:
	//			if (faces.Contains(2))
	//			{
	//				CurrentYahtzeeUI.text = "Full House";
	//			}
	//			else
	//			{
	//				CurrentYahtzeeUI.text = "Three of a kind";
	//			}
	//			break;
	//		case 2:
	//		case 1:
	//			if (counts[0] >= 1 &&
	//				 counts[1] >= 1 &&
	//				 counts[2] >= 1 &&
	//				 counts[3] >= 1 &&
	//				 counts[4] >= 1)
	//			{
	//				CurrentYahtzeeUI.text = "Small Straight";
	//			}
	//			else if (counts[1] >= 1 &&
	//				 counts[2] >= 1 &&
	//				 counts[3] >= 1 &&
	//				 counts[4] >= 1 &&
	//				 counts[5] >= 1)
	//			{
	//				CurrentYahtzeeUI.text = "Large Straight";
	//			}
	//			else
	//			{
	//				CurrentYahtzeeUI.text = "";
	//			}
	//			break;
	//		default:
	//			CurrentYahtzeeUI.text = "";
	//			break;
	//	}
	//}
}
