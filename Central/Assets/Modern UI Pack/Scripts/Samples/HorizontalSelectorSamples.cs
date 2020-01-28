using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack; // You'll need this if you won't use the namespace

namespace Michsky.UI.ModernUIPack
{
    public class HorizontalSelectorSamples : MonoBehaviour
    {
        [Header("VARIABLES")]
        public HorizontalSelector selectorVariable; // HS variable

        [Header("CREATING A SINGLE ITEM")]
        public string itemName; // We'll be taking the item title from this variable

        [Header("CREATING ITEMS FROM A LIST")]
        public List<string> itemList = new List<string>(); // You can spawn items from a list too!

        public void GenerateItem()
        {
            selectorVariable.CreateNewItem(itemName); // Creating the item - taking the titel from itemName variable
            selectorVariable.UpdateUI(); // We've created a new item, so, let's update the element
        }

        public void GenerateListItems()
        {
            for (int i = 0; i < itemList.Count; ++i)
            {
                selectorVariable.CreateNewItem(itemList[i]); // Creating the item - taking the titel from itemName variable
                selectorVariable.UpdateUI(); // We've created new items, so, let's update the element
            }
        }
    }
}