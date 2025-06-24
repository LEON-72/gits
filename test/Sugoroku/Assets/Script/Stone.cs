using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Stone : MonoBehaviour
{

    public Route currentRoute;

    int routePosition;

    public int steps;

    bool isMoving;

    // 各マスをジャンプするのにかかる時間
    public float jumpDurationPerNode = 0.5f;
    // ジャンプの高さ
    public float jumpPower = 1f;
    // ジャンプの回数
    public int numJumps = 1;

    public GameObject speechBubblePanel; // 吹き出しのパネル
    public TextMeshProUGUI bubbleText; // 吹き出しのテキスト
    public float bubbleDisplayDuration = 2.0f; // 吹き出しの表示時間

    void Start()
    {
        // ゲーム開始時に駒を最初のマスに配置
        if (currentRoute != null && currentRoute.childNodeList.Count > 0)
        {
            transform.position = currentRoute.childNodeList[0].position;
            routePosition = 0;
        }

        if (speechBubblePanel != null)
        {
            speechBubblePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isMoving)
        {
            steps = Random.Range(1, 7);
            Debug.Log("出た目は" + steps);//サイコロを振った時のログ
            StartCoroutine(Move());

            //if (routePosition + steps < currentRoute.childNodeList.Count)
            //{
            //    StartCoroutine(Move());
            //}
        }
    }

    IEnumerator Move()
    {
        if (isMoving)
        {
            yield break;
        }
        isMoving = true;

        HideSpeechBubble();

        while (steps > 0)
        {
            routePosition = (routePosition + 1) % currentRoute.childNodeList.Count;
            Vector3 nextPos = currentRoute.childNodeList[routePosition].position;

            //while (MoveToNextNode(nextPos)) { yield return null; }

            //DOTweenを使ったジャンプ移動
            yield return transform.DOJump(nextPos, jumpPower, numJumps, jumpDurationPerNode).SetEase(Ease.OutQuad).WaitForCompletion();
            steps--;
            //routePosition++;
        }

        isMoving = false;

        DisplayNodeEffect(routePosition);
    }
    
    void DisplayNodeEffect(int currentPosIndex)
    {
        if (speechBubblePanel == null || bubbleText == null)
        {
            Debug.LogWarning("SpeechBubblePanel または BubbleText が設定されていません。");
            return;
        }

        // 現在止まっているマスのGameObjectを取得
        Transform currentNodeTransform = currentRoute.childNodeList[currentPosIndex];
        // そのGameObjectからNodeEffectコンポーネントを取得
        NodeEffect nodeEffect = currentNodeTransform.GetComponent<NodeEffect>();

        if (nodeEffect != null)
        {
            // 吹き出しに効果テキストを設定
            bubbleText.text = nodeEffect.effectDescription;
            // 吹き出しを駒の少し上に移動（調整してください）
            speechBubblePanel.transform.position = transform.position + Vector3.up * 2f; 
            // 吹き出しを表示
            speechBubblePanel.SetActive(true);

            // 一定時間後に吹き出しを非表示にするコルーチンを開始
            StartCoroutine(HideBubbleAfterDelay(bubbleDisplayDuration));
        }
        else
        {
            // NodeEffectがアタッチされていないマスに止まった場合
            HideSpeechBubble();
            Debug.Log("このマスにはNodeEffectコンポーネントがアタッチされていません。");
        }
    }

    IEnumerator HideBubbleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideSpeechBubble();
    }

    void HideSpeechBubble()
    {
        if (speechBubblePanel != null)
        {
            speechBubblePanel.SetActive(false);
        }
    }
   // bool MoveToNextNode(Vector3 goal)
    // {
    //return goal != (transform.position = Vector3.MoveTowards(transform.position, goal, 2f * Time.deltaTime));
    // }
}
