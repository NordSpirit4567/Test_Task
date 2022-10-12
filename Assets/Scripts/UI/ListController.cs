using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class ListController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject[] ListsContainer;
    [SerializeField]
    private GameObject[] ListTitles;
    [SerializeField]
    private GameObject[] ListElementCount;
    [SerializeField]
    private GameObject LoadButton;
    [SerializeField]
    private GameObject SaveButton;
    [SerializeField]
    private GameObject IntSortLabel;
    [SerializeField]
    private GameObject StringSortLabel;
    private string[] ListNames;
    private string filename = "list.xml";
    private List<Dictionary<string, int>>[] ListsArray = new List<Dictionary<string, int>>[2];
    private const string ELEMENT_COUNT = "Кол-во элементов: ";
    private const string INT_SORT = "По числовому значению";
    private const string INT_SORT_ASC = "(По возрастанию)";
    private const string INT_SORT_DES = "(По убыванию)";
    private const string STRING_SORT = "По алфавиту";
    private const string STRING_SORT_ASC = "(По возрастанию)";
    private const string STRING_SORT_DES = "(По убыванию)";

    void Start()
    {
        InitializeListWithTestValues();
        RenderListUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SortByInt(bool IsAsc=false)
    {
        string labelText = INT_SORT + "\n\r";
        if (IsAsc)
        {
            ListsArray[0] = ListsArray.ElementAt(0).OrderBy(u => u.ElementAt(0).Value).ToList();
            labelText += INT_SORT_ASC;
        }
        else
        {
            ListsArray[0] = ListsArray.ElementAt(0).OrderByDescending(u => u.ElementAt(0).Value).ToList();
            labelText += INT_SORT_DES;
        }
        IntSortLabel.GetComponent<Text>().text = labelText;
        RenderListUI();
    }

    public void SortByString(bool IsAsc)
    {
        string labelText = STRING_SORT + "\n\r";
        if (IsAsc)
        {
            ListsArray[0] = ListsArray.ElementAt(0).OrderBy(u => u.ElementAt(0).Key).ToList();
            labelText += STRING_SORT_ASC;
        }
        else
        {
            ListsArray[0] = ListsArray.ElementAt(0).OrderByDescending(u => u.ElementAt(0).Key).ToList();
            labelText += STRING_SORT_DES;
        }
        StringSortLabel.GetComponent<Text>().text = labelText;
        RenderListUI();
    }
    public async void LoadFromFile()
    {
        LoadButton.SetActive(false);
        string filePath = Application.persistentDataPath + "/" + filename;
        if (!File.Exists(filePath))
        {
            LoadButton.SetActive(true);
            return;
        }
        try
        {
            using (Stream fs = File.OpenRead(filePath))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;
                List<List<Dictionary<string, int>>> ReaderResult = new List<List<Dictionary<string, int>>>();
                List<string> stringNames = new List<string>();
                int listIndex = -1;
                using (XmlReader reader = XmlReader.Create(fs, settings))
                {
                    while (await reader.ReadAsync())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (reader.Name)
                                {
                                    case "list":
                                        ReaderResult.Add(new List<Dictionary<string, int>>());
                                        listIndex++;
                                        stringNames.Add(reader.GetAttribute("name"));
                                        break;
                                    case "element":
                                        ReaderResult.ElementAt(listIndex).Add(new Dictionary<string, int>() { { reader.GetAttribute("key"), int.Parse(reader.GetAttribute("value")) } });
                                        break;
                                }
                                break;
                            default: break;
                        }
                    }
                }

                ListsArray = new List<Dictionary<string, int>>[ReaderResult.Count];
                for (int i = 0; i < ReaderResult.Count; i++)
                {
                    ListsArray[i] = ReaderResult.ElementAt(i);
                }
                ListNames = new string[stringNames.Count];
                for (int i = 0; i < stringNames.Count; i++)
                {
                    ListNames[i] = stringNames.ElementAt(i);
                }

                RenderListUI();
                UpdateUIListNames();
                UpdateUIListCount();              
            }
            LoadButton.SetActive(true);
        }
        catch (Exception){LoadButton.SetActive(true);}
        
    }
    
    public async void SaveToFile()
    {       
        SaveButton.SetActive(false);
        string filePath = Application.persistentDataPath + "/" + filename;
        
        try
        {
            using (Stream fs = File.OpenWrite(filePath))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Async = true;
                using (XmlWriter writer = XmlWriter.Create(fs, settings))
                {
                    await writer.WriteStartElementAsync(null, "lists", null);
                    for (int i = 0; i < ListsArray.Length; i++)
                    {
                        await writer.WriteStartElementAsync(null, "list", null);
                        await writer.WriteAttributeStringAsync(null, "name", null, ListNames[i]);
                        for (int y = 0; y < ListsArray[i].Count; y++)
                        {
                            await writer.WriteStartElementAsync(null, "element", null);
                            await writer.WriteAttributeStringAsync(null, "key", null, ListsArray[i].ElementAt(y).ElementAt(0).Key);
                            await writer.WriteAttributeStringAsync(null, "value", null, ListsArray[i].ElementAt(y).ElementAt(0).Value.ToString());
                            await writer.WriteEndElementAsync();
                        }
                        await writer.WriteEndElementAsync();
                    }
                }
            }
            SaveButton.SetActive(true);
        }
        catch (Exception) { SaveButton.SetActive(true); }
        
    }

    private void InitializeListWithTestValues()
    {
        for (int y = 0; y < ListsArray.Length; y++)
        {
            ListsArray[y] = new List<Dictionary<string, int>>();
            for (int i = 0; i < 11; i++)
            {
                ListsArray[y].Add(new Dictionary<string, int>() { { "test_" + i.ToString(), i } });
            }
        }
        ListNames = new string[] { "List name 1", "List name 2" };
        UpdateUIListNames();
        UpdateUIListCount();
    }

    private void UpdateUIListNames()
    {
        for (int i = 0; i < ListTitles.Length; i++)
        {
            if (i < ListNames.Length)
            {
                ListTitles[i].GetComponent<Text>().text = ListNames[i];
            }
        }
    }

    private void UpdateUIListCount()
    {
        for (int y = 0; y < ListsArray.Length; y++)
        {
            if (y < ListElementCount.Length)
            {
                ListElementCount[y].GetComponent<Text>().text = ELEMENT_COUNT + ListsArray[y].Count.ToString();
            }
        }
    }

    //Fill listview with data from lists
    private void RenderListUI()
    {
        for (int i = 0; i < ListsContainer.Length; i++)
        {
            foreach (Transform child in ListsContainer[i].transform)
            {
                Destroy(child.gameObject);
            }
        }
        for (int i = 0; i < ListsArray.Length; i++)
        {
            for (int y = 0; y < ListsArray[i].Count; y++)
            {
                GameObject panel = Instantiate(Resources.Load("Prefabs/ListElement", typeof(GameObject))) as GameObject;
                panel.name = y.ToString();
                panel.transform.SetParent(ListsContainer[i].transform);
                panel.transform.GetChild(0).GetComponent<Text>().text = ListsArray[i].ElementAt(y).ElementAt(0).Key + " " + ListsArray[i].ElementAt(y).ElementAt(0).Value.ToString();
                panel.GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }
        RsizeScrollList();
    }

    //Change size of the ScrollList content to math panels count
    public void RsizeScrollList()
    {
        for (int i = 0; i < ListsContainer.Length; i++)
        {
            if (ListsArray[i].Count == 0)
            {
                ListsContainer[i].GetComponent<RectTransform>().offsetMin = new Vector2(ListsContainer[i].GetComponent<RectTransform>().offsetMin.x, (1000 * -1));
            }
            else
            {
                ListsContainer[i].GetComponent<RectTransform>().offsetMin = new Vector2(ListsContainer[i].GetComponent<RectTransform>().offsetMin.x, (120 * ListsArray[i].Count) * -1);
            }
        }
    }

    //Make change to dictionaris due UI change
    public bool ListChangedByUI(string listFrom,string listTo, int indexFrom, int indexTo)
    {
        int listFromIndex = GetListIndexFromName(listFrom);
        int listToIndex = GetListIndexFromName(listTo);
        if (listFromIndex < 0 || listToIndex < 0)
        {
            return false;
        }
        
        if (!AddItemToList(listFromIndex, indexFrom, listToIndex, indexTo))
        {
            return false;
        }

        if (listFromIndex == listToIndex)
        {
            if (!RemoveItemToList(listFromIndex, indexFrom,indexTo,true))
            {
                return false;
            }
        }else
        {
            if (!RemoveItemToList(listFromIndex, indexFrom))
            {
                return false;
            }
        }

        UpdateUIListCount();
        RsizeScrollList();
        return true;

    }

    private bool AddItemToList(int listFromIndex,int posIndex, int listToindex , int posToIndex)
    {
        try
        {
            ListsArray[listToindex].Insert(posToIndex, ListsArray[listFromIndex].ElementAt(posIndex));
        }
        catch (Exception){return false;}
        return true;
    }

    private bool RemoveItemToList(int listFromIndex, int posIndex, int posToIndex = 0 ,bool isSameList = false)
    {
        int indexCorr = 0;

        if (isSameList&& posToIndex < posIndex)
        {
            indexCorr = 1;
        }

        try
        {
            ListsArray[listFromIndex].RemoveAt(posIndex + indexCorr);
        }
        catch{return false;}

        return true;
        
    }

    //Get List index from gameobject name
    private int GetListIndexFromName(string name) {
        string[] temp = name.Split('_');
        if (temp.Length < 2)
        {
            return -1;
        }
        try
        {
            return int.Parse(temp[1]);
        }catch (Exception)
        {
            return -1;
        }
    }

}
