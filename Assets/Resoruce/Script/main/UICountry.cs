using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WPMF;
/*
 * 국가 창
 */
public class UICountry : MonoBehaviour
{
  
    public GameObject main;
    public GameObject country_window;
    WorldMap2D world;

    
    public Button btn_country_outline;
    public Button btn_country_energy;
    public Button btn_country_life;
    public Button btn_country_industry;

    public Text country_content_text;
    public int selectedCountryNum;
    int type;
    void Awake()
    {
        type = 0;
        world = main.GetComponent<Game>().map;
        selectedCountryNum = -1;
        country_content_text.supportRichText = true;
    }
   

    //국가 창의 항목 선택 시 1초마다 정보 업데이트
    public void BtnCountryItemOnClick(int t)
    {
        CancelInvoke("ContentUpdate");
        type = t;
        InvokeRepeating("ContentUpdate",0.0f,1.0f);
    }

    //국가 정보 업데이트
    void ContentUpdate()
    {
        
        switch (type)
        {
            case 0:
                country_content_text.text=world.countries[selectedCountryNum].name+"("+world.countries[selectedCountryNum].continent+")\n"+main.GetComponent<Game>().CountryOutlineToString(selectedCountryNum);
                break;
            case 1:
                country_content_text.text = main.GetComponent<Game>().CountryEnergyToString(selectedCountryNum);
                break;
             
            default:
                Debug.Log("UiCountry Type error");
                break;
        }
        country_content_text.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, country_content_text.preferredHeight);
      

    }
    //닫기 버튼
    public void BtnCloseOnClick()
    {
        CancelInvoke();
        country_window.SetActive(false);
        
    }

  
   

   
    
}
