using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 플레이어 Class
 */
public class Player : MonoBehaviour
{
 
     public int gold;//플레이어 보유 골드
     public int readershipCard;//플레이어 보유 리더쉽 카드
     public int year;//연도
     public int month;//월
     public int day;//일
    
  
     public Player()
     {
        
    
        gold = 0;
        readershipCard = 0;
        year = 2019;
        month = 1;
        day = 0;
     }
    
    
}
