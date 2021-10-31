using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WPMF;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UB.Simple2dWeatherEffects.Standard;

/*
 * Game
 */

public class Game : MonoBehaviour {
	

	public WorldMap2D map;
	public Player player;//Player 객체
	public GameObject UiObj;

	
	public GameObject MainCamera;

	public GameObject SmokeEffect;//스모그 효과 prefab
	

	//팝업 창
	public GameObject popup_panel;
	public Text popup_title;
	public Text popup_content;
	public Button popup_close_btn;


	//엔딩
	public GameObject EndingPanel;
	public GameObject EndingTextPanel;
	public Text EndingText;
	public GameObject ResultPanel;
	public Text ResultText;

	


	GUIStyle labelStyle, labelStyleShadow, buttonStyle, sliderStyle, sliderThumbStyle;
	ColorPicker colorPicker;
	bool changingFrontiersColor;
	bool minimizeState = false;

	//UI 인스턴스
	UIMain uimain;
	UICountry uicountry;
	UINews uinews;
	UIPolicy uipolicy;
	UIQuest uiquest;
	UIWorld uiworld;
	D2FogsPE fog;
	GameObject[] smokeeffect;

	private const int COUNTRY_NUM=10;//국가 수
	private const long DAY_CYCLE = 1000;//하루 주기(1000=1초)
	private const long ENDING_DEFEAT_DEADNUM = 0;//패배까지의 인구 수
	private const int ENDING_DEFEAT_SUPPORTNUM =5;//패배까지의 지지도
	private const int ENDING_VICTORY_DATE =2879; //승리까지의 날짜  (2027/12/30 = 2879)
	private const int QUEST_NUM = 7;//퀘스트 수
	
	int alarm = 0;//0.5도 상승치 경고 횟수

	int day;

	//연간 보고서
	public struct WorldReport
    {
        public float WTemperature;
        public float WCarbonPPM;
        public long WPopulation;
        public int PDGold;
        public int WDEmission;
        public int WTEmission;
        public int WNeedEnergy;
        public int WSupplyEnergy;
        public int PSupport;
        public int WRecognition;
    }
	public WorldReport worldreport;
	public WorldReport startworldval;

	//국가 주요 정보
	public struct CountryReport
    {
		public int Support;
		public int Recognition;
		public int FirePlants;
		public int GreenPlants;
		public int DEmission;
    }

	public CountryReport[] startcountryval;

	// 성능 test
	float deltaTime = 0.0f;
	public Text deltaTimeText;
	public Text opTimeText;
	float opTime;
	float timer = 0.0f;
	
	System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
	

	/*
		64bit dll = "SV_Simulator_v1x64"
		32bit dll = "SV_Simulator_v1"
	*/

	private const string libName = "SV_Simulator_v1"; //UIPolicy , UINews, UIQuest에서도 바꿔줘야 함
	//dll 선언부

	// interface 관련 외부함수
	[DllImport(libName)]
	public static extern int InitGame(long _cycle, int _debugMode);

	[DllImport(libName)]
	public static extern int PlayGame();

	[DllImport(libName)]
	public static extern int EndGame();

	[DllImport(libName)]
	public static extern int Pause();

	[DllImport(libName)]
	public static extern int Resume();

	[DllImport(libName)]
	public static extern int DoubleSpeed();
	[DllImport(libName)]
	public static extern int QuadSpeed();

	[DllImport(libName)]
	public static extern int OctoSpeed();

	[DllImport(libName)]
	public static extern int NormalSpeed();
	//

	// main 관련 외부함수
	[DllImport(libName)]
	public static extern int Today();

	[DllImport(libName)]
	public static extern float GetWTemperature();

	[DllImport(libName)]
	public static extern float GetWElevatedTemperature();

	[DllImport(libName)]
	public static extern float GetWCarbonPPM();

	[DllImport(libName)]
	public static extern int GetPTGold();

	[DllImport(libName)]
	public static extern int GetPDGold();

	[DllImport(libName)]
	public static extern int GetPSupport();

	[DllImport(libName)]
	public static extern int GetPDSupport();

	[DllImport(libName)]
	public static extern long GetWPopulation();

	[DllImport(libName)]
	public static extern long GetWLive();

	[DllImport(libName)]
	public static extern long GetWDead();

	[DllImport(libName)]
	public static extern long GetWDDead();

	[DllImport(libName)]
	public static extern int GetWDEmission();

