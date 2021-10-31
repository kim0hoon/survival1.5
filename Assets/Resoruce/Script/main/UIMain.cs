using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using WPMF;

/*
 * 메인화면UI
 */
public class UIMain : MonoBehaviour
{
    public GameObject news_window;

    public GameObject world_window;

    public GameObject country_window;

    public GameObject policy_window;

    public GameObject quest_window;

    public GameObject Main;
    public Button country_btn;
    public Button policy_btn;
    public TextMeshProUGUI date;

    //하단 온도 슬라이더 바
    public Slider temperatureSlider;
    public Image tempSliderImage;
    public Text tempSliderText;

    //상단 상태 표시 창
    public TextMeshProUGUI selected_country;//선택된 국가명 혹은 World
   
    public GameObject world_info_panel;//어떤 국가도 선택되지 않았을 시 활성화
    public TextMeshProUGUI gold_text1;//골드
    public TextMeshProUGUI wpop_text;//세계 인구
    public TextMeshProUGUI carbon_text;//세계 탄소 농도
    public TextMeshProUGUI temperature_text;//세계 평균 기온

    public GameObject country_info_panel;//국가 선택 시 활성화
    public TextMeshProUGUI gold_text2;//골드
    public TextMeshProUGUI pop_text;//국가 내 인구
    public TextMeshProUGUI energy_text;//에너지(수요/공급)
    public TextMeshProUGUI supRate_text;//지지율
    public TextMeshProUGUI eduRate_text;//인식률

    Game game;
    int h;
    void Awake()
    {
        game = Main.GetComponent<Game>();
        h = -2;
        news_window.SetActive(false);
        world_window.SetActive(false);
        country_window.SetActive(false);
        policy_window.SetActive(false);
        quest_window.SetActive(false);
        gold_text1.richText = true;
        wpop_text.richText = true;
        carbon_text.richText = true;
        temperature_text.richText = true;
        gold_text2.richText = true;
        pop_text.richText = true;
        energy_text.richText = true;
        supRate_text.richText = true;
        eduRate_text.richText = true;

    }


    void Update()
    {
        //국가 선택 시 활성화
        country_btn.interactable = (game.map.countryHighlighted != null);
        policy_btn.interactable = (game.map.countryHighlighted != null);


        if (h != game.map.countryHighlightedIndex)
        {
            game.UIMainUpdate();
            if (game.map.countryHighlightedIndex == -1)
            {
                selected_country.text = "WORLD";

                country_info_panel.SetActive(false);
                world_info_panel.SetActive(true);

            }
            else
            {
                selected_country.text = game.map.countryHighlighted.name;

                country_info_panel.SetActive(true);
                world_info_panel.SetActive(false);
            }
            
        }
        h = game.map.countryHighlightedIndex;
    }
   

    //가속버튼
    public void BtnFasterOnClick()
    {
       
    }



    //일시정지(?)버튼
    public void BtnStopOnClick()
    {
      
    }


    //exit 버튼
    public void BtnExitOnClick()
    {
        
        game.OnApplicationQuit();//게임을 종료시키고 시작화면으로 돌아감
        SceneManager.LoadScene("0_start_scene");
    }
     //save 버튼
    public void BtnSaveOnClick()
    {
       
    }



    //news 버튼
    public void BtnNewsOnClick()
    {
        news_window.SetActive(true);
        
    }


    //quest 버튼
    public void BtnQuestOnClick()
    {
        quest_window.SetActive(true);
    }



    //policy 버튼
    public void BtnPolicyOnClick()
   {
        gameObject.GetComponent<UIPolicy>().BtnPolicyItemOnClick(gameObject.GetComponent<UIPolicy>().main_item_set);
        gameObject.GetComponent<UIPolicy>().selectedCountryNum = game.map.countryHighlightedIndex;
        policy_window.SetActive(true);
   }



    //country 버튼
    public void BtnCoutnryOnClick()
   {

        country_window.SetActive(true);
       
        gameObject.GetComponent<UICountry>().BtnCountryItemOnClick(0);
        gameObject.GetComponent<UICountry>().selectedCountryNum = game.map.countryHighlightedIndex;
    }



    //world 버튼
    public void BtnWorldOnClick()
   {
        world_window.SetActive(true);
        gameObject.GetComponent<UIWorld>().InvokeContentUpdate();
   }
  
}
