using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Stone : MonoBehaviour
{

    public Route currentRoute;
    public int routePosition;
    public int steps;
    public bool isMoving;
    public bool isRollingDice;

    public int currentPoints = 0;
    public int pointsToWin = 3; //勝利ポイント
    public string winSceneName = "Finish";
    // 各マスをジャンプするのにかかる時間
    public float jumpDurationPerNode = 0.5f;
    // ジャンプの高さ
    public float jumpPower = 1f;
    // ジャンプの回数
    public int numJumps = 1;

    public GameObject speechBubblePanel; // 吹き出しのパネル
    public TextMeshProUGUI bubbleText; // 吹き出しのテキスト
    public float bubbleDisplayDuration = 2.0f; // 吹き出しの表示時間

    private Camera mainCamera; //メインカメラへの参照

    public GameObject dicePrefab;
    public Vector3 diceSpawnPosition = new Vector3(8, 8, 0); // サイコロが出現する位置
    public float diceThrowForce = 300f; // サイコロを投げる力

    private GameObject currentDiceInstance;

    public int turnsToSkip = 0; // スキップするターンの数

    public void SetDiceResultAndMove(int resultSteps)
    {
        if (isMoving)
        {
            Debug.LogWarning("駒が移動中にサイコロの結果を受け取りました。");
            return;
        }
        //isRollingDice = false; // サイコロのロールが完了した
        steps = resultSteps; // Dice_checkerから受け取った値を steps に設定
        Debug.Log("サイコロの出た目は: " + steps + "マス");
        StartCoroutine(Move());
    }

    void Start()
    {
        // ゲーム開始時に駒を最初のマスの中心に配置
        if (currentRoute != null && currentRoute.childNodeList.Count > 0)
        {
            transform.position = currentRoute.childNodeList[0].position; // マスの中心に設定
            routePosition = 0; // 最初のマスのインデックスを0に設定
        }

        if (speechBubblePanel != null)
        {
            speechBubblePanel.SetActive(false);
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("シーンに 'MainCamera' タグが付けられたカメラが見つかりません。");
        }
    }

    void Update()
    {

    }

    public void RollDice()
    {
        if (dicePrefab == null)
        {
            Debug.LogError("Dice Prefab が設定されていません。");
            isRollingDice = false; // エラーなのでフラグをリセット
            return;
        }

        isRollingDice = true;

        Debug.Log("ダイスロール開始！");

        currentDiceInstance = GameObject.Instantiate(dicePrefab) as GameObject; // 生成したインスタンスを変数に保持
        currentDiceInstance.transform.position = diceSpawnPosition;

        // 生成されたサイコロのDice_checkerコンポーネントを取得し、PlayerController を渡す
        Dice_checker diceChecker = currentDiceInstance.GetComponent<Dice_checker>();
        if (diceChecker != null)
        {
            diceChecker.playerController = this; // ここで自身の参照を渡す
            diceChecker.ResetDiceChecker(); // サイコロの状態をリセット
        }
        else
        {
            Debug.LogWarning("生成されたサイコロのプレハブに Dice_checker コンポーネントが見つかりません。");
            isRollingDice = false; // エラーなのでフラグをリセット
            return;
        }

        // サイコロにランダムな回転と力を加える
        int rotateX = Random.Range(0, 360);
        int rotateY = Random.Range(0, 360);
        int rotateZ = Random.Range(0, 360);

        Rigidbody diceRb = currentDiceInstance.GetComponent<Rigidbody>();
        if (diceRb != null)
        {
            diceRb.AddForce(-transform.right * diceThrowForce); // 駒の右方向の逆向きに力を加える
            currentDiceInstance.transform.Rotate(rotateX, rotateY, rotateZ); // 初期回転を設定
            diceRb.AddTorque(Random.insideUnitSphere * 100f); // ランダムな回転力を追加
        }
        else
        {
            Debug.LogWarning("生成されたサイコロに Rigidbody コンポーネントが見つかりません。");
        }
    }

    IEnumerator Move()
    {
        if (isMoving)
        {
            yield break;
        }
        isMoving = true;

        HideSpeechBubble(); // 移動開始時に吹き出しを隠す

        while (steps > 0)
        {
            routePosition = (routePosition + 1) % currentRoute.childNodeList.Count; //周回
            Vector3 nextPos = currentRoute.childNodeList[routePosition].position; //移動

            // DOTweenのアニメーション目標位置をマスの中心に設定
            yield return transform.DOJump(nextPos, jumpPower, numJumps, jumpDurationPerNode).SetEase(Ease.OutQuad).WaitForCompletion();

            transform.position = nextPos; // マスの中心

            if (steps > 0) // まだ移動が残っている場合 (通過マス)
            {
                Transform passedNodeTransform = currentRoute.childNodeList[routePosition];
                NodeEffect passedNodeEffect = passedNodeTransform.GetComponent<NodeEffect>();

                if (passedNodeEffect != null && passedNodeEffect.applyOnPass)
                {
                    currentPoints += passedNodeEffect.passPointChange;
                    Debug.Log(this.name + "がマスを通過！ " + passedNodeEffect.passPointChange + "ポイントゲット！ 現在のポイント: " + currentPoints);
                    // 通過時のメッセージ表示は、短い通知か、後でまとめて表示することを検討
                    // ここで吹き出しを表示すると、移動が途切れる可能性がある
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.UpdateAllPlayerPointsUI(); // 通過時にUIを更新
                    }
                }

                if (passedNodeEffect.changeSceneOnPass && currentPoints >= passedNodeEffect.requiredPointsToPass)
                {
                    Debug.Log(this.name + "がポイント条件を満たして通過マスに到達！シーンを切り替えます: " + passedNodeEffect.targetSceneNameOnPass);
                    // シーン遷移を行う前に、現在のアニメーションや処理を停止
                    yield return new WaitForSeconds(1f); //1秒待つ
                    DOTween.KillAll(); // 全てのDOTweenアニメーションを停止
                    SceneManager.LoadScene(passedNodeEffect.targetSceneNameOnPass);
                    yield break;
                }
            }

            steps--;
        }

        isMoving = false;

        yield return StartCoroutine(ApplyNodeEffect(routePosition)); // マスの効果を表示

        //CheckWinCondition(); // 勝利条件をチェック

        /*yield return new WaitForSeconds(1f);
        DestroyDice(); // サイコロを消す*/
    }

    IEnumerator ApplyNodeEffect(int currentPosIndex)
    {
        // 吹き出しパネルやテキストが設定されていない場合は、すぐにコルーチンを終了
        if (speechBubblePanel == null || bubbleText == null)
        {
            Debug.LogWarning("SpeechBubblePanel または BubbleText が設定されていません。");
            // 吹き出しが表示できない場合でもターンは進める
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndPlayerTurn();
            }
            yield break;
        }

        bubbleText.color = new Color(1f, 1f, 0f, 1f); // 例: 黄色 (RGBA: 1.0, 1.0, 0.0, 1.0)

        Image panelImage = speechBubblePanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color(0f, 1f, 1f, 0.7f); //効果の背景
        }
        else
        {
            Debug.LogWarning("SpeechBubblePanel に Image コンポーネントが見つかりません。色を変更できません。");
        }

        Transform currentNodeTransform = currentRoute.childNodeList[currentPosIndex];
        NodeEffect nodeEffect = currentNodeTransform.GetComponent<NodeEffect>();

        if (nodeEffect != null)
        {
            bubbleText.text = nodeEffect.effectDescription;
            if (nodeEffect.effectType == NodeEffect.EffectType.AddPoints)
            {
                currentPoints += nodeEffect.pointChange;
                bubbleText.text += "\n" + nodeEffect.pointChange + "ポイントゲット！ \n現在のポイント: " + currentPoints;
                Debug.Log("ポイントゲット！現在のポイント: " + currentPoints);
            }
            else if (nodeEffect.effectType == NodeEffect.EffectType.LosePoints)
            {
                currentPoints -= nodeEffect.pointChange;
                if (currentPoints < 0)
                {
                    currentPoints = 0;
                }
                bubbleText.text += "\n" + Mathf.Abs(nodeEffect.pointChange) + "ポイント失った！ 現在のポイント: " + currentPoints;
                Debug.Log("ポイント失った！現在のポイント: " + currentPoints);
            }
            else if (nodeEffect.effectType == NodeEffect.EffectType.SkipTurn)
            {
                turnsToSkip += nodeEffect.turnSkipCount;
            }
            else if (nodeEffect.effectType == NodeEffect.EffectType.Teleport) // テレポート
            {
                int targetIndex = nodeEffect.teleportTargetNodeIndex;
                if (targetIndex >= 0 && targetIndex < currentRoute.childNodeList.Count)
                {
                    Vector3 targetPosition = currentRoute.childNodeList[targetIndex].position;
                    bubbleText.text += "\n" + (targetIndex + "マスに移動");
                    Debug.Log("テレポート！ 目標マス: " + targetIndex);
                    yield return new WaitForSeconds(1f); //1秒待つ
                    // テレポートアニメーション
                    yield return transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();
                    routePosition = targetIndex; // 現在のルート位置を更新
                    transform.position = targetPosition; // 位置を固定
                }
                else
                {
                    Debug.LogWarning("テレポート先のノードインデックスが不正です: " + targetIndex);
                }
            }
            else if (nodeEffect.effectType == NodeEffect.EffectType.SwapPositions) // 位置入れ替え
            {
                if (GameManager.Instance != null)
                {
                    Stone furthestPlayer = GameManager.Instance.GetFurthestPlayer(this); // 自分以外の最も進んでいるプレイヤーを取得
                    if (furthestPlayer != null)
                    {
                        // 自分の現在の位置を保存
                        int originalMyPosition = this.routePosition;
                        Vector3 originalMyWorldPosition = this.transform.position;

                        // 最も進んでいるプレイヤーの現在の位置を保存
                        int originalFurthestPlayerPosition = furthestPlayer.routePosition;
                        Vector3 originalFurthestPlayerWorldPosition = furthestPlayer.transform.position;

                        // 自分の駒を最も進んでいるプレイヤーの位置へ移動
                        yield return this.transform.DOMove(originalFurthestPlayerWorldPosition, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();
                        this.routePosition = originalFurthestPlayerPosition;
                        this.transform.position = originalFurthestPlayerWorldPosition; // 位置を固定

                        // 最も進んでいるプレイヤーの駒を自分の元々の位置へ移動
                        yield return furthestPlayer.transform.DOMove(originalMyWorldPosition, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();
                        furthestPlayer.routePosition = originalMyPosition;
                        furthestPlayer.transform.position = originalMyWorldPosition; // 位置を固定

                        bubbleText.text += $"\n{furthestPlayer.name}と位置を入れ替えた！";
                        Debug.Log($"{this.name}が{furthestPlayer.name}と位置を入れ替えました。");
                    }
                    else
                    {
                        bubbleText.text += "\n入れ替わるプレイヤーがいません。";
                        Debug.LogWarning("位置を入れ替える相手のプレイヤーが見つかりませんでした。");
                    }
                }
                else
                {
                    Debug.LogError("GameManagerのインスタンスが見つかりません！");
                }
            }
            else if (nodeEffect.effectType == NodeEffect.EffectType.None) // 効果なし
            {
                // 何もしない
            }

            speechBubblePanel.SetActive(true);
            // 吹き出し表示後にターンを終了させるため、HideBubbleAfterDelayを呼ぶ
            StartCoroutine(HideBubbleAfterDelay(bubbleDisplayDuration));
        }
        else
        {
            HideSpeechBubble(); // 吹き出しを隠す（もし表示中なら）
            Debug.Log("このマスにはNodeEffectコンポーネントがアタッチされていません。");
            // NodeEffectがない場合は吹き出しを表示しないので、すぐにターンを終了させる
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndPlayerTurn();
            }
        }
    }

    /*void CheckWinCondition()
    {
        if (currentPoints >= pointsToWin)
        {
            Debug.Log("勝利！");
            SceneManager.LoadScene(winSceneName);
        }
    }*/

    IEnumerator HideBubbleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideSpeechBubble();

        // 吹き出しが完全に消えた後にターンを終了させる
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndPlayerTurn();
        }
    }

    void HideSpeechBubble()
    {
        if (speechBubblePanel != null)
        {
            speechBubblePanel.SetActive(false);
        }
    }

    public void DestroyDice()
    {
        isRollingDice = false; // サイコロのロールが完了した
        if (currentDiceInstance != null)
        {
            Destroy(currentDiceInstance, 0.5f);
            currentDiceInstance = null;
        }
    }

    public void DisplaySkipTurnMessage()
    {
        if (speechBubblePanel == null || bubbleText == null) return;

        Image panelImage = speechBubblePanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color(1f, 0.5f, 0f, 0.7f); // 休みの背景色（オレンジ）
        }
        bubbleText.color = new Color(1f, 1f, 1f, 1f); // 白字に
        bubbleText.text = "1回休み！\n残り " + turnsToSkip + " 回休み";
        speechBubblePanel.SetActive(true);
        // 休みメッセージ表示後も吹き出しが消えてからターンを終了させるため、HideBubbleAfterDelayを呼ぶ
        StartCoroutine(HideBubbleAfterDelay(bubbleDisplayDuration));
    }
}