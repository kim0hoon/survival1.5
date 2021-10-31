using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 소식 Class
 */
public class News
{
    public int year;
    public int month;
    public int day;
    public string name;
    public string content;
  

    public News(int year, int month, int day, string name, string content)
    {
        this.year = year;
        this.month = month;
        this.day = day;
        this.name = name;
        this.content = content;
    
        
    }
  
}
