using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class DropdownMultiSelect : MonoBehaviour
    {
        [Header("OBJECTS")]
        public GameObject triggerObject;
        public Transform itemParent;
        public GameObject itemObject;
        public GameObject scrollbar;
        private VerticalLayoutGroup itemList;
        private Transform currentListParent;
        public Transform listParent;

        [Header("SETTINGS")]
        public bool enableIcon = true;
        public bool enableTrigger = true;
        public bool enableScrollbar = true;
        public bool setHighPriorty = true;
        public bool outOnPointerExit = false;
        public bool isListItem = false;
        public AnimationType animationType;

        [Header("WIP - NOT FUNCTIONAL")]
        public bool saveSelected = false;
        public bool invokeAtStart = false;
        public string toggleTag = "Multi Dropdown";

        [SerializeField]
        [Header("CONTENT")]
        public List<Item> dropdownItems = new List<Item>();

        private Animator dropdownAnimator;
        private TextMeshProUGUI setItemText;

        string textHelper;
        string newItemTitle;
        Sprite newItemIcon;
        bool isOn;
        [HideInInspector] public int iHelper = 0;
        [HideInInspector] public int siblingIndex = 0;

        [System.Serializable]
        public class ToggleEvent : UnityEvent<bool> { }

        public enum AnimationType
        {
            FADING,
            SLIDING,
            STYLISH
        }

        [System.Serializable]
        public class Item
        {
            public string itemName = "Dropdown Item";
            public bool isOn = false;
            [SerializeField] public ToggleEvent toggleEvents;
        }

        void Start()
        {
            dropdownAnimator = this.GetComponent<Animator>();
            itemList = itemParent.GetComponent<VerticalLayoutGroup>();
            itemList = itemParent.GetComponent<VerticalLayoutGroup>();

            SetupDropdown();
            currentListParent = transform.parent;

            if (enableScrollbar == true)
            {
                itemList.padding.right = 25;
                scrollbar.SetActive(true);
            }

            else
            {
                itemList.padding.right = 8;
                Destroy(scrollbar);
            }

            if (setHighPriorty == true)
                transform.SetAsLastSibling();
        }

        public void SetupDropdown()
        {
            foreach (Transform child in itemParent)
                GameObject.Destroy(child.gameObject);

            for (int i = 0; i < dropdownItems.Count; ++i)
            {
                GameObject go = Instantiate(itemObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(itemParent, false);

                setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                textHelper = dropdownItems[i].itemName;
                setItemText.text = textHelper;

                Toggle itemToggle;
                itemToggle = go.GetComponent<Toggle>();

                iHelper = i;

                itemToggle.onValueChanged.AddListener(UpdateToggle);

                if (dropdownItems[i].toggleEvents != null)
                    itemToggle.onValueChanged.AddListener(dropdownItems[i].toggleEvents.Invoke);

                if (saveSelected == true)
                {
                    if (invokeAtStart == true)
                    {
                        if (PlayerPrefs.GetInt(toggleTag + "Toggle") == 1)
                            dropdownItems[i].toggleEvents.Invoke(true);

                        else
                            dropdownItems[i].toggleEvents.Invoke(false);
                    }

                    else
                        itemToggle.onValueChanged.AddListener(SaveToggle);
                }

                else
                {
                    if (invokeAtStart == true)
                    {
                        if (dropdownItems[i].isOn == true)
                            dropdownItems[i].toggleEvents.Invoke(true);

                        else
                            dropdownItems[i].toggleEvents.Invoke(false);
                    }

                    else
                    {
                        if (dropdownItems[i].isOn == true)
                            itemToggle.isOn = true;
                        else
                            itemToggle.isOn = false;
                    }
                }

                if (invokeAtStart == true)
                {
                    if (dropdownItems[i].isOn == true)
                        dropdownItems[i].toggleEvents.Invoke(true);

                    else
                        dropdownItems[i].toggleEvents.Invoke(false);
                }
            }

            currentListParent = transform.parent;
        }

        public void UpdateToggle(bool isOn)
        {
            //   if (isOn)
            //         dropdownItems[iHelper].isOn = true;
            //      else
            //
            //        dropdownItems[iHelper].isOn = false;
        }

        public void SaveToggle(bool isOn)
        {
            if (isOn == true)
                PlayerPrefs.SetInt(toggleTag + "Toggle" + iHelper, 1);
            else
                PlayerPrefs.SetInt(toggleTag + "Toggle" + iHelper, 0);
        }

        public void Animate()
        {
            if (isOn == false && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading Out");
                isOn = false;

                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            else if (isOn == false && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding Out");
                isOn = false;

                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            else if (isOn == false && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish Out");
                isOn = false;
                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            if (enableTrigger == true && isOn == false)
                triggerObject.SetActive(false);

            else if (enableTrigger == true && isOn == true)
                triggerObject.SetActive(true);

            if (outOnPointerExit == true)
                triggerObject.SetActive(false);

            if (setHighPriorty == true)
                transform.SetAsLastSibling();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (outOnPointerExit == true)
            {
                if (isOn == true)
                {
                    Animate();
                    isOn = false;
                }

                if (isListItem == true)
                    gameObject.transform.SetParent(currentListParent, true);
            }
        }

        public void UpdateValues()
        {
            if (enableScrollbar == true)
            {
                itemList.padding.right = 25;
                scrollbar.SetActive(true);
            }

            else
            {
                itemList.padding.right = 8;
                scrollbar.SetActive(false);
            }
        }

        public void CreateNewItem()
        {
            Item item = new Item();
            item.itemName = newItemTitle;
            dropdownItems.Add(item);
            SetupDropdown();
        }

        public void SetItemTitle(string title)
        {
            newItemTitle = title;
        }
    }
}