using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WPMF;
/*
 * 세계 창
 */
public class UIWorld : MonoBehaviour
{
    
    public GameObject world_window;
    public Text world_window_content;
    public GameObject main;
    WorldMap2D world;

    void Awake()
    {
        world = main.GetComponent<Game>().map;
        world_window_content.supportRichText = true;
    }

    //세계 창 내용을 1초마다 업데이트
    public void InvokeContentUpdate(){
        
        InvokeRepeating("ContentUpdate", 0.0f, 1.0f);
    }

    //세계 창 내용을 업데이트
    void ContentUpdate(){
       world_window_content.text = main.GetComponent<Game>().WorldDataToString();       
    }

    //세계 창 닫기버튼
    public void BtnCloseOnClick()
    {
        CancelInvoke();
        world_window_content.text = "";
        world_window.SetActive(false);
        
    }


}