	[DllImport(libName)]
	public static extern int GetWTEmission();

	[DllImport(libName)]
	public static extern int GetWNeedEnergy();

	[DllImport(libName)]
	public static extern int GetWDNeedEnergy();

	[DllImport(libName)]
	public static extern int GetWSupplyEnergy();

	[DllImport(libName)]
	public static extern int GetWDSupplyEnergy();

	[DllImport(libName)]
	public static extern int GetWRecognition();

	[DllImport(libName)]
	public static extern int GetWDRecognition();


	//

	// country 관련 외부함수
	[DllImport(libName)]
	public static extern long GetPopulation(int _countryCode);

	[DllImport(libName)]
	public static extern long GetLive(int _countryCode);

	[DllImport(libName)]
	public static extern long GetDead(int _countryCode);

	[DllImport(libName)]
	public static extern long GetDDead(int _countryCode);

	[DllImport(libName)]
	public static extern int GetDGold(int _countryCode);

	[DllImport(libName)]
	public static extern int GetTGold(int _countryCode);

	[DllImport(libName)]
	public static extern int GetDSupport(int _countryCode);

	[DllImport(libName)]
	public static extern int GetSupport(int _countryCode);

	[DllImport(libName)]
	public static extern int GetDRecognition(int _countryCode);

	[DllImport(libName)]
	public static extern int GetRecognition(int _countryCode);

	[DllImport(libName)]
	public static extern int GetNeedEnergy(int _countryCode);

	[DllImport(libName)]
	public static extern int GetDNeedEnergy(int _countryCode);

	[DllImport(libName)]
	public static extern int GetSupplyEnergy(int _countryCode);

	[DllImport(libName)]
	public static extern int GetDSupplyEnergy(int _countryCode);

	[DllImport(libName)]
	public static extern int GetFirePlants(int _countryCode);

	[DllImport(libName)]
	public static extern int GetDFirePlants(int _countryCode);

	[DllImport(libName)]
	public static extern int GetGreenPlants(int _countryCode);

	[DllImport(libName)]
	public static extern int GetDGreenPlants(int _countryCode);

	[DllImport(libName)]
	public static extern int GetDEmission(int _countryCode);

	[DllImport(libName)]
	public static extern int GetTEmission(int _countryCode);


	
	void Awake()
    {
		//1280 x 720 해상도로 고정
		Screen.SetResolution(1280, 720, true);


		//가로화면으로 고정
		Screen.orientation = ScreenOrientation.LandscapeLeft;

		map = WorldMap2D.instance;
		
		player = this.gameObject.AddComponent<Player>();
		uimain = UiObj.GetComponent<UIMain>();
		uicountry = UiObj.GetComponent<UICountry>();
		uinews = UiObj.GetComponent<UINews>();
		uipolicy = UiObj.GetComponent<UIPolicy>();
		uiquest = UiObj.GetComponent<UIQuest>();
		uiworld = UiObj.GetComponent<UIWorld>();
		fog = MainCamera.GetComponent<D2FogsPE>();
		day = 0;
		smokeeffect = new GameObject[COUNTRY_NUM];
		popup_close_btn.onClick.AddListener(BtnPopupCloseOnClick);
		popup_panel.SetActive(false);
		EndingPanel.SetActive(false);
	}
	void Start () {
		
		
		GameObject wmap = GameObject.Find("WorldMap2D");
		for (int i = 0; i < COUNTRY_NUM; i++)
		{
			smokeeffect[i] = Instantiate(SmokeEffect);
			
			smokeeffect[i].transform.SetParent(wmap.transform,false);
			smokeeffect[i].GetComponent<ParticleSystem>().playbackSpeed=0.2f;
			smokeeffect[i].transform.position =new Vector3(map.countries[i].center.x*wmap.transform.localScale.x,map.countries[i].center.y*wmap.transform.localScale.y,map.transform.position.z);
			
		}
		InitGame(DAY_CYCLE, 0);

		//세계 초기 값
		worldreport.WTemperature = GetWTemperature();
		worldreport.WCarbonPPM = GetWCarbonPPM();
		worldreport.WPopulation = GetWPopulation();
		worldreport.PDGold = GetPDGold();
		worldreport.WDEmission = GetWDEmission();
		worldreport.WTEmission = GetWTEmission();
		worldreport.WNeedEnergy = GetWNeedEnergy();
		worldreport.WSupplyEnergy = GetWSupplyEnergy();
		worldreport.PSupport = GetPSupport();
		worldreport.WRecognition = GetWRecognition();
		startworldval = worldreport;
		startcountryval = new CountryReport[COUNTRY_NUM];

		//국가 초기 값
		for(int i=0; i<COUNTRY_NUM; i++)
        {
			startcountryval[i].Support = GetSupport(i);
			startcountryval[i].Recognition = GetRecognition(i);
			startcountryval[i].FirePlants = GetFirePlants(i);
			startcountryval[i].GreenPlants = GetGreenPlants(i);
			startcountryval[i].DEmission = GetDEmission(i);
        }
		uiquest.questInit(QUEST_NUM, COUNTRY_NUM);
		
		PlayGame();
		InvokeRepeating("CallSimulator", 0.0f, DAY_CYCLE/2000.0f);//유니티에서 시뮬레이터 주기의 1/2주기로 호출하여 날짜가 변경될 경우 일일 주기의 유니티 작업 실행

		
		// UI Setup - non-important, only for this demo
		
		labelStyle = new GUIStyle ();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.normal.textColor = Color.white;
		labelStyleShadow = new GUIStyle (labelStyle);
		labelStyleShadow.normal.textColor = Color.black;
       
		map.CenterMap(); // Center map on the screen

		
		
	}


