using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
/*
 * 퀘스트 창
 */
public class UIQuest : MonoBehaviour
{
    public GameObject main;
    Game game;

    public GameObject quest_window;

    //퀘스트 선택패널
    public GameObject quest_select_list;//퀘스트 선택 패널
    public GameObject quest_prefab;
    float btn_quest_height;


    //퀘스트 내용창
    public GameObject quest_content_window;//퀘스트 선택 시 내용을 띄워주는 창
    public Text quest_content_title;//퀘스트 선택시 내용 창의 제목
    public Text quest_content_text;//퀘스트 선택시 내용 창의 퀘스트 내용
    public Button btn_quest_content_complete;//퀘스트 선택시 내용 창의 제목

    public Quest selectedQuest;//현재 선택된 퀘스트
    private int QUEST_CATEGORY_NUM = 4;//퀘스트 종류

  
    private const string libName = "SV_Simulator_v1";

    [DllImport(libName)]
    public static extern int RewardGold(int _gold);

    [DllImport(libName)]
    public static extern int GetFirePlants(int _countryCode);

    [DllImport(libName)]
    public static extern int GetGreenPlants(int _countryCode);

    [DllImport(libName)]
    public static extern int GetCountEduPolicy(int _countryCode, int _eduCode);

    [DllImport(libName)]
    public static extern int GetCountLifePolicy(int _countryCode, int _lifeCode);

    [DllImport(libName)]
    public static extern int GetRecognition(int _countryCode);

    [DllImport(libName)]
    public static extern int GetSupplyEnergy(int _countryCode);
    void Awake()
    {

        btn_quest_height = quest_prefab.GetComponent<RectTransform>().rect.height;
        quest_content_text.supportRichText = true;
        BtnQuestContentCloseOnClick();
        selectedQuest = null;
        game = main.GetComponent<Game>();
    }
  

    //quest_window_panel 닫기버튼
    public void BtnCloseOnClick()
    {
        quest_window.SetActive(false);
    }

    //quest_window_panel의 quest_select_panel에서 퀘스트를 선택(터치)할 경우 퀘스트 내용창을 활성화
    public void BtnQuestOnClick(Quest quest, GameObject target)
    {
        selectedQuest = quest;
        quest_content_title.text = quest.name;
        QuestUpdate(quest);
        quest_content_text.transform.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, quest_content_text.preferredHeight);
        btn_quest_content_complete.transform.GetComponent<Button>().onClick.AddListener(delegate () { BtnQuestContentCompleteOnClick(quest,target); });
        quest_content_window.SetActive(true);

    }

    //퀘스트 진행도를 업데이트(Game 스크립트에서 업데이트 될 때마다 호출)
    public void QuestUpdate(Quest quest)
    {
        int progress = -1;
        string richColorText = "";
        switch (quest.parameternum)
        {
            case 0:
                progress = quest.zeroIntval();
                break;
            case 1:
                progress = quest.oneIntval(quest.p1);
                break;
            case 2:
                progress = quest.twoIntval(quest.p1, quest.p2);
                break;
            default:
                break;
        }
        if (progress * quest.f >= quest.needInt * quest.f)
        {
            btn_quest_content_complete.interactable = true;
            richColorText = "<color=green>" + progress + "/" + quest.needInt + "</color>(완료가능)";
        }
        else
        {
            btn_quest_content_complete.interactable = false;
            richColorText = "<color=red>" + progress + "/" + quest.needInt + "</color>";
        }

        quest_content_text.text = quest.content + "\n" + richColorText;
    }

    //quest_content_window 퀘스트 완료 버튼
    public void BtnQuestContentCompleteOnClick(Quest quest,GameObject target)
    {
        
        RewardGold(quest.reward);//골드 보상
        Destroy(target);
        BtnQuestContentCloseOnClick();
    }

   
   

    //quest_content_widnow 닫기 버튼
    public void BtnQuestContentCloseOnClick()
    {
        selectedQuest = null;
        btn_quest_content_complete.transform.GetComponent<Button>().onClick.RemoveAllListeners();
        quest_content_window.SetActive(false);
    }

    //퀘스트 추가
    public void AddQuest(Quest quest)
    {
        GameObject btn_quest = Instantiate(quest_prefab);
      
        btn_quest.transform.SetParent(quest_select_list.transform, false);
      
        btn_quest.transform.GetChild(0).GetComponent<Text>().text = quest.name;

        btn_quest.transform.GetComponent<Button>().onClick.AddListener(delegate () { BtnQuestOnClick(quest, btn_quest); });

    }


    //게임 시작 시 퀘스트 추가(AddQuest 호출)
    public void questInit(int questNum, int countryNum)
    {
       
        for(int i=1; i<=questNum; i++)
        {
            int c = Random.Range(0, countryNum);//랜덤한 국가
            int k;
            int category = Random.Range(0, QUEST_CATEGORY_NUM);//랜덤한 퀘스트 종류

            int rewardGold;
            int needNum;
            string name=i+". "+ game.map.countries[c].name;
            string content="";
            Quest q=null;
            switch (category)
            {
                case 0:
                    rewardGold = i * 30;
                    needNum = i * 5;
                    name += "의 재생에너지 발전소" + needNum + "개 추가건설";
                    
                    content = "보상 : " + rewardGold + "골드";
                    q = new Quest(name, content, game.startcountryval[c].GreenPlants + needNum, 1, GetGreenPlants, c, rewardGold);
                    break;
                case 1:
                    k = Random.Range(0, 3);//교육정책 중 랜덤한 정책
                    rewardGold = i * 10 * (k + 1);
                    needNum = i * 3;
                   
                    name +="의 "+(k+1)+ "번 교육정책" + needNum + "번 시행";
                    
                    content = "보상 : " + rewardGold + "골드";
                    q = new Quest(name, content, needNum, 1, GetCountEduPolicy, c, k, rewardGold);
                    break;
                case 2:
                    k = Random.Range(0, 3);//생활정책 중 랜덤한 정책
                    rewardGold = i * 10 * (k + 1);
                    needNum = i * 3;
                   
                    name +=  "의 " + (k + 1) + "번 생활정책" + needNum + "번 시행";
                    
                    content = "보상 : " + rewardGold + "골드";
                    q = new Quest(name, content, needNum, 1, GetCountLifePolicy, c, k, rewardGold);
                    break;
                case 3:
                    rewardGold = i * 20;
                    needNum = i * 40;
                    name += "의 인식률 " + needNum + "만큼 상승시키기";
                    
                    content = "보상 : " + rewardGold + "골드";

                    q = new Quest(name, content, game.startcountryval[c].Recognition + needNum, 1, GetRecognition, c, rewardGold);
                    break;
            }
            if (q != null)
            {
                AddQuest(q);
            }
        }
    }
}
