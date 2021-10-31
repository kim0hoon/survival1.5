using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
/*
 * 소식 창
 */
public class UINews : MonoBehaviour
{
    public GameObject Main;

    public GameObject news_window;

    //연도 선택
    public GameObject news_year_list;//연도 선택 리스트
    public GameObject news_year_prefab;

    //뉴스 선택
    public GameObject news_select_panel;//연도 선택 시 퀘스트 선택하는 패널
    public GameObject news_select_prefab;
    public GameObject news_prefab;

    //뉴스 내용
    public GameObject news_content_window;//뉴스 클릭 시 내용을 띄워주는 창
    public Text news_content_title;
    public Text news_content_text;
    public Button btn_news_content_delete;
   

    float btn_year_height;
    float btn_news_height;
    public struct NEWS_SET //연도버튼과 해당 연도에 따른 스크롤 뷰
    {
        public GameObject btn_year;
        public GameObject now_year_scrollView;
       
    }
    Game game;
    NEWS_SET selected_set;//현재 뉴스창에서 선택된 연도버튼 및 스크롤뷰
    NEWS_SET last_set;//현재연도의 버튼 및 스크롤뷰
    // public Text news_content;
    public List<News> newslist;
    private const string libName = "SV_Simulator_v1";
    [DllImport(libName)]
    public static extern int Today();


    void Awake()
    {

        
        game = Main.GetComponent<Game>(); 
        btn_year_height= news_year_prefab.GetComponent<RectTransform>().rect.height;
        btn_news_height=news_prefab.GetComponent<RectTransform>().rect.height;
        news_content_window.SetActive(false);
        news_content_text.supportRichText = true;
        news_content_title.supportRichText = true;

        NewsListInit();
    }
    
    //news_window 닫는 버튼
    public void BtnSelectCloseOnClick()
    {
        news_window.SetActive(false);
    }


    //news_year_panel에서 연도 터치 시 해당 연도의 뉴스 내용 패널 활성화
    public void BtnNewsYearSelectOnClick(NEWS_SET ns)
    {
        
        //현재 다른 뉴스 내용 패널이 활성화 되있는 경우 비활성화 시킴
        if (selected_set.now_year_scrollView != null)
        {
            selected_set.now_year_scrollView.SetActive(false);
        }
        ns.now_year_scrollView.SetActive(true);
        selected_set = ns;
        
    }


    //news_select_panel에서 뉴스를 클릭 시 뉴스 내용창(news_content_window) 활성화
    public void BtnNewsOnClick(News news,GameObject target)
    {
       
        news_content_title.text = news.name;
        
        news_content_text.text =news.year.ToString() + "년 " + news.month.ToString() + "월 " + news.day.ToString() + "일\n" + news.content;
        news_content_text.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, news_content_text.preferredHeight);