	//시뮬레이터의 함수 호출
    void CallSimulator()
    {

        //성능 test
        sw.Reset();
        sw.Start();
        //


        int nday;
        if (day != (nday = Today()))
        {

            if (day % 360 == 0)
            {
                OneYear();
            }
            if (day % 30 == 0)
            {
                OneMonth();
            }
            day = nday;
            OneDay();



        }



        //성능 test
        sw.Stop();
        opTimeText.text = "연산시간 : " + sw.ElapsedTicks.ToString() + "(*10^-7)";
        //






    }

	//게임 내 시간 하루주기로 실행
    void OneDay() 
    {
		
		UIMainUpdate();
        EffectFogUpdate();
		EffectSmokeUpdate();//EffectSmokeUpdate
		EventManager();
		QuestManager();



	}
	
	
	//게임 내 시간 한달주기로 실행
	void OneMonth()
    {
		
	}

	//게임 내 시간 일년 주기로 실행
	void OneYear()
    {
		player.year++;
		uinews.AddYear();
		OneYearWorldReport();
		
	}

	//Main화면의 UI를 업데이트
	public void UIMainUpdate()
	{
	
		string richColor = "";
		uimain.gold_text1.text = numFormat(GetPTGold());
		uimain.wpop_text.text = numFormat(GetWLive());
		uimain.carbon_text.text = numFormat(GetWCarbonPPM());
		uimain.temperature_text.text = numFormat(GetWTemperature());
		uimain.gold_text2.text = numFormat(GetPTGold());
		uimain.pop_text.text = numFormat(GetLive(map.countryHighlightedIndex));
		richColor = GetNeedEnergy(map.countryHighlightedIndex) > GetSupplyEnergy(map.countryHighlightedIndex) ? "<color=red>" : "<color=green>";
		uimain.energy_text.text = richColor + numFormat(GetNeedEnergy(map.countryHighlightedIndex)) + "/" + numFormat(GetSupplyEnergy(map.countryHighlightedIndex)) + "</color>";
		uimain.supRate_text.text = numFormat(GetSupport(map.countryHighlightedIndex));
		uimain.eduRate_text.text = numFormat(GetRecognition(map.countryHighlightedIndex));
		uimain.date.text = calDate(Today(), false);

		
		float t = GetWElevatedTemperature();
		uimain.temperatureSlider.value = t;
		uimain.tempSliderText.text = string.Format("{0:0.##} ℃", t);
		uimain.tempSliderImage.color = t > 1.5 ? new Color((2f - t) / 0.5f, 0, 0) : new Color(t / 1.5f, 0, (1.5f - t) / 1.5f);

	}

	//온도에 따른 안개효과 업데이트
	public void EffectFogUpdate()
	{
		float t = GetWElevatedTemperature() / 2.0f;
		fog.Color = new Color(1.0f, (2.0f - 2.0f * t * t * t * t) / 2.0f, (2.0f - 2.0f * t * t * t * t) / 2.0f);
	}

	//탄소배출량에 따른 스모그 효과 크기 업데이트
	public void EffectSmokeUpdate()
    {
		for (int i = 0; i < COUNTRY_NUM; i++)
		{
			smokeeffect[i].GetComponent<ParticleSystem>().startSize = (GetDEmission(i) / 100f > 10.0f ? 10.0f : GetDEmission(i) / 100f);
		}
		
	}

