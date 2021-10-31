using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
/*
 * 정책 창
 */
public class UIPolicy : MonoBehaviour
{
  

    public GameObject policy_window;//정책 창
    public GameObject main;
    Game game;

    //정책 선택
    public GameObject policy_item_list;//정책 항목 선택 패널
    public GameObject btn_policy_item_prefab;//정책 항목 혹은 정책 선택 버튼 prefab
    float policy_item_height;//btn_policy_item_prefab의 height
    public GameObject policy_item_panel_prefab;//세부항목 표시 패널 prefab

    //정책 조절
    public GameObject policy_content_window;//정책 내용 및 조절기능 창
    public Text policy_content_title;//정책 내용 창의 이름 텍스트
    public Text policy_content_text;//정책 내용 창의 정책내용 텍스트
    public Button policy_custom_btn_close;//닫기버튼
    public Button policy_custom_btn_enforce;//시행버튼
    public Button policy_custom_btn_reset;//초기화버튼
    public Text policy_custom_alarm;//정책 실패 시 알림
    public Slider policy_custom_slider;//정책 조절 슬라이더
    public Text policy_custom_controltext;//정책 조절 텍스트(on/off일 경우)
    public Text policy_custom_info;//국가 정보 및 변하는 정도 표시
    public Text policy_custom_slider_min;
    public Text policy_custom_slider_max;
    public Button btn_policy_slider_down;
    public Button btn_policy_slider_up;
   



    public Policy_set main_item_set; //최상위 root Policy_set
    public int selectedCountryNum;


    //정책 하위항목을 List에 저장하는 클래스
    public class Policy_set
    {
        public GameObject panel;
        public List<Policy_set> pl;
        public int depth;
        public Policy_set(List<Policy_set> pl,int depth)
        {
            this.panel = null;
            this.pl = pl;
            this.depth = depth;
        }
        public Policy_set(GameObject panel,List<Policy_set> pl,int depth)
        {
            this.panel = panel;
            this.pl = pl;
            this.depth = depth;
        }
    }
    
    private const string libName= "SV_Simulator_v1";
    
    [DllImport(libName)]
    public static extern int GetPTGold();

    [DllImport(libName)]
    public static extern int GetGreenPlants(int _countryCode);

    [DllImport(libName)]
    public static extern int GetFirePlants(int _countryCode);

    //policy
    //1.에너지
    [DllImport(libName)]
    public static extern int BuildFirePlants(int _countryCode, int _numBuild);

    [DllImport(libName)]
    public static extern int DestroyFirePlants(int _countryCode, int _numDestory);

    [DllImport(libName)]
    public static extern int GetCostFirePlants();

    [DllImport(libName)]
    public static extern int GetRefundFirePlants();

    [DllImport(libName)]
    public static extern int GetEmissionFirePlants();

    [DllImport(libName)]
    public static extern int GetSupplyFirePlants();

    [DllImport(libName)]
    public static extern int BuildGreenPlants(int _countryCode, int _numBuild);

    [DllImport(libName)]
    public static extern int DestroyGreenPlants(int _countryCode, int _numDestory);

    [DllImport(libName)]
    public static extern int GetCostGreenPlants();

    [DllImport(libName)]
    public static extern int GetRefundGreenPlants();

    [DllImport(libName)]
    public static extern int GetEmissionGreenPlants();

    [DllImport(libName)]
    public static extern int GetSupplyGreenPlants();

    //2. 교육
    [DllImport(libName)]
    public static extern int EnforceEduPolicy(int _countryCode, int _eduCode);

    [DllImport(libName)]
    public static extern int GetCountEduPolicy(int _countryCode, int _eduCode);

    [DllImport(libName)]
    public static extern int GetCostEduPolicy(int _eduCode);

    [DllImport(libName)]
    public static extern int GetEffectEduPolicy(int _eduCode);

    //3.생활
    [DllImport(libName)]
    public static extern int EnforceLifePolicy(int _countryCode, int _lifeCode);

    [DllImport(libName)]
    public static extern int GetCountLifePolicy(int _countryCode, int _lifeCode);

    [DllImport(libName)]
    public static extern int GetCostLifePolicy(int _lifeCode);

    [DllImport(libName)]
    public static extern int GetEffectLifePolicy(int _lifeCode);

    [DllImport(libName)]
    public static extern int GetNeedRecognition(int _lifeCode);
    //
    