        news_content_window.SetActive(true);


       
        btn_news_content_delete.transform.GetComponent<Button>().onClick.AddListener(delegate () { BtnContentDeleteOnClick(target); });

    }


    //news_content_panel의 뉴스 삭제 버튼
    public void BtnContentDeleteOnClick(GameObject target)
    {

        Destroy(target);
        BtnContentCloseOnClick();
    }

    //news_content_panel 닫기 버튼
    public void BtnContentCloseOnClick()
    {
        
        btn_news_content_delete.transform.GetComponent<Button>().onClick.RemoveAllListeners();
        news_content_window.SetActive(false);
    }


    //연도가 바뀔 때 뉴스 연도 추가
    public void AddYear()
    {
        //연도선택 버튼 생성
        NEWS_SET news_set=new NEWS_SET();   
        news_set.btn_year = Instantiate(news_year_prefab);     
        news_set.btn_year.transform.SetParent(news_year_list.transform,false);
        news_set.btn_year.transform.GetChild(0).GetComponent<Text>().text=game.player.year.ToString();
        news_set.btn_year.transform.GetComponent<Button>().onClick.AddListener(delegate() { BtnNewsYearSelectOnClick(news_set); });
        //


        //소식선택 스크롤뷰 생성
        news_set.now_year_scrollView = Instantiate(news_select_prefab);      
        news_set.now_year_scrollView.transform.SetParent(news_select_panel.transform,false);
        news_set.now_year_scrollView.SetActive(false);
        last_set = news_set;
        //
     


    }


    //현재년도에 뉴스 추가
    public void AddNews(News news)
    {
       
        GameObject news_select_content = last_set.now_year_scrollView.transform.GetChild(0).transform.GetChild(0).gameObject;
        GameObject btn_news = Instantiate(news_prefab);        
        btn_news.transform.SetParent(news_select_content.transform, false);     
        btn_news.transform.GetChild(0).GetComponent<Text>().text = "수신 : "+game.calDate(Today(), true)+"\n"+news.name;
        btn_news.transform.GetComponent<Button>().onClick.AddListener(delegate () { BtnNewsOnClick(news,btn_news); });
    }

    //게임 시작 시 추가되는 뉴스
    public void NewsListInit()
    {
     
        newslist = new List<News>();
        string name;
        string content;
        //0번
        name = "수몰 위기 몰디브의 처절한 '수중 회의'";
        content = "지구 온난화가 시작되면서 평균 해발고도가 2.1미터에 불과한 몰디브에 위기가 찾아왔습니다.\n" +
            "지구가 계속 뜨거워질 경우, 앞으로 90년 뒤에는 이 몰디브 공화국은거주 지역 대부분이 물에 잠길 것으로 환경 전문가들은 예측하고 있습니다\n" +
            "실제 남태평양에 있는 섬나라 투발루에선 해변 마을들이 물에 잠기면서 많은 주민들이 목숨을 잃어 머지않아 몰디브에 찾아올 재앙을 보는 듯 했습니다.";
        newslist.Add(new News(2009, 10, 19, name, content));

        //1번
        name = "과학자들 충격 경고 기후위기 '티핑 포인트' 이미 지났다";
        content = "지구 평균기온이 산업혁명 전보다 섭씨 1.5도 이상 오를 경우를 인간이 지구 기후를 통제하기 불가능해지는 티핑 포인트로 지적했다.\n " +
            "아울러 이들은 2030년까지 1.5도 목표를 달성하지 못한다면 인류는 티핑 포인트에 도달하리라고 내다봤다\n." +
            "지구 스스로 온실가스를 내뿜는 상황(티핑 포인트)이 온다면 인류가 기후를 통제하기란 불가능해지리라고 경고했다.\n";
        newslist.Add(new News(2020, 2, 15, name, content));


        //2번
        name = "빙하 녹아내리고, 잔디 자라고…심각한 남극 상황";
        content = " 세종기지 하계 시즌이라고 볼 수 있는 12월에서 3월 평균기온을 보면 영상 1~2도에 있습니다. " +
            "이 기간 세종기지에는 비도 자주 내리기 때문에 겨울 동안 쌓였던 눈들이 녹아 해안가를 중심으로 맨땅이 드러나는 시기입니다. " +
            "올해 이 기간 평균기온이 과거보다 1도 정도 높았습니다. 그러다보니까 눈 녹음과 맨땅이 드러나는 정도가 다른 해보다 좀 더 심했던 모습이었습니다. " +
            "기지 주변에 펼쳐진 만년설의 면적이 점점 적어지는 것은 사실입니다. 세종기지에서 빙하가 녹는 모습을 볼 수 있는 곳은 기지 앞의 마리안 소만이라는 작은 만입니다. " +
            "이곳에 커다란 빙벽이 있는데요. 세종기지가 설립된 초창기에는 기지에서 2.8km 거리에 있었는데요. 지난 30년 동안 그 빙벽이 약 1.5km 정도 이상 깎여 녹아 없어져서 현재는 기지에서 약 4.3km 떨어진 곳에 빙벽이 위치하고 있습니다.";
        newslist.Add(new News(2020,9,17,name,content));

     


    }
   
   
}