	//연간 리포트 소식 발생
	public void OneYearWorldReport()
    {
        float nWTemperature=GetWTemperature();
		float nWCarbonPPM = GetWCarbonPPM(); ;
        long nWPopulation = GetWPopulation(); ;
        int nPDGold = GetPDGold(); ;
        int nWDEmission = GetWDEmission(); ;
        int nWTEmission = GetWTEmission(); ;
        int nWNeedEnergy = GetWNeedEnergy(); ;
        int nWSupplyEnergy = GetWSupplyEnergy(); ;
        int nPSupport = GetPSupport(); ;
        int nWRecognition = GetWRecognition(); ;
		int y = player.year + (Today() / 360);
		string s = "";
		string richColor;

		s += "평균온도 : " + numFormat(nWTemperature) + "℃\n(연간 변화량 : <color=red>+" + numFormat(nWTemperature-worldreport.WTemperature) + "</color>℃)\n";

		richColor=(nWCarbonPPM-worldreport.WCarbonPPM)>0 ?"<color=red>+":"<color=green>";
		s += "탄소 농도 : " + numFormat(nWCarbonPPM) + "ppm\n(연간 변화량 : "+richColor+numFormat(nWCarbonPPM - worldreport.WCarbonPPM)+ "</color>ppm)\n";

	
		s += "인구 수 : " + numFormat(nWPopulation) + "명\n(연간 변화량 : <color=red>-" + numFormat(nWPopulation-worldreport.WPopulation) + "</color>명)\n";

		//+하루 골드 수입(평균 or 고정)

		richColor = (nWDEmission - worldreport.WDEmission) > 0 ? "<color=red>+" : "<color=green>";
		s += "일일 총 배출량 : " + numFormat(nWDEmission) + " Mton\n(연간 변화량 : "+richColor+ numFormat(nWDEmission - worldreport.WDEmission) + "</color>million ton)\n";
		//+처음과 비교하여 얼마나 변했는가

		s += "누적 총 배출량 : " + numFormat(nWTEmission) + " Mton\n";


		richColor = (nWNeedEnergy- worldreport.WNeedEnergy) > 0 ? "<color=red>+" : "<color=green>";
		s += "에너지 수요 : " + numFormat(nWNeedEnergy) + "\n(연간 변화량 : " + richColor + numFormat(nWNeedEnergy - worldreport.WNeedEnergy) + "</color>)\n";

        richColor = (nWSupplyEnergy - worldreport.WSupplyEnergy) < 0 ? "<color=red>" : "<color=green>+";
		s += "에너지 공급 : " + numFormat(nWSupplyEnergy) + "\n(연간 변화량 : " + richColor + numFormat(nWSupplyEnergy - worldreport.WSupplyEnergy) + "</color>)\n";

		richColor=nPSupport-worldreport.PSupport<0 ?"<color=red>":"<color=green>+";
		s += "지지도 : " + numFormat(nPSupport) + "\n(연간 변화량 : "+richColor+numFormat(nPSupport - worldreport.PSupport) +"</color>)\n";
		

		s += " 인식률 : " + numFormat(nWRecognition) + "\n(연간 변화량 : <color=green>+" +numFormat(nWRecognition-worldreport.WRecognition)+ "</color>)\n";
		//+얼마나 증가했는가
		
		uinews.AddNews(new News(y, 1, 1, (y - 1) + "년 연간보고서",s));
        worldreport.WTemperature = nWTemperature;
		worldreport.WCarbonPPM = nWCarbonPPM;
		worldreport.WPopulation = nWPopulation;
		worldreport.PDGold = nPDGold;
		worldreport.WDEmission = nWDEmission;
		worldreport.WTEmission = nWTEmission;
		worldreport.WNeedEnergy = nWNeedEnergy;
		worldreport.WSupplyEnergy = nWSupplyEnergy;
		worldreport.PSupport = nPSupport;
		worldreport.WRecognition = nWRecognition;

	}
	

