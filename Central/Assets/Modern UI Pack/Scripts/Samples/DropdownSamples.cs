using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack; // You'll need this if you won't use the namespace

namespace Michsky.UI.ModernUIPack
{
    public class DropdownSamples : MonoBehaviour
    {
        [Header("VARIABLES")]
        public CustomDropdown dropdownVariable; // Dropdown variable

        [Header("CREATING A SINGLE ITEM")]
        public Sprite itemIcon; // We'll be taking the item icon from this variable
        public string itemName; // We'll be taking the item title from this variable

        [Header("CREATING ITEMS FROM A LIST")]
        public List<string> itemList = new List<string>(); // You can spawn items from a list too!

        public void GenerateItem()
        {
            dropdownVariable.SetItemIcon(itemIcon); // Setting up the icon - OPTIONAL
            dropdownVariable.SetItemTitle(itemName); // Setting up the title
            dropdownVariable.CreateNewItem(); // And finally, creating the item
        }

        public void GenerateListItems()
        {
            for (int i = 0; i < itemList.Count; ++i)
            {
                dropdownVariable.SetItemIcon(itemIcon); // Setting up the icon - OPTIONAL
                dropdownVariable.SetItemTitle(itemList[i]); // Setting up the title
                dropdownVariable.CreateNewItem(); // And finally, creating the item
            }
        }
    }
}