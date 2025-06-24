using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // シーン内のメインカメラのTransformを取得
        mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        // 吹き出しのY軸は固定しつつ、カメラの方向を向くようにする
        // これにより、吹き出しが上下に傾くのを防ぎ、常に地面と平行に回転します。
        Vector3 lookAtTarget = mainCameraTransform.position;
        lookAtTarget.y = transform.position.y; // Y軸を固定

        transform.LookAt(lookAtTarget);

        // 必要であれば、Z軸（吹き出しの「手前」方向）を調整するために180度回転させる
        // 吹き出しのモデルがどちらを向いて作られているかによります
        transform.Rotate(0, 180, 0); 
    }
}