	void Update()
    {
		//성능 test
		timer += Time.deltaTime;
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
		//
	}
	void OnGUI () {
		//성능 test
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		
		
		string tt = string.Format("{0:0.0} ms ({1:0.} fps) {2:0.0}s", msec, fps,timer);
		deltaTimeText.text = tt;
		//


		// Do autoresizing of GUI layer
		GUIResizer.AutoResize ();
        
		// Check whether a country or city is selected, then show a label
        
		if (map.countryHighlighted != null || map.cityHighlighted != null) {
            
            
		  //Vector3 mousePos = Input.mousePosition;
			string text;
			
			if (map.countryHighlighted != null) {
				
				text = map.countryHighlighted.name + " (" + map.countryHighlighted.continent + ")";
			} else {
				text = "";
			}
			float x ,y;
			if (minimizeState) {
                
                x = Screen.width - 130;
				y = Screen.height - 140;
			} else {
                
                x = Screen.width / 2.0f;
				y = Screen.height - 40;
			}
            
			GUI.Label (new Rect (x - 1, y - 1, 0, 10), text, labelStyleShadow);
			GUI.Label (new Rect (x + 1, y + 2, 0, 10), text, labelStyleShadow);
			GUI.Label (new Rect (x + 2, y + 3, 0, 10), text, labelStyleShadow);
			GUI.Label (new Rect (x + 3, y + 4, 0, 10), text, labelStyleShadow);
			GUI.Label (new Rect (x, y, 0, 10), text, labelStyle);
		}
	}
   
    

	//숫자 -> 문자열
	public string numFormat(int input)
    {
		
		return string.Format("{0:#,0}", input);
    }
	public string numFormat(long input)
	{
		
		return string.Format("{0:#,0}", input);
	}
	public string numFormat(float input)
	{
		
		return string.Format("{0:##,##0.00}", input);
	}
	
	//날짜 연,월,일로 표시 f=true일 경우 '년','월','일' 표시
	public string calDate(int _day,bool f)
    {

		int day = _day;
		int y = player.year+day / 360;
		day %= 360;
		int m = player.month+day / 30;
		day %= 30;
		int d = player.day + day+1;
		if (!f)
		{
			return string.Format("{0} {1} {2}", y, m, d);
        }
        else
        {
			return string.Format("{0}년 {1}월 {2}일", y, m, d);
		}
    }
	
	//세계 창 Text
	public string WorldDataToString()
    {
		string retstr = "";
		string richColor;
		retstr += "현재 날짜 : " + calDate(Today(), true) + "(+" + Today() + "일)\n";

		retstr += "평균온도 : " + numFormat(GetWTemperature()) + "(<color=red>+" + numFormat(GetWTemperature()-startworldval.WTemperature) + "</color>) ℃\n";

		richColor=GetWCarbonPPM()-startworldval.WCarbonPPM>0?"<color=red>+":"<color=green>";
		retstr += "탄소 농도 : " + numFormat(GetWCarbonPPM()) +"("+richColor+numFormat(GetWCarbonPPM()-startworldval.WCarbonPPM)+"</color>) ppm\n";
		

		retstr += "인구 수 : " + numFormat(GetWLive()) + "(<color=red>-" + numFormat(GetWDead()) + "</color>)명\n";


		retstr += "골드 : " + numFormat(GetPTGold()) + "\n";
		//+하루 골드 수입(평균 or 고정)

		richColor=GetWDEmission()-startworldval.WDEmission>=0 ?"<color=red>+":"<color=green>";
		retstr += "일일 총 배출량 : " + numFormat(GetWDEmission()) +"("+richColor+numFormat(GetWDEmission()-startworldval.WDEmission)+"</color>) million ton\n"; ;

		retstr+= "누적 총 배출량 : " + numFormat(GetWTEmission()) + " Mton\n";
       

		richColor =GetWNeedEnergy()>GetWSupplyEnergy()? "<color=red>":"<color=green>";
		retstr += "에너지(수요/공급) : "+richColor + numFormat(GetWNeedEnergy()) + " / " + numFormat(GetWSupplyEnergy()) + "</color>\n";

		richColor=GetPSupport()-startworldval.PSupport>=0?"<color=green>+":"<color=red>";
		retstr += "지지도 : " + numFormat(GetPSupport()) +"("+richColor+numFormat(GetPSupport()-startworldval.PSupport)+"</color>) %\n";
		
		retstr += "기후위기 인식률 : " + numFormat(GetWRecognition())+"(<color=green>+"+numFormat(GetWRecognition()-startworldval.WRecognition)+"</color>) %\n";
		
		return retstr;
		
	
	}

