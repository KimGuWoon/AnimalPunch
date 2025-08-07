using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    // 전역적으로 사용자 데이터를 저장하고 공유하는 역할의 클래스임
    // MonoBehaviour 상속 필요없음
    // static(정적 변수)을 사용함, 씬 이동간에도 데이터가 유지됨.
    public static string userID = "admin";
    public static string userPW = "1234";
    public static string nickName = "";
    public static int charIndex = 99;
    public static bool isPlayerWinner = false;

}