    void Awake()
    {
        selectedCountryNum= -1;
        policy_item_height = btn_policy_item_prefab.GetComponent<RectTransform>().rect.height;
        policy_item_list.SetActive(false);
        main_item_set = new Policy_set(policy_item_list, new List<Policy_set>(),0);
       
        policy_content_window.SetActive(false);
        policy_custom_info.supportRichText = true;
        
       
    }
    void Start()
    {

        game = main.GetComponent<Game>();

  
        PolicyInit();


        BtnPolicyContentCloseOnClick();
    }
    

    //policy_window 닫기버튼
    public void BtnCloseOnClick()
    {
        
        policy_window.SetActive(false);
        BtnPolicyItemOnClick(main_item_set);
    }

    //정책을 선택(터치)할 경우 정책 내용 창을 활성화
    public void BtnPolicyOnClick(Policy policy)
    {
        policy_content_title.text = policy.name;
        policy_content_text.text = policy.content;
        policy_content_text.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, policy_content_text.preferredHeight);
        


        switch (policy.type)
        {
            case 0:// 에너지
                policy_custom_btn_reset.gameObject.SetActive(true);
                policy_custom_btn_enforce.gameObject.SetActive(true);
                policy_custom_slider.gameObject.SetActive(true);

                policy_custom_btn_enforce.onClick.AddListener(delegate () { BtnPolicyContentEnforceOnClick(policy); });
                policy_custom_btn_reset.onClick.AddListener(delegate () { BtnPolicyContentResetOnClick(policy); });
                policy_custom_slider.onValueChanged.AddListener(delegate { SliderPolicy(policy); });
                policy_custom_slider.wholeNumbers = true;
                
                
         
                policy_custom_info.text = "현재 값 : "+policy_custom_slider.value.ToString();
                policy_custom_slider.minValue = 0;
                policy_custom_slider_min.text="0";
                policy_custom_slider.maxValue = policy.nval(selectedCountryNum)+50;
                policy_custom_slider_max.text = policy_custom_slider.maxValue.ToString();
                
                btn_policy_slider_down.transform.GetChild(0).GetComponent<Text>().text ="-"+policy.d;
                btn_policy_slider_up.transform.GetChild(0).GetComponent<Text>().text = "+" + policy.d;
                btn_policy_slider_down.onClick.AddListener(delegate () { BtnPolicyValueControlOnClick(-policy.d); });
                btn_policy_slider_up.onClick.AddListener(delegate () { BtnPolicyValueControlOnClick(policy.d); });

                
                break;

            case 1://교육
            case 2://생활
           
                policy_custom_btn_enforce.gameObject.SetActive(true);
                policy_custom_controltext.gameObject.SetActive(true);
                Text t = policy_custom_btn_enforce.gameObject.transform.GetChild(0).transform.GetComponent<Text>();
                t.text = "시행";
                policy_custom_btn_enforce.onClick.AddListener(delegate () { BtnPolicyContentEnforceOnClick(policy); });
              
                break;
            default:
                Debug.Log("Error : BtnPolicyOnClick policy type error");
                break;

        }