	//국가 창 - 개요 Text
	public string CountryOutlineToString(int _countryCode)
    {
		string retstr = "";
		string richColor;
		retstr += "현재 날짜 : " + calDate(Today(), true) + "(+" + Today() + "일)\n";

		retstr +="인구 수 :"+numFormat(GetLive(_countryCode))+"(<color=red>-" + numFormat(GetDead(_countryCode)) + "</color>)명\n";

		retstr += "누적 골드 : " + numFormat(GetTGold(_countryCode)) + "\n";

		richColor = GetDEmission(_countryCode) - startcountryval[_countryCode].DEmission > 0 ? "<color=red>+" : "<color=green>";
		retstr += "일일 탄소 배출량 : " + numFormat(GetDEmission(_countryCode)) + "(" + richColor + numFormat(GetDEmission(_countryCode) - startcountryval[_countryCode].DEmission) + "</color>) Mton\n";

		richColor=GetSupport(_countryCode)-startcountryval[_countryCode].Support>=0 ?"<color=green>+":"<color=red>";
		retstr += "지지도 : " + numFormat(GetSupport(_countryCode)) +"("+richColor+numFormat(GetSupport(_countryCode) - startcountryval[_countryCode].Support)+"</color>) %\n";
		


		retstr += "인식률 : " + numFormat(GetRecognition(_countryCode)) + "(<color=green>+"+numFormat(GetRecognition(_countryCode)-startcountryval[_countryCode].Recognition) + ")</color> %\n"; ;
		
		
	


		return retstr;
	}
	
	//국가 창 - 에너지 Text
	public string CountryEnergyToString(int _countryCode)
    {
		string retstr = "";
		string richColor;
		richColor = GetNeedEnergy(_countryCode) > GetSupplyEnergy(_countryCode) ? "<color=red>" : "<color=green>";
		retstr += "에너지(수요/공급) : " + richColor + GetNeedEnergy(_countryCode) + " / " + GetSupplyEnergy(_countryCode) + "</color>\n";

		richColor= GetFirePlants(_countryCode) - startcountryval[_countryCode].FirePlants>= 0 ? "+" : "";
		retstr += "화력 발전소 : " + GetFirePlants(_countryCode) +"("+richColor+numFormat(GetFirePlants(_countryCode)-startcountryval[_countryCode].FirePlants)+")\n";

		richColor = GetGreenPlants(_countryCode) - startcountryval[_countryCode].GreenPlants>= 0 ? "+" : "";
		retstr += "재생에너지 발전소 : " + GetGreenPlants(_countryCode)+"("+richColor+numFormat(GetGreenPlants(_countryCode)-startcountryval[_countryCode].GreenPlants)+")\n";
		

		return retstr;
	}

