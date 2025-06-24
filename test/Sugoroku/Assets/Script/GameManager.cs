using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // シングルトンパターン

    public Stone[] players; // 全プレイヤーのStoneコンポーネントを格納
    public int currentPlayerIndex = 0; // 現在のプレイヤーのインデックス

    void Awake()
    {
        // シングルトンインスタンスの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄されないようにする場合
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 次のプレイヤーのターンに進むメソッド
    public void NextPlayerTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        Debug.Log("現在のプレイヤー: " + (currentPlayerIndex + 1) + "P");
    }
}