        BtnPolicyContentResetOnClick(policy);
        policy_custom_info.gameObject.SetActive(true);
        policy_custom_btn_close.gameObject.SetActive(true);
        policy_content_window.SetActive(true);

    }

    //에너지 정책에서 + 혹은 - 버튼을 터치할 경우 슬라이더 값 d만큼 증감
    public void BtnPolicyValueControlOnClick(float d)
    {
        policy_custom_slider.value += d;
    }

    //초기화 버튼 터치 시 정책 내용 창에서 현재 값으로 초기화
    public void BtnPolicyContentResetOnClick(Policy policy)
    {
        
        switch (policy.type)
        {
            case 0:
        
                policy_custom_slider.maxValue = policy.nval(selectedCountryNum) + 50;
                policy_custom_slider_max.text = policy_custom_slider.maxValue.ToString();
                policy_custom_slider.value = policy.nval(selectedCountryNum);
                SliderPolicy(policy);
                break;
            case 1:
                policy_custom_controltext.text = "현재 " + policy.count(selectedCountryNum, policy.code) + "번 실행했습니다.";
                policy_custom_info.text = "골드 : <color=red>-"+policy.costf(policy.code)+ "</color>\n인식률 : <color=green>+" + policy.effect(policy.code)+ "</color>";
                break;
            case 2:
                policy_custom_controltext.text = "현재 " + policy.count(selectedCountryNum, policy.code) + "번 실행했습니다.";
                policy_custom_info.text = "골드 : <color=red>-" + policy.costf(policy.code) + "</color>\n에너지 수요 : <color=green>-" + policy.effect(policy.code) + "</color>\n" +
                    "필요 인식률 : <color=yellow>" + policy.needRecognition(policy.code) + "</color>\n";
                
                break;
            default:
                Debug.Log("Error : BtnPolicyContentResetOnClick policy type error");
                break;
        }
        
       
    
    }

    //정책 시행 버튼
    public void BtnPolicyContentEnforceOnClick(Policy policy)
    {
        int ret;
        switch (policy.type) {
            case 0:
                if(policy_custom_slider.value >= policy.nval(selectedCountryNum)){
                    ret=policy.build(selectedCountryNum, (int)policy_custom_slider.value -policy.nval(selectedCountryNum));                
                    if (ret == 0)
                    {
                        policy_custom_alarm.text = "<color=green>정책이 시행되었습니다.</color>";
                    }
                    else if (ret == -1)
                    {
                        policy_custom_alarm.text = "<color=red>골드가 부족합니다.</color>";
                    }
                }
                else
                {
                    ret=policy.destroy(selectedCountryNum, policy.nval(selectedCountryNum)- (int)policy_custom_slider.value);
                    if (ret == 0)
                    {
                        policy_custom_alarm.text = "<color=green>정책이 시행되었습니다.</color>";
                    }
                    else if (ret == -1)
                    {
                        Debug.Log("발전소 폐쇄오류");
                    }
                }
               
                break;
            case 1:
            case 2:
                ret = policy.enforce(selectedCountryNum, policy.code);

                if (ret == 0)
                {
                    policy_custom_alarm.text = "<color=green>정책이 시행되었습니다.</color>";
                }
                else if (ret == -1)
                {
                    policy_custom_alarm.text = "<color=red>골드가 부족합니다.</color>";
                }
                else if (ret == -2)
                {
                    Debug.Log("Error : policy.enforce array index out of bound");
                }else if(ret == -3)
                {
                    policy_custom_alarm.text = "<color=red>인식률이 부족합니다.</color>";
                }

                break;
            default:
                Debug.Log("Error : BtnPolicyContentEnforceOnClick policy type error");
                break;           
        }
        BtnPolicyContentResetOnClick(policy);
        game.UIMainUpdate();
    }
    
    //정책 내용창 닫기 버튼
    public void BtnPolicyContentCloseOnClick()
    {
        policy_custom_btn_enforce.onClick.RemoveAllListeners();
        policy_custom_btn_reset.onClick.RemoveAllListeners();
        policy_custom_slider.onValueChanged.RemoveAllListeners();
        btn_policy_slider_down.onClick.RemoveAllListeners();
        btn_policy_slider_up.onClick.RemoveAllListeners();

        policy_custom_btn_close.gameObject.SetActive(false);
        policy_custom_btn_enforce.gameObject.SetActive(false);
        policy_custom_btn_reset.gameObject.SetActive(false);
        policy_custom_slider.gameObject.SetActive(false);
        policy_custom_controltext.gameObject.SetActive(false);
        policy_custom_info.gameObject.SetActive(false);
        policy_content_window.SetActive(false);
    }

    //에너지 정책에서 슬라이더 값을 변화시킬 경우 시행 시 변화하는 값 업데이트
    public void SliderPolicy(Policy policy)
    {
        string richColor = "";
        policy_custom_slider_max.text = policy_custom_slider.maxValue.ToString();
        

        if(policy_custom_slider.value >= policy.nval(selectedCountryNum)){
            policy_custom_info.text = "현재 값 : " + policy_custom_slider.value+ "(<color=green>+" + (policy_custom_slider.value - policy.nval(selectedCountryNum)) +")</color>";
            policy_custom_info.text += "\n골드 : <color=red>-" + policy.cost * (policy_custom_slider.value - policy.nval(selectedCountryNum)) + "</color>";
            policy_custom_info.text += "\n에너지 공급량 : <color=green>+" + policy.supply * (policy_custom_slider.value - policy.nval(selectedCountryNum)) + "</color>";
            policy_custom_info.text += "\n탄소 배출량 : <color=red>+" + policy.emission * (policy_custom_slider.value - policy.nval(selectedCountryNum)) + "</color>";
        }
        else
        {
            policy_custom_info.text = "현재 값 : " + policy_custom_slider.value + "(<color=red>" + (policy_custom_slider.value - policy.nval(selectedCountryNum)) + ")</color>";
            policy_custom_info.text += "\n골드 : <color=green>+" + policy.refund * (policy.nval(selectedCountryNum)-policy_custom_slider.value) + "</color>";
            policy_custom_info.text += "\n에너지 공급량 : <color=red>-" + policy.supply * (policy.nval(selectedCountryNum) - policy_custom_slider.value) + "</color>";
            policy_custom_info.text += "\n탄소 배출량 : <color=green>-" + policy.emission * (policy.nval(selectedCountryNum) - policy_custom_slider.value) + "</color>";
        }
    }




    //정책항목의 항목 선택(터치)시 패널을 확장하고 세부내용 활성화
    public void BtnPolicyItemOnClick(Policy_set policy_set)
    {
       
        float size = 0.0f;
        if (policy_set.panel.activeSelf)
        {
            size = -policy_set.panel.GetComponent<RectTransform>().rect.height;
            policy_set.panel.SetActive(false);
            AllChildSetOff(policy_set);
        }
        else
        {
            size = policy_set.pl.Count * policy_item_height;
            policy_set.panel.SetActive(true);
        }
        SetPolicyItemSize(policy_set.panel, size);
    }


    //policy_item_list까지 모든 부모 패널의 사이즈를 조정함
    public void SetPolicyItemSize(GameObject panel,float size)
    {

        panel.GetComponent<RectTransform>().sizeDelta += new Vector2(0, size);
        if(panel!=policy_item_list)
        {
            SetPolicyItemSize(panel.transform.parent.gameObject, size);
        }
    }
    
    
    //모든 자식을 DFS탐색하여 off시키고 패널크기를 0으로 만듦
    public void AllChildSetOff(Policy_set policy_set)
    {
        foreach (Policy_set item in policy_set.pl)
        {
            if (item != null)
            {
                AllChildSetOff(item);
                item.panel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                item.panel.SetActive(false);
            }
        }
    }

    //정책항목을 만드는 함수 MakePolicyItem(policy_set : 부모노드 ,btn_name : 정책(혹은 항목)이름, policy : item이 항목일 경우 null값 or 정책일 경우 policy 인스턴스를 넣어줌) 
    public void MakePolicyItem(Policy_set policy_set,string btn_name,Policy policy)
    {
        GameObject btn_policy = Instantiate(btn_policy_item_prefab);
        btn_policy.transform.SetParent(policy_set.panel.transform, false);
        btn_policy.SetActive(true);
        string s=btn_name;
       
        
        btn_policy.transform.GetChild(0).GetComponent<Text>().text = s.PadLeft(s.Length+4*policy_set.depth);
       
        Policy_set new_set=null;

        if (policy == null)
        {
            GameObject policy_item_panel = Instantiate(policy_item_panel_prefab);
            policy_item_panel.transform.SetParent(policy_set.panel.transform, false);
            policy_item_panel.SetActive(false);
            new_set = new Policy_set(policy_item_panel, new List<Policy_set>(),policy_set.depth+1);
            btn_policy.transform.GetComponent<Button>().onClick.AddListener(delegate () { BtnPolicyItemOnClick(new_set); });

        }
        else
        {
            btn_policy.transform.GetComponent<Button>().onClick.AddListener(delegate () { BtnPolicyOnClick(policy); });
        }
        

        policy_set.pl.Add(new_set);
       
    }


    //게임 시작 시 정책 추가
    void PolicyInit()
    {
        //MakePolicyItem(policy_set : 부모노드 ,btn_name : 정책(혹은 항목)이름, policy : item이 항목일 경우 null값 or 정책일 경우 policy 인스턴스를 넣어줌) 
        Policy p;
        string n = "";
        string c = "";

        //1.에너지
        MakePolicyItem(main_item_set, "1. 에너지", null);//에너지 항목

        n = "·[에너지] 화력발전소 수 조절";
        c = "비교적 저렴하게 에너지 발전이 가능한 석탄화력발전소를 조절합니다. 단, 그 대가는 결코 저렴하지 않습니다.\n 폐쇄할 경우 탄소배출량을 감축되지만 부족한 에너지량은 보충해야 함을 주의하세요.\n 인식률이 높을 경우 일정확률로 지지도가 감소합니다.\n개당 건설 비용 : " + GetCostFirePlants() + "\n개당 반환 비용 : " + GetRefundFirePlants() + "\n개당 탄소배출량 : " + GetEmissionFirePlants() +
           "\n개당 에너지 공급량 : " + GetSupplyFirePlants();
        p = new Policy(0, n, c, 1.0f, GetCostFirePlants(), GetRefundFirePlants(), GetSupplyFirePlants(), GetEmissionFirePlants(), GetFirePlants, BuildFirePlants, DestroyFirePlants);
        MakePolicyItem(main_item_set.pl[0], p.name, p);

        n = "·[에너지] 재생 에너지 발전소 수 조절";
        c = "탄소배출량이 0에 수렴하는 재생에너지 발전소를 조절합니다. 장기적으로 환경에 매우 이로운 선택입니다.\n 폐쇄할 경우 부족한 에너지량은 보충해야 함을 주의하세요.\n 인식률이 낮을 경우 일정확률로 지지도가 감소합니다\n개당 건설 비용 : " + GetCostGreenPlants() + "\n개당 반환 비용 : " + GetRefundGreenPlants() + "\n개당 탄소배출량 : " + GetEmissionGreenPlants() +
            "\n개당 에너지 공급량 : " + GetSupplyGreenPlants();
        p = new Policy(0, n, c, 1.0f, GetCostGreenPlants(), GetRefundGreenPlants(), GetSupplyGreenPlants(), GetEmissionGreenPlants(), GetGreenPlants, BuildGreenPlants, DestroyGreenPlants);
        MakePolicyItem(main_item_set.pl[0], p.name, p);


        //2.교육
        MakePolicyItem(main_item_set, "2. 교육", null);//교육 항목

        n = "·[교육] 도서출판";
        c = "전문가들이 기후위기에 대한 서적들을 출간합니다. 기후위기의 심각성을 심도있게 알릴 수 있습니다.\n 기후위기에 대한 인식률이 증가합니다.";
        p = new Policy(1, 0, n, c, GetCountEduPolicy, GetCostEduPolicy, GetEffectEduPolicy, EnforceEduPolicy);
        MakePolicyItem(main_item_set.pl[1], p.name, p);

        n = "·[교육] 환경 캠페인";
        c = "유명인을 홍보대사로 위촉하여 많은 사람들이 기후위기 캠페인에 참여하도록 유도합니다.\n 기후위기에 대한 인식률이 증가합니다.";
        p = new Policy(1, 1, n, c, GetCountEduPolicy, GetCostEduPolicy, GetEffectEduPolicy, EnforceEduPolicy);
        MakePolicyItem(main_item_set.pl[1], p.name, p);

        n = "·[교육] 환경특집 방송 방영";
        c = "예능 프로그램에 환경 문제를 노출시키고 TV매체에 환경 다큐멘터리를 방영하여 기후위기의 심각성을 대중적으로 알릴 수 있습니다.\n 기후위기에 대한 인식률이 증가합니다.";
        p = new Policy(1, 2, n, c, GetCountEduPolicy, GetCostEduPolicy, GetEffectEduPolicy, EnforceEduPolicy);
        MakePolicyItem(main_item_set.pl[1], p.name, p);


        //3.생활
        MakePolicyItem(main_item_set, "3. 생활", null);//생활 항목

        n = "·[생활] 육류 생산 제한";
        c = "탄소배출이 심각한 육류 판매를 제한하기 위해 농축산 업체에 강력한 규제를 적용합니다.\n 에너지 수요량이 감소합니다. \n낮은 수준의 인식률이 필요합니다.";
        p = new Policy(2, 0, n, c, GetCountLifePolicy, GetCostLifePolicy, GetEffectLifePolicy, EnforceLifePolicy, GetNeedRecognition);
        MakePolicyItem(main_item_set.pl[2], p.name, p);

        n = "·[생활] 그린리모델링 의무화";
        c = "건물이 재생에너지로 자급자족하여 탄소가 배출되지 않도록 하는 그린 리모델링을 신규 건축 건물에 한하여 의무화 합니다.\n 에너지 수요량이 감소합니다. \n중간 수준의 인식률이 필요합니다.";
        p = new Policy(2, 1, n, c, GetCountLifePolicy, GetCostLifePolicy, GetEffectLifePolicy, EnforceLifePolicy, GetNeedRecognition);
        MakePolicyItem(main_item_set.pl[2], p.name, p);

        n = "·[생활] 탄소배출차량 판매금지";
        c = "탄소를 배출하는 차량은 전면 판매 금지합니다.\n 에너지 수요량이 감소합니다. \n높은 수준의 인식률이 필요합니다.";
        p = new Policy(2, 2, n, c, GetCountLifePolicy, GetCostLifePolicy, GetEffectLifePolicy, EnforceLifePolicy, GetNeedRecognition);
        MakePolicyItem(main_item_set.pl[2], p.name, p);
    }
    
}