	//결과 창 Text
	public string ResultString()
    {
		string retstr="";
		string richColor;
		retstr += "생존 날짜 : " + calDate(Today(), true) + "(+" + Today() + "일)\n";

		retstr += "평균온도 : " + numFormat(GetWTemperature()) + "(<color=red>+" + numFormat(GetWTemperature() - startworldval.WTemperature) + "</color>) ℃\n";

		richColor = GetWCarbonPPM() - startworldval.WCarbonPPM > 0 ? "<color=red>+" : "<color=green>";
		retstr += "탄소 농도 : " + numFormat(GetWCarbonPPM()) + "(" + richColor + numFormat(GetWCarbonPPM() - startworldval.WCarbonPPM) + "</color>) ppm\n";


		retstr += "인구 수 : " + numFormat(GetWLive()) + "(<color=red>-" + numFormat(GetWDead()) + "</color>)명\n";


		retstr += "골드 : " + numFormat(GetPTGold()) + "\n";

		richColor = GetWDEmission() - startworldval.WDEmission >= 0 ? "<color=red>+" : "<color=green>";
		retstr += "일일 총 배출량 : " + numFormat(GetWDEmission()) + "(" + richColor + numFormat(GetWDEmission() - startworldval.WDEmission) + "</color>) million ton\n"; ;

		retstr += "누적 총 배출량 : " + numFormat(GetWTEmission()) + " million ton\n";


		richColor = GetWNeedEnergy() > GetWSupplyEnergy() ? "<color=red>" : "<color=green>";
		retstr += "에너지(수요/공급) : " + richColor + numFormat(GetWNeedEnergy()) + " / " + numFormat(GetWSupplyEnergy()) + "</color>\n";

		richColor = GetPSupport() - startworldval.PSupport >= 0 ? "<color=green>+" : "<color=red>";
		retstr += "지지도 : " + numFormat(GetPSupport()) + "(" + richColor + numFormat(GetPSupport() - startworldval.PSupport) + "</color>) %\n";

		retstr += "기후위기 인식률 : " + numFormat(GetWRecognition()) + "(<color=green>+" + numFormat(GetWRecognition() - startworldval.WRecognition) + "</color>) %\n";

		for(int _countryCode=0; _countryCode<COUNTRY_NUM; _countryCode++)
        {
			retstr += "\n<b> - "+map.countries[_countryCode].name + "</b>\n";
			retstr += "인구 수 :" + numFormat(GetLive(_countryCode)) + "(<color=red>-" + numFormat(GetDead(_countryCode)) + "</color>)명\n";

			retstr += "누적 골드 : " + numFormat(GetTGold(_countryCode)) + "\n";
			

			richColor = GetSupport(_countryCode) - startcountryval[_countryCode].Support >= 0 ? "<color=green>+" : "<color=red>";
			retstr += "지지도 : " + numFormat(GetSupport(_countryCode)) + "(" + richColor + numFormat(GetSupport(_countryCode) - startcountryval[_countryCode].Support) + "</color>) %\n";

			richColor = GetDEmission(_countryCode) - startcountryval[_countryCode].DEmission > 0 ? "<color=red>+" : "<color=green>";
			retstr += "일일 탄소 배출량 : " + numFormat(GetDEmission(_countryCode)) + "(" + richColor + numFormat(GetDEmission(_countryCode) - startcountryval[_countryCode].DEmission) + "</color>) Mton\n";


			retstr += "인식률 : " + numFormat(GetRecognition(_countryCode)) + "(<color=green>+" + numFormat(GetRecognition(_countryCode) - startcountryval[_countryCode].Recognition) + ")</color> %\n"; ;
			richColor = GetNeedEnergy(_countryCode) > GetSupplyEnergy(_countryCode) ? "<color=red>" : "<color=green>";
			retstr += "에너지(수요/공급) : " + richColor + GetNeedEnergy(_countryCode) + " / " + GetSupplyEnergy(_countryCode) + "</color>\n";

			richColor = GetFirePlants(_countryCode) - startcountryval[_countryCode].FirePlants >= 0 ? "+" : "";
			retstr += "화력 발전소 : " + GetFirePlants(_countryCode) + "(" + richColor + numFormat(GetFirePlants(_countryCode) - startcountryval[_countryCode].FirePlants) + ")\n";

			richColor = GetGreenPlants(_countryCode) - startcountryval[_countryCode].GreenPlants >= 0 ? "+" : "";
			retstr += "재생에너지 발전소 : " + GetGreenPlants(_countryCode) + "(" + richColor + numFormat(GetGreenPlants(_countryCode) - startcountryval[_countryCode].GreenPlants) + ")\n";

		}
		return retstr;
    }
	
	
	
	public void OnApplicationQuit()
	{
		//Debug.Log("QUIT");
		
		ExitGame();
		EndGame();


	}

