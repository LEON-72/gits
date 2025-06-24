using UnityEngine;

public class CircleArranger : MonoBehaviour
{
    [Header("配置するオブジェクト")]
    public GameObject objectToPlace; // 円状に配置したいオブジェクトのプレハブまたは既存のオブジェクト

    [Header("設定")]
    [Range(3, 100)] // 配置するオブジェクトの数を制限（最小3、最大100）
    public int numberOfObjects = 12; // 配置するオブジェクトの数
    public float radius = 5f; // 円の半径
    public Vector3 centerOffset = Vector3.zero; // 円の中心のオフセット
    public float startAngleOffset = 0f; // 開始角度のオフセット（度）
    public bool lookAtCenter = false; // オブジェクトを円の中心に向かせるか

    // エディタ上での変更を即座に反映させるため
    void OnValidate()
    {
        // オブジェクトが設定されているか確認
        if (objectToPlace == null)
        {
            Debug.LogWarning("配置するオブジェクトが設定されていません。'Object To Place' フィールドに設定してください。", this);
            return;
        }

        // 既存の子オブジェクトをすべて削除して再配置
        ClearExistingObjects();
        ArrangeObjectsInCircle();
    }

    // ゲーム実行時にも配置したい場合 (通常はOnValidateで十分)
    void Start()
    {
        // OnValidateで既に配置されている場合は不要だが、念のため
        // ClearExistingObjects();
        // ArrangeObjectsInCircle();
    }

    // 以前に配置された子オブジェクトをすべて削除する
    void ClearExistingObjects()
    {
        // transformの子オブジェクトをすべて削除
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            // DestroyImmediate を使うことで、エディタモードで即座にオブジェクトを削除します。
            // 実行時であれば Destroy を使います。
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    // オブジェクトを円状に配置するメインロジック
    void ArrangeObjectsInCircle()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            // 各オブジェクトの角度を計算
            // 360度をオブジェクトの数で割り、現在のインデックスを掛ける
            float angle = i * (360f / numberOfObjects) + startAngleOffset;
            float rad = angle * Mathf.Deg2Rad; // 角度をラジアンに変換

            // 円上のXとZ座標を計算 (UnityのデフォルトはY軸が上方向のため)
            float x = centerOffset.x + radius * Mathf.Cos(rad);
            float z = centerOffset.z + radius * Mathf.Sin(rad);
            float y = centerOffset.y; // Y座標は中心のYオフセットと同じ

            Vector3 position = new Vector3(x, y, z);

            // オブジェクトのインスタンスを生成
            // 親をこのスクリプトがアタッチされているGameObjectに設定することで、
            // Hierarchyが整理され、一括管理しやすくなる
            GameObject newObject = Instantiate(objectToPlace, position, Quaternion.identity);
            newObject.transform.parent = this.transform;
            newObject.name = objectToPlace.name + "_" + i; // オブジェクト名を分かりやすくする

            // オブジェクトを円の中心に向かせる
            if (lookAtCenter)
            {
                // 中心点からオブジェクトへのベクトルを計算
                // このスクリプトがアタッチされているオブジェクトの位置 + 中心オフセットが円の中心
                Vector3 directionToCenter = (transform.position + centerOffset) - newObject.transform.position;
                // Y軸の回転のみを考慮し、上下の傾きをなくす
                directionToCenter.y = 0;
                if (directionToCenter != Vector3.zero) // ゼロベクトルでないことを確認
                {
                    newObject.transform.rotation = Quaternion.LookRotation(directionToCenter);
                }
            }
        }
    }

    // シーンビューでの可視化用 (ギズモ表示)
    // エディタ上でスクリプトが選択されているときに円のプレビューを表示します
    void OnDrawGizmos()
    {
        // 円のギズモの色を設定
        Gizmos.color = Color.yellow;
        // 円の中心にワイヤーフレームの球を表示して、円の中心と半径を視覚化
        Gizmos.DrawWireSphere(transform.position + centerOffset, radius);

        // 配置されるオブジェクトのプレビューポイントを表示 (OnValidateで実際に生成されるため、ここでは円自体のみ)
        if (objectToPlace != null && numberOfObjects > 0)
        {
            for (int i = 0; i < numberOfObjects; i++)
            {
                float angle = i * (360f / numberOfObjects) + startAngleOffset;
                float rad = angle * Mathf.Deg2Rad;

                float x = centerOffset.x + radius * Mathf.Cos(rad);
                float z = centerOffset.z + radius * Mathf.Sin(rad);
                float y = centerOffset.y;

                Vector3 position = new Vector3(x, y, z);

                // 各オブジェクトが配置されるポイントに小さな球を表示
                Gizmos.DrawSphere(transform.position + position, 0.1f);
            }
        }
    }
}