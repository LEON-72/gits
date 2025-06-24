using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera; // メインカメラへの参照

    void Start()
    {
        // シーン内のメインカメラを取得
        // メインカメラのTagが"MainCamera"であることを確認してください
        mainCamera = Camera.main; 

        if (mainCamera == null)
        {
            Debug.LogError("シーンに 'MainCamera' タグの付いたカメラが見つかりません。");
        }
    }

    void LateUpdate()
    {
        // メインカメラが存在する場合のみ処理を実行
        if (mainCamera != null)
        {
            // 吹き出しのTransformをカメラのY軸回転に合わせてZ軸を0に保ちながら回転させる
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}