	//게임 종료 시 시뮬레이터 호출을 중지
	public void ExitGame()
    {
		CancelInvoke();
		

	}
	public bool EventManager()
    {
		bool ret = false;

		//승리조건
		if (Today() >= ENDING_VICTORY_DATE)
		{
			Ending(0);
		}
		//인구 일정 수 이하일 경우 엔딩
		else if (GetWLive()<=ENDING_DEFEAT_DEADNUM)
        {
			Ending(1);
        }

		//지지도 일정 수 이하일 경우 엔딩
		else if (GetPSupport()<=ENDING_DEFEAT_SUPPORTNUM)
        {
			Ending(2);
        }
		//온도에 따른 경고
        switch (alarm)
        {
			case 0:
                if (GetWTemperature() - startworldval.WTemperature >= 0.5f)
                {
					uinews.AddNews(new News(player.year + day / 360, player.month + day / 30, player.day + day + 1, "<color=yellow>경고! 평균온도 0.5도상승</color>", "<color=yellow>경고! 평균온도 0.5도상승</color>\n사망자가 발생합니다."));
					ActivePopUp("<color=yellow>경고! 평균온도 0.5도상승</color>", "사망자가 발생합니다.");
					alarm++;
                }
				break;
			case 1:
				if (GetWTemperature() - startworldval.WTemperature >= 1.0f)
				{
					uinews.AddNews(new News(player.year + day / 360, player.month + day / 30, player.day + day + 1, "<color=orange>경고! 평균온도 1도상승</color>", "<color=orange>경고! 평균온도 1도상승</color>\n사망자 발생이 더욱 증가합니다."));
					ActivePopUp("<color=orange>경고! 평균온도 1도상승</color>", "사망자 발생이 더욱 증가합니다.");
					alarm++;
				}
				break;
			case 2:
				if (GetWTemperature() - startworldval.WTemperature >= 1.5f)
				{
					uinews.AddNews(new News(player.year + day / 360, player.month + day / 30, player.day + day + 1, "<color=red>경고! 평균온도 1.5도상승</color>", "<color=red>경고! 평균온도 1.5도상승</color>\n사망자 발생이 심각하게 증가합니다."));
					ActivePopUp("<color=red>경고! 평균온도 1.5도상승</color>", "사망자 발생이 심각하게 증가합니다.");
					alarm++;
				}
				break;
        }

		//송신 대기중인 소식들 송신 시 리스트에서 삭제
		Stack<int> toRemove = new Stack<int>();
		for(int i=0; i<uinews.newslist.Count; i++)
        {
			
			int d = (uinews.newslist[i].year - 2020) * 360 + (uinews.newslist[i].month - 1) * 30 + uinews.newslist[i].day;
            if (d <= Today())
            {
				uinews.AddNews(uinews.newslist[i]);
				toRemove.Push(i);
            }
			
        }


        while (toRemove.Count > 0)
        {
            ActivePopUp("새 소식 알림", uinews.newslist[toRemove.Peek()].name);
            uinews.newslist.RemoveAt(toRemove.Pop());

        }

        return ret;

	}

	//퀘스트 진행도 업데이트
	public void QuestManager()
    {
        if (uiquest.selectedQuest != null)
        {
			uiquest.QuestUpdate(uiquest.selectedQuest);
		}
    }

	//엔딩 발생
	public void Ending(int code)
    {
		
		ExitGame();
		StartCoroutine(CoEnding(4.0f, code));

		
	}

	//팝업 발생
	public void ActivePopUp(string title, string content)
    {
		popup_title.text = title;
		popup_content.text = content;
		popup_panel.SetActive(true);
    }

	//팝업 닫기 버튼
	public void BtnPopupCloseOnClick()
    {
		popup_panel.SetActive(false);
    }

	//엔딩 발생 시 time동안 페이드 아웃 효과 후 code에 맞는 엔딩화면 발생
	IEnumerator CoEnding(float time,int code)
    {
		Color tempColor = EndingPanel.GetComponent<Image>().color;
		ResultPanel.SetActive(false);
		EndingText.gameObject.SetActive(false);
		EndingTextPanel.SetActive(false);
		EndingPanel.SetActive(true);
		while (tempColor.a < 1.0f)
        {

			tempColor.a += Time.deltaTime / time;
			EndingPanel.GetComponent<Image>().color = tempColor;
			
			yield return null;
        }
		GameObject child = EndingPanel.transform.GetChild(code).gameObject;
		
		tempColor = child.GetComponent<Image>().color;
		child.GetComponent<Image>().color = new Color(tempColor.r, tempColor.g, tempColor.b, 0f);
		tempColor = child.GetComponent<Image>().color;
		
		child.gameObject.SetActive(true);
		while (tempColor.a < 1.0f)
		{

			tempColor.a += Time.deltaTime / time;
			child.GetComponent<Image>().color = tempColor;

			yield return null;
		}
		string richVictory = "<color=green><size=80><b>승리</b></size></color>\n";
		string richDefeat = "<color=red><size=80><b>패배</b></size></color>\n";
		switch (code)
		{
			case 0:
				EndingText.text =richVictory+"축하합니다. 당신은 " + calDate(ENDING_VICTORY_DATE, true) + "까지 생존하였습니다.";
				break;
			case 1:
				EndingText.text = richDefeat + "당신은 기후위기에 대처하지 못했습니다.";
				break;
			case 2:
				EndingText.text = richDefeat + "당신은 충분한 지지를 받지 못했습니다.";
				break;
		}
		EndingTextPanel.SetActive(true);
		EndingText.gameObject.SetActive(true);
		yield return new WaitForSeconds(3.0f);
		EndingText.gameObject.SetActive(false);
		EndingTextPanel.SetActive(false);
		ResultPanel.SetActive(true);
		ResultText.text = ResultString();
	}

}

