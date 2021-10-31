using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 퀘스트 Class
 */
public class Quest
{
   
  
    public string name;//퀘스트 이름
    public string content;//퀘스트 내용


   
    public delegate int Delegate_zeroInt();
    public delegate int Delegate_oneInt(int _countryCode);
    public delegate int Delegate_twoInt(int _countryCode, int num);



    public int needInt;//퀘스트 요구량
    public int f;// -1 : 감소요구, 1 : 증가요구



    //퀘스트 진행도 호출하는 함수
    public Delegate_zeroInt zeroIntval;
    public Delegate_oneInt oneIntval;
    public Delegate_twoInt twoIntval;
    public int p1;//매개변수 1
    public int p2;//매개변수 2
    public int parameternum=-1;//매개변수 개수

    public int reward;

    //퀘스트 요구가 0개의 매개변수로 호출해지는 경우
    public Quest(string name, string content, int needInt, int f,Delegate_zeroInt zeroIntval,int reward)
    {
        this.name = name;
        this.content = content;
        this.needInt = needInt;
        this.zeroIntval = zeroIntval;
        this.f = f;
        this.reward = reward;
        parameternum = 0;
    }

    //퀘스트 요구가 1개의 매개변수로 호출해지는 경우
    public Quest(string name, string content,int needInt, int f,Delegate_oneInt oneIntval, int p1,int reward)
    {
        this.name = name;
        this.content = content;
        this.needInt = needInt;
        this.oneIntval = oneIntval;
        this.f = f;
        this.p1 = p1;
        this.reward = reward;
        parameternum = 1;
    }

    //퀘스트 요구가 2개의 매개변수로 호출해지는 경우
    public Quest(string name, string content,int needInt, int f, Delegate_twoInt twoIntval,int p1, int p2,int reward)
    {
        this.name = name;
        this.content = content;
        this.needInt = needInt;
        this.twoIntval = twoIntval;
        this.f = f;
        this.p1 = p1;
        this.p2 = p2;
        this.reward = reward;
        parameternum = 2;
    }
    

}
