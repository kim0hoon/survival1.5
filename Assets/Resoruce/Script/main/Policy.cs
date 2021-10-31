/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
*/
public class Policy
{
    public string name;//정책 이름
    public string content;//정책 설명
    public int type;//정책 타입 0 : 슬라이더 형식,  1: on/off 형식
    public float d;//슬라이더 형식일 경우 증감 터치시 변화량
    public int cost;//정책시행비용
    public int code;//정책 코드
    public int refund;//환불골드
    public int supply;//에너지 공급량
    public int emission;//탄소 배출량
    public delegate int Delegate_oneInt(int _countryCode);
    public delegate int Delegate_twoInt(int _countryCode, int num);
    public Delegate_oneInt nval;
    public Delegate_twoInt build;
    public Delegate_twoInt destroy;
    public Delegate_twoInt count;
    public Delegate_oneInt costf;
    public Delegate_oneInt effect;
    public Delegate_twoInt enforce;
    public Delegate_oneInt needRecognition;
    public Policy(int type, string name, string content, float d,int cost,int refund,int supply,int emission, Delegate_oneInt nval, Delegate_twoInt build,Delegate_twoInt destroy)
    {
        //정책타입= 0 : 에너지정책
        //정책타입,이름,내용,슬라이더 버튼 변화량, 정책시행비용, 환불골드, 에너지 공급량, 탄소배출량, 현재 발전소 개수 함수, 발전소 건설 함수, 발전소 폐쇄 함수
        this.name = name; 
        this.content = content;
        this.type = type;     
        this.d = d;
        this.cost = cost;
        this.refund = refund;
        this.supply = supply;
        this.emission = emission;
        this.nval = nval;
        this.build = build;
        this.destroy = destroy;
    }
    public Policy(int type,  int code,string name, string content, Delegate_twoInt count,Delegate_oneInt costf,Delegate_oneInt effect, Delegate_twoInt enforce)
    {
        //정책타입 = 1 : 교육정책
        //정책타입,이름,내용,정책코드,시행횟수,정책시행비용,효과,정책 시행 함수
        this.type = type;
        this.name = name;
        this.content = content;
        this.code = code;
        this.count = count;
        this.costf = costf;
        this.effect = effect;
        this.enforce = enforce;
    }

    public Policy(int type, int code, string name, string content, Delegate_twoInt count, Delegate_oneInt costf, Delegate_oneInt effect, Delegate_twoInt enforce, Delegate_oneInt needRecognition)
    {
        //정책타입 = 2 : 생활정책
        //정책타입,이름,내용,정책코드,시행횟수,비용,효과,정책 시행 함수,필요 인식률 함수
        this.type = type;
        this.name = name;
        this.content = content;
        this.code = code;
        this.count = count;
        this.costf = costf;
        this.effect = effect;
        this.enforce = enforce;
        this.needRecognition = needRecognition;
    }
}

