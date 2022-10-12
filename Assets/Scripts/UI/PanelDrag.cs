using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelDrag : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas; //ListController
    private bool isRemovedFromUIList = false;//Is panel draging at the moment
    private Transform current_position; //Current panel's list
    private Transform new_position; //Destination list
    private int new_list_index; //New position of the panel
    private bool isNewPlaceFinded = false; //true if both, new list and new position avalible for panel
    private GameObject placeHolder;
    private int currentIndex; //Current panel's index
    private float smootheTimer=0.0f; //Timer to reduce drag calling rate
    // Start is called before the first frame update
    void Start()
    {
        //Assign gameobject with ListController
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

 

    public void DragHandler(BaseEventData baseEventData)
    {
        smootheTimer += Time.deltaTime;
        //Check if panel alredy draging, run once on drag
        if (!isRemovedFromUIList)
        {
            isRemovedFromUIList = true;
            current_position = this.transform.parent;      
            this.transform.SetParent(canvas.transform);
            try { currentIndex = int.Parse(this.gameObject.name); }
            catch (Exception) { DropHandler(baseEventData);return; }
        }
        //Transform position of the panel, and move it to mouse position in Canvas space
        PointerEventData pointerData = (PointerEventData)baseEventData;
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, pointerData.position, canvas.worldCamera, out position);
        this.transform.position = canvas.transform.TransformPoint(position);
        //Since Drag event calling every frame, we need to slow down it a little bit,
        //othervise it creates flickiring of placeholder and other lags
        if (smootheTimer < 0.02f)
        {
            return;
        }
        smootheTimer = 0.0f;
        //Getting UI elements under the pointer
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData,results);
        bool listFinded = false;
        bool panelFinded = false;
        foreach (var elem in results)
        {
            switch (elem.gameObject.tag)
            {
                case "ui_panel": //Geting data, over what panel the mouse are
                    {
                        int index;                    
                        try{index = int.Parse(elem.gameObject.name);}
                        catch (Exception) { panelFinded = false; break; }
                        //Check if under, above or on the middle of other panel to show placeholder
                        // in apropriate place
                        float positionDelta = this.transform.position.y - elem.gameObject.transform.position.y;
                        if (positionDelta > -0.1f && positionDelta < 0.1f)
                        {
                            panelFinded = false;
                            break;
                        }
                        if (positionDelta < -0.1f)
                        {
                            if (current_position.Equals(new_position))
                            {
                                new_list_index = index;
                            }
                            else
                            {
                                new_list_index = index + 1;
                            }                        

                        }
                        else if (positionDelta > 0.1f)
                        {
                            if (current_position.Equals(new_position))
                            {
                                new_list_index = (currentIndex < index) ? index - 1 : index;
                            }
                            else
                            {
                                new_list_index = index;
                            }
                           
                        }
                        panelFinded = true;
                        break;
                    }
                case "ui_list"://Getting data, over what list the mouse are
                    {
                        new_position = elem.gameObject.transform;
                        listFinded = true;
                        //If there is no panels in list, we set position of placeholder at first element
                        if (new_position.childCount == 0)
                        {
                            new_list_index = 0;
                            panelFinded = true;
                        }
                        break;
                    }
                case "ui_placeholder": //If we over placeholder, do noting
                    {
                        panelFinded = true;
                        break;
                    }
                default:{break;}
            }
        }
        if (listFinded && panelFinded)
        {
            isNewPlaceFinded = true;
            CreatePlaceHolder();
        }
        else
        {
            isNewPlaceFinded = false;
            DeletePlaceHolder();
        }
    }

    //Creating transparent gameobject at content of scrollview,
    //to create effect of empty space, when user drag panel to new place
    public void CreatePlaceHolder()
    {
        if (placeHolder == null)
        {
            placeHolder = Instantiate(Resources.Load("Prefabs/PlaceHolder", typeof(GameObject))) as GameObject;
            placeHolder.transform.SetParent(new_position);
            placeHolder.transform.SetSiblingIndex(new_list_index);
            placeHolder.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    public void DeletePlaceHolder()
    {
        GameObject.Destroy(placeHolder);
    }

    //Recreate names of panels of switching place,
    //it is allow it to stay consistent with lists
    public void reindex(List<Transform> ReindexList)
    {
        int step = 0;
        for (int i = 0; i < ReindexList.Count; i++)
        {
            foreach (Transform child in ReindexList.ElementAt(i))
            {
                if (child.tag == "ui_panel")
                {
                    child.name = step.ToString();
                    step++;
                }
            }
            step = 0;
        }
    }
    public void DropHandler(BaseEventData baseEventData)
    {
        isRemovedFromUIList = false;
        DeletePlaceHolder();
        if (isNewPlaceFinded)
        {
            this.transform.SetParent(new_position);
            this.transform.SetSiblingIndex(new_list_index);
            List<Transform> ReindexList = new List<Transform>();
            ReindexList.Add(current_position);
            if (!current_position.Equals(new_position))
            {
                ReindexList.Add(new_position);
            }
            reindex(ReindexList);

            //Set data to dictionaries
            if (!canvas.GetComponent<ListController>().ListChangedByUI(current_position.name,new_position.name,currentIndex,new_list_index))
            {
                this.transform.SetParent(current_position);
                this.transform.SetSiblingIndex(currentIndex);
            }
            
        }
        else
        {
            //No panel finded under the mouse, backup
            this.transform.SetParent(current_position);
            this.transform.SetSiblingIndex(currentIndex);
        }
        
    }
}

   
