using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * 시작화면(0번씬) 
 */

public class UIstart : MonoBehaviour
{
    void Start(){
        
       
        
        //가로화면으로 고정
        Screen.orientation=ScreenOrientation.LandscapeLeft;

        //1280 x 720 해상도로 조정
        Screen.SetResolution(1280, 720, true);

    }

    //Start 버튼 
    public void BtnStartOnClick()
    {
        SceneManager.LoadScene("1_main_scene");
    }


    //Load 버튼
    public void BtnLoadOnClick()
    {

    }


    //Exit 버튼
    public void BtnExitOnClick()
    {
        Application.Quit();
    }